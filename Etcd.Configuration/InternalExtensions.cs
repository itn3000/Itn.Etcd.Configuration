using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Etcd.Configuration
{
    internal static class InternalExtensions
    {
        public static EtcdClientFactory CreateClientFactory(this EtcdConfigurationOptions options)
        {
            return new EtcdClientFactory(options.Urls, user: options.User, password: options.Password, serverName: options.ServerName,
                configureChannelOptions: options.ConfigureChannelOptions, configureSslOptions: options.ConfigureSslOptions);
        }
    }
}
