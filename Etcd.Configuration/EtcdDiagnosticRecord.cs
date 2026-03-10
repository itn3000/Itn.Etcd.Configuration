using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Etcd.Configuration
{
    internal class EtcdDiagnosticRecord
    {
    }
    public record class EtcdErrorRecord(Exception Exception, string? Key = null);
    public record class EtcdWatchGrpcErrorRecord(RpcException Exception, long? WatchId, string? Key = null);
    public record class EtcdWatchErrorRecord(Exception Exception, long? WatchId, string? Key = null);
}
