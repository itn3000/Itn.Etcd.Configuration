using dotnet_etcd;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;

namespace Etcd.Configuration
{
    /// <summary>
    /// Factory for EtcdClient
    /// </summary>
    /// <param name="url">etcd server address</param>
    /// <param name="user">username for credential</param>
    /// <param name="password">password for credential</param>
    /// <param name="serverName">server name</param>
    /// <param name="configureChannelOptions">configuring gRPC channel options</param>
    /// <param name="configureSslOptions">configuring TLS options</param>
    class EtcdClientFactory(string url, string user = "", string password = "", string serverName = "etcd-server", Action<GrpcChannelOptions>? configureChannelOptions = null, Action<SslClientAuthenticationOptions>? configureSslOptions = null)
    {
        public Action<GrpcChannelOptions>? ConfigureChannelOptions
        {
            get => configureChannelOptions;
            set => configureChannelOptions = value;
        }
        public Action<SslClientAuthenticationOptions>? ConfigureSslOptions
        {
            get => configureSslOptions;
            set => configureSslOptions = value;
        }
        public EtcdClient CreateClient()
        {
            if (configureSslOptions != null)
            {
                var client = new EtcdClient(url, configureSslOptions: configureSslOptions, configureChannelOptions: configureChannelOptions, serverName: serverName);
                if (!string.IsNullOrEmpty(user))
                {
                    client.SetCredentials(user, password);
                }
                return client;
            }
            else
            {
                if (!string.IsNullOrEmpty(user))
                {
                    return new EtcdClient(url, user, password, serverName: serverName, configureChannelOptions: configureChannelOptions);
                }
                else
                {
                    return new EtcdClient(url, configureChannelOptions: configureChannelOptions, serverName: serverName);
                }
            }
        }
    }
}
