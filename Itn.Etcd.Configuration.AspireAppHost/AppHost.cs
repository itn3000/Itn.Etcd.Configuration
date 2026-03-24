var builder = DistributedApplication.CreateBuilder(args);

var etcd = builder.AddEtcd("etcdtest")
    ;
builder.AddProject<Projects.Itn_Etcd_Configuration_TestWeb>("etcd-configuration-testweb")
    .WithReference(etcd)
    .WaitFor(etcd);

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
            .WithHttpEndpoint(name: "http", targetPort: 2379)
            .WithEnvironment("http_proxy", "")
            .WithEnvironment("https_proxy", "")
            .WithUrlForEndpoint("http", annot =>
            {
                annot.DisplayText = "http";
            })
            .WithArgs("etcd", "-name", name, "-advertise-client-urls", "http://localhost:2380", "-listen-client-urls", "http://0.0.0.0:2379")
            ;
    }

}

