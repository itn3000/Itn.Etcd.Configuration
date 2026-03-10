using dotnet_etcd;
using Etcdserverpb;
using Grpc.Core;
using R3;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using EtcdEventNames = Etcd.Configuration.Definitions.EventNames;
using ActivityNames = Etcd.Configuration.Definitions.ActivityNames;

namespace Etcd.Configuration
{
    internal class EtcdConfigurationProvider : ConfigurationProvider
    {
        string rootKey;
        EtcdClient? etcdClient;
        static readonly ActivitySource activitySource = new ActivitySource(Definitions.ProviderDiagnosticName);
        Task WatchTask;
        Subject<Unit> subject;
        DisposableBag disposables;
        EtcdClientFactory etcdClientFactory;
        SemaphoreSlim clientSemaphore;
        SemaphoreSlim loadSemaphore;
        long? _watchId = null;
        DateTimeOffset lastLoad;
        ConcurrentDictionary<long, CancellationTokenSource> ChangeTokens = new();
        static long ChangeTokenKeySeed = 0;
        public EtcdConfigurationProvider(string key, EtcdClientFactory clientFactory)
        {
            rootKey = key;
            etcdClient = null;
            etcdClientFactory = clientFactory;
            disposables = new DisposableBag();
            subject = new Subject<Unit>();
            lastLoad = DateTimeOffset.MinValue;
            subject.ThrottleLast(TimeSpan.FromSeconds(1))
                .SubscribeAwait(async (unit, ct) =>
                {
                    await LoadAsync(ct);
                }, maxConcurrent: 1)
                .AddTo(ref disposables);
            ChangeToken.OnChange(() =>
            {
                var cts = new CancellationTokenSource();
                var token = new CancellationChangeToken(cts.Token);
                var val = Interlocked.Increment(ref ChangeTokenKeySeed);
                ChangeTokens[val] = cts;
                return token;
            },
            () =>
            {
                subject.OnNext(Unit.Default);
            }).AddTo(ref disposables);
            clientSemaphore = new SemaphoreSlim(1, 1);
            disposables.Add(clientSemaphore);
            loadSemaphore = new SemaphoreSlim(1, 1);
            disposables.Add(loadSemaphore);
            WatchTask = BeginWatch();
        }
        CancellationTokenSource terminate = new();
        public void Dispose()
        {
            using var act = activitySource.StartActivity(ActivityNames.Dispose);
            if (act != null)
            {
                act.AddTag("root_key", rootKey);
            }
            try
            {
                terminate?.Cancel();
                WatchTask.Wait();
            }
            catch (Exception e)
            {
                if (act != null)
                {
                    act.AddException(e, [new("name", EtcdEventNames.WaitTaskError)]);
                    act.SetStatus(ActivityStatusCode.Error);
                }
            }
            try
            {
                disposables.Dispose();
                etcdClient?.Dispose();
                terminate?.Dispose();
            }
            catch (Exception e)
            {
                if (act != null)
                {
                    act.AddException(e, [new("name", EtcdEventNames.DisposeClientError)]);
                    act.SetStatus(ActivityStatusCode.Error);
                }
            }
        }
        async ValueTask LoadAsync(CancellationToken ct)
        {
            using var act = activitySource.StartActivity(ActivityNames.LoadAsync);
            if (act != null)
            {
                act.AddTag("root_key", rootKey);
            }
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var prevUpdate = lastLoad;
                    await loadSemaphore.WaitAsync(ct);
                    try
                    {
                        if (lastLoad != prevUpdate)
                        {
                            // update by another thread already
                            if (act != null)
                            {
                                act.AddEvent(new ActivityEvent(EtcdEventNames.UpdateByAnother));
                            }
                            break;
                        }
                        etcdClient = etcdClient ?? etcdClientFactory.CreateClient();
                        var res = await etcdClient.GetRangeAsync(rootKey, cancellationToken: ct);
                        var newdic = new Dictionary<string, string?>();
                        foreach (var kv in res.Kvs)
                        {
                            var key = kv.Key.ToStringUtf8().Substring(rootKey.Length);
                            if (!string.IsNullOrEmpty(key))
                            {
                                newdic[kv.Key.ToStringUtf8().Substring(rootKey.Length)] = kv.Value?.ToStringUtf8();
                            }
                        }
                        Data = newdic;
                        lastLoad = DateTimeOffset.Now;
                        OnReload();
                        break;
                    }
                    catch (Exception e)
                    {
                        if (act != null)
                        {
                            act.AddException(e);
                            act.SetStatus(ActivityStatusCode.Error, "LoadAsyncError");
                        }
                    }
                    finally
                    {
                        loadSemaphore.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                    if (act != null)
                    {
                        act.AddEvent(new ActivityEvent("Canceled"));
                    }
                    break;
                }
            }
        }
        public override void Load()
        {
            using var act = activitySource.StartActivity(ActivityNames.Load);
            if (act != null)
            {
                act.AddTag("root_key", rootKey);
            }

            try
            {
                var prevUpdate = lastLoad;
                loadSemaphore.Wait(terminate.Token);
                try
                {
                    if (prevUpdate != lastLoad)
                    {
                        // update by another thread
                        if (act != null)
                        {
                            act.AddEvent(new ActivityEvent(EtcdEventNames.UpdateByAnother));
                        }
                        return;
                    }
                    etcdClient = etcdClient ?? etcdClientFactory.CreateClient();
                    var res = etcdClient.GetRange(rootKey);
                    var newdic = new Dictionary<string, string?>();
                    foreach (var kv in res.Kvs)
                    {
                        newdic[kv.Key.ToStringUtf8().Substring(rootKey.Length)] = kv.Value?.ToStringUtf8();
                    }
                    Data = newdic;
                    lastLoad = DateTimeOffset.Now;
                    OnReload();
                }
                catch (Exception e)
                {
                    if (act != null)
                    {
                        act.AddException(e, [new("name", EtcdEventNames.LoadError)]);
                        act.SetStatus(ActivityStatusCode.Error);
                    }
                }
                finally
                {
                    loadSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                if (act != null)
                {
                    act.AddEvent(new ActivityEvent(EtcdEventNames.Cancel));
                }
            }
        }
        public override bool TryGet(string key, out string? value)
        {
            return base.TryGet(key, out value);
        }
        void RenewClient()
        {
            using var act = activitySource.StartActivity(ActivityNames.RenewClient);
            if (act != null)
            {
                act.AddTag("root_key", rootKey);
            }
            var oldClient = etcdClient;
            if (!clientSemaphore.Wait(TimeSpan.FromSeconds(1)))
            {
                if (act != null)
                {
                    act.SetStatus(ActivityStatusCode.Error, "SemaphoreTimeout");
                }
                return;
            }
            try
            {
                if (etcdClient != oldClient)
                {
                    // already updated by another thread
                    if (act != null)
                    {
                        act.AddEvent(new ActivityEvent(EtcdEventNames.ClientAlreadyUpdate));
                    }
                    return;
                }
                try
                {
                    etcdClient = etcdClientFactory.CreateClient();
                    oldClient?.Dispose();
                    _watchId = null;
                }
                catch (Exception e)
                {
                    if (act != null)
                    {
                        act.AddException(e, tags: [new("name", EtcdEventNames.RenewClientError)]);
                        act.SetStatus(ActivityStatusCode.Error);
                    }
                }
            }
            finally
            {
                clientSemaphore.Release();
            }
        }
        void WatchRangeCallback(WatchResponse res)
        {
            using var act = activitySource.StartActivity(ActivityNames.WatchDetected);
            if (act != null)
            {
                act.AddTag("watch_id", res.WatchId);
                act.AddTag("root_key", rootKey);
            }
            if (res.Events.Count > 0 || res.Created)
            {
                List<long> tokens = new();

                foreach (var (k, cts) in ChangeTokens)
                {
                    cts.Cancel();
                    tokens.Add(k);
                }
                foreach (var token in tokens)
                {
                    if (ChangeTokens.TryRemove(token, out var cts))
                    {
                        cts.Dispose();
                    }
                }
            }
        }
        async Task BeginWatch()
        {
            // IWatchManager? watchManager = null;
            while (!terminate.IsCancellationRequested)
            {
                {
                    using var act = activitySource.StartActivity(ActivityNames.CheckWatch);
                    if (act != null)
                    {
                        act.AddTag("root_key", rootKey);
                    }
                    try
                    {
                        etcdClient = etcdClient ?? etcdClientFactory.CreateClient();
                        if (!_watchId.HasValue)
                        {
                            _watchId = await etcdClient.WatchRangeAsync(rootKey, res =>
                            {
                                WatchRangeCallback(res);
                            }, cancellationToken: terminate.Token);
                        }
                        if(act != null)
                        {
                            act.AddTag("watch_id", _watchId.Value);
                        }
                        var statusRequest = new StatusRequest();
                        var res = await etcdClient.StatusAsync(statusRequest, cancellationToken: terminate.Token);
                        if(act != null)
                        {
                            act.AddEvent(new ActivityEvent(EtcdEventNames.Status, tags: [
                                new("version", res.Version),
                                new("db_size", res.DbSize),
                                new("watch_id", _watchId),
                                new("root_key", rootKey),
                                new("current_tokens_count", ChangeTokens.Count)
                                ]));
                        }
                    }
                    catch (RpcException rpcException)
                    {
                        if(act != null)
                        {
                            act.AddException(rpcException, [new("name", EtcdEventNames.WatchError)]);
                            act.SetStatus(ActivityStatusCode.Error);
                        }
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), terminate.Token);
                        }
                        catch { }
                        RenewClient();
                    }
                    catch (OperationCanceledException)
                    {
                        if(act != null)
                        {
                            act.AddEvent(new ActivityEvent(EtcdEventNames.OperationCancel));
                        }
                    }
                    catch (Exception e)
                    {
                        if(act != null)
                        {
                            act.AddException(e, [new("name", EtcdEventNames.WatchError)]);
                            act.SetStatus(ActivityStatusCode.Error);
                        }
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), terminate.Token);
                        }
                        catch { }
                    }
                }
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), terminate.Token);
                }
                catch { }
            }
            if (_watchId.HasValue && etcdClient != null)
            {
                using var act = activitySource.StartActivity(ActivityNames.CancelWatch);
                try
                {
                    etcdClient?.CancelWatch(_watchId.Value);
                }
                catch (Exception e)
                {
                    if (act != null)
                    {
                        act.AddException(e, [new("name", EtcdEventNames.WatchError)]);
                        act.SetStatus(ActivityStatusCode.Error);
                    }
                }
            }
        }

    }
}
