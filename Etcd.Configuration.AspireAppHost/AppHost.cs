var builder = DistributedApplication.CreateBuilder(args);

var etcd = builder.AddEtcd("etcdtest")
    ;
builder.AddProject<Projects.Etcd_Configuration_TestWeb>("etcd-configuration-testweb")
    .WithReference(etcd);

builder.Build().Run();

public class EtcdResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    public ReferenceExpression ConnectionStringExpression
    {
        get
        {
            if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var annotation))
            {
                return annotation.Resource.ConnectionStringExpression;
            }
            else
            {
                return ReferenceExpression.Create($"{this.GetEndpoint("http").Url}");
            }
        }
    }
}

static class EtcdResourceExtension
{
    public static IResourceBuilder<EtcdResource> AddEtcd(this IDistributedApplicationBuilder builder,
        string name)
    {
        var rsrc = new EtcdResource(name);
        return builder.AddResource(rsrc)
            .WithImage("coreos/etcd")
            .WithImageRegistry("quay.io")
            .WithImageTag("v3.6.8")
            .WithHttpEndpoint(name: "http", port: 2379, targetPort: 2379)
            .WithUrlForEndpoint("http", annot =>
            {
                annot.DisplayText = "http";
            })
            .WithArgs("etcd", "-name", name, "-advertise-client-urls", "http://localhost:2380", "-listen-client-urls", "http://0.0.0.0:2379");
            ;
    }
}