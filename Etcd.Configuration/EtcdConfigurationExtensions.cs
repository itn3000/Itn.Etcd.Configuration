using Etcd.Configuration;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Extensions.Configuration
{
    public static class EtcdConfigurationExtensions
    {
        public static IConfigurationBuilder AddEtcd(this IConfigurationBuilder builder,
            string rootKey = "",
            string urls = "http://localhost:2379",
            string user = "",
            string password = "",
            string serverName = "my-etcd-server",
            Action<GrpcChannelOptions>? configureChannelOptions = null,
            Action<SslClientAuthenticationOptions>? configureSslOptions = null,
            int checkStatusIntervalSec = 30,
            int loadThrottleSec = 1
            )
        {
            var options = new EtcdConfigurationOptions(rootKey, Urls: urls, User: user, Password: password,
                ServerName: serverName, ConfigureChannelOptions: configureChannelOptions, ConfigureSslOptions: configureSslOptions,
                CheckStatusIntervalSec: checkStatusIntervalSec, LoadThrottleSec: loadThrottleSec);
            return AddEtcd(builder, options);
        }
        public static IConfigurationBuilder AddEtcd(this IConfigurationBuilder builder,
            EtcdConfigurationOptions options)
        {
            var clientFactory = options.CreateClientFactory();
            var source = new EtcdConfigurationSource(options, clientFactory);
            builder.Add(source);
            return builder;
        }
    }
}
