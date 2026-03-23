using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Itn.Etcd.Configuration
{
    class EtcdConfigurationSource(EtcdConfigurationOptions options, EtcdClientFactory etcdClientFactory) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EtcdConfigurationProvider(options.RootKey, etcdClientFactory, TimeSpan.FromSeconds(options.CheckStatusIntervalSec), TimeSpan.FromSeconds(options.LoadThrottleSec));
        }
    }
}
