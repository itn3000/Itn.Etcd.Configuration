using Grpc.Net.Client;
using Itn.Etcd.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Extensions.Configuration
{
    public static class EtcdConfigurationExtensions
    {
        /// <summary>
        /// add etcd configuration provider
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="rootKey">etcd search key prefix</param>
        /// <param name="urls">comma separated etcd connection url, like https://example1.com:2379,https://example2.com:2379</param>
        /// <param name="user">etcd auth user name</param>
        /// <param name="password">etcd auth password</param>
        /// <param name="serverName">server name</param>
        /// <param name="keySeparator">key separator, replace to ':' when loading</param>
        /// <param name="configureChannelOptions">grpc channel options</param>
        /// <param name="configureSslOptions">tls settings, use if you want to skip cert validation</param>
        /// <param name="checkStatusIntervalSec">connection check interval</param>
        /// <param name="loadThrottleSec">loading throttling time</param>
        /// <remarks>detailed connection spec is under https://github.com/shubhamranjan/dotnet-etcd/blob/main/docs/client-initialization/index.md</remarks>
        /// <returns></returns>
        public static IConfigurationBuilder AddEtcd(this IConfigurationBuilder builder,
            string rootKey = "",
            string urls = "http://localhost:2379",
            string user = "",
            string password = "",
            string serverName = "my-etcd-server",
            char keySeparator = ':',
            Action<GrpcChannelOptions>? configureChannelOptions = null,
            Action<SslClientAuthenticationOptions>? configureSslOptions = null,
            int checkStatusIntervalSec = 30,
            int loadThrottleSec = 1
            )
        {
            var options = new EtcdConfigurationOptions(rootKey, Urls: urls, User: user, Password: password,
                ServerName: serverName, KeySeparator: keySeparator, ConfigureChannelOptions: configureChannelOptions, ConfigureSslOptions: configureSslOptions,
                CheckStatusIntervalSec: checkStatusIntervalSec, LoadThrottleSec: loadThrottleSec);
            return AddEtcd(builder, options);
        }
        /// <summary>
        /// add etcd configuratin provider
        /// </summary>
        /// <param name="builder">configuration builder</param>
        /// <param name="options">options</param>
        /// <seealso cref="EtcdConfigurationOptions"/>
        /// <returns></returns>
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
