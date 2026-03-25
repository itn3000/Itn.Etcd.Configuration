using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;

namespace Itn.Etcd.Configuration
{
    /// <summary>
    /// etcd client options
    /// </summary>
    /// <param name="RootKey">etcd key search key prefix</param>
    /// <param name="Urls">comma separated etcd server url, like "https://example1.com:2379,https://example2.com:2379"</param>
    /// <param name="User">etcd auth user name</param>
    /// <param name="Password">etcd auth password</param>
    /// <param name="ServerName">server name</param>
    /// <param name="KeySeparator">etcd key separator for configuration section</param>
    /// <param name="ConfigureChannelOptions">configurator grpc channel options</param>
    /// <param name="ConfigureSslOptions">configurator tls connection options</param>
    /// <param name="CheckStatusIntervalSec">etcd server connection check interval</param>
    /// <param name="LoadThrottleSec">load throttling time</param>
    /// <remarks>detailed connection spec is under https://github.com/shubhamranjan/dotnet-etcd/blob/main/docs/client-initialization/index.md</remarks>
    public record class EtcdConfigurationOptions(string RootKey,
        string Urls = "http://localhost:2379",
        string User = "",
        string Password = "",
        string ServerName = "etcd-server",
        char KeySeparator = ':',
        Action<GrpcChannelOptions>? ConfigureChannelOptions = null,
        Action<SslClientAuthenticationOptions>? ConfigureSslOptions = null,
        int CheckStatusIntervalSec = 30,
        int LoadThrottleSec = 1
        )
    {
        public EtcdConfigurationOptions() : this("")
        { }
    }
}
