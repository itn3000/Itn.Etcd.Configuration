using dotnet_etcd;
using Etcdserverpb;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Etcd.Configuration.AspireTest
{
    public class ErrorTest
    {
        [Test]
        public async Task Reboot()
        {
            Environment.SetEnvironmentVariable("http_proxy", "");
            Environment.SetEnvironmentVariable("https_proxy", "");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Etcd_Configuration_AspireAppHost>(cts.Token);
            appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
            });
            using var app = await appHost.BuildAsync().WaitAsync(cts.Token);
            await app.StartAsync();
            await app.ResourceNotifications.WaitForResourceAsync("etcdtest", cancellationToken: cts.Token);
            var connectionString = await app.GetConnectionStringAsync("etcdtest", cts.Token);
            Assert.IsNotNull(connectionString);
            using var etcdClient = new EtcdClient(connectionString);
            var putRequest = new PutRequest()
            {
                Key = ByteString.CopyFromUtf8("MyOptions:X"),
                Value = ByteString.CopyFromUtf8("test")
            };
            await etcdClient.PutAsync(putRequest, cancellationToken: cts.Token);
            await Task.Delay(200, cts.Token);
            using var httpClient = app.CreateHttpClient("etcd-configuration-testweb");
            await AssertOptionValue(etcdClient, "MyOptions:X", "test", httpClient, cts.Token);
            await app.ResourceCommands.ExecuteCommandAsync("etcdtest", KnownResourceCommands.StopCommand, cancellationToken: cts.Token);
            await app.ResourceNotifications.WaitForResourceAsync("etcdtest", KnownResourceStates.Exited, cancellationToken: cts.Token);
            await app.ResourceCommands.ExecuteCommandAsync("etcdtest", KnownResourceCommands.StartCommand, cancellationToken: cts.Token);
            await app.ResourceNotifications.WaitForResourceAsync("etcdtest", KnownResourceStates.Running, cancellationToken: cts.Token);
            await etcdClient.PutAsync(new PutRequest() { Key = ByteString.CopyFromUtf8("MyOptions:X"), Value = ByteString.CopyFromUtf8("abc") }, cancellationToken: cts.Token);
            await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
            await AssertOptionValue(etcdClient, "MyOptions:X", "abc", httpClient, cts.Token);
        }
        async Task AssertOptionValue(EtcdClient etcdClient, string key, string expected, HttpClient httpClient, CancellationToken cancellationToken)
        {
            var rangeRequest = new RangeRequest() { Key = ByteString.CopyFromUtf8(key) };
            var rangeResponse = await etcdClient.GetAsync(rangeRequest, cancellationToken: cancellationToken);
            foreach (var item in rangeResponse.Kvs)
            {
                TestContext.Out.WriteLine($"key = {item.Key.ToStringUtf8()}, value = {item.Value.ToStringUtf8()}");
            }
            using var res = await httpClient.GetAsync("/", cancellationToken);
            res.EnsureSuccessStatusCode();
            var actual = await res.Content.ReadAsStringAsync(cancellationToken);
            TestContext.Out.WriteLine($"actual = {actual},expected = {expected}");
            Assert.That(actual == expected);
        }
    }
}
