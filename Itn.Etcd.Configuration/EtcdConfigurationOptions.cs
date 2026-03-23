using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;

namespace Itn.Etcd.Configuration
{
    public record class EtcdConfigurationOptions(string RootKey, 
        string Urls = "http://localhost:2379", 
        string User = "", 
        string Password = "",
        string ServerName = "etcd-server",
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
