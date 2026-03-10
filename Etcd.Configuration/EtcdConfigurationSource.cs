using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Etcd.Configuration
{
    class EtcdConfigurationSource(string key, EtcdClientFactory etcdClientFactory) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EtcdConfigurationProvider(key, etcdClientFactory);
        }
    }
}
