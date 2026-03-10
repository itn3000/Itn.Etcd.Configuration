using System;
using System.Collections.Generic;
using System.Text;

namespace Etcd.Configuration
{
    public record class EtcdConfigurationOptions(string RootKey, 
        string Url = "http://localhost:2379", 
        string User = "", 
        string Password = "",
        string ServerName = "etcd-server"
        )
    {
        public EtcdConfigurationOptions() : this("")
        { }
    }
}
