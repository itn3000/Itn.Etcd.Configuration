using System;
using System.Collections.Generic;
using System.Text;
using dotnet_etcd;
using Etcdserverpb;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Etcd.Configuration.AspireTest
{

    public class KeyValueTest
    {
        [Test]
        public async Task InsertTest()
        {
            Environment.SetEnvironmentVariable("http_proxy", "");
            Environment.SetEnvironmentVariable("https_proxy", "");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Itn_Etcd_Configuration_AspireAppHost>(cts.Token);
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
            {
                var rangeRequest = new RangeRequest() { Key = ByteString.CopyFromUtf8("MyOptions:X") };
                var rangeResponse = await etcdClient.GetAsync(rangeRequest, cancellationToken: cts.Token);
                foreach (var item in rangeResponse.Kvs)
                {
                    TestContext.Out.WriteLine($"key = {item.Key.ToStringUtf8()}, value = {item.Value.ToStringUtf8()}");
                }
                using var res = await httpClient.GetAsync("/", cts.Token);
                res.EnsureSuccessStatusCode();
                var actual = await res.Content.ReadAsStringAsync(cts.Token);
                TestContext.Out.WriteLine(actual);
                Assert.That(actual == "test");
            }
            await etcdClient.PutAsync(new PutRequest(putRequest) { Key = ByteString.CopyFromUtf8("MyOptions:X"), Value = ByteString.CopyFromUtf8("abc") }, cancellationToken: cts.Token);
            {
                var rangeRequest = new RangeRequest() { Key = ByteString.CopyFromUtf8("MyOptions:X") };
                var rangeResponse = await etcdClient.GetAsync(rangeRequest, cancellationToken: cts.Token);
                foreach(var item in rangeResponse.Kvs)
                {
                    TestContext.Out.WriteLine($"key = {item.Key.ToStringUtf8()}, value = {item.Value.ToStringUtf8()}");
                }
            }
            await Task.Delay(1000, cts.Token);
            {
                using var res = await httpClient.GetAsync("/", cts.Token);
                res.EnsureSuccessStatusCode();
                var actual = await res.Content.ReadAsStringAsync(cts.Token);
                TestContext.Out.WriteLine($"value = {actual}");
                Assert.That(actual == "abc");
            }
            await app.StopAsync();
        }
    }
}
