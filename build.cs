#:sdk Cake.Sdk@6.1.1

var configuration = Argument("configuration", "Debug");
var target = Argument("target", "Default");
Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    ;
Task("Test")
    .Does(() =>
    {
        var project = Directory("Itn.Etcd.Configuration.AspireTest").Path.CombineWithFilePath("Itn.Etcd.Configuration.AspireTest.csproj");
        var buildlogdir = Directory("artifacts").Path.Combine("buildlog").Combine(configuration);
        var buildlog = buildlogdir.CombineWithFilePath("test.binlog");
        DotNetTest(project.FullPath, new DotNetTestSettings()
        {
            Configuration = configuration,
            MSBuildSettings = new DotNetMSBuildSettings()
            {
                BinaryLogger = new MSBuildBinaryLoggerSettings()
                {
                    Enabled = true,
                    FileName = buildlog.FullPath
                }
            }
        });
    });
Task("Pack")
    .Does(() =>
    {
        var project = Directory("Itn.Etcd.Configuration").Path.CombineWithFilePath("Itn.Etcd.Configuration.csproj");
        var buildlogdir = Directory("artifacts").Path.Combine("buildlog");
        var buildlog = buildlogdir.CombineWithFilePath("pack.binlog");
        var setting = new DotNetPackSettings()
        {
            Configuration = configuration,
            MSBuildSettings = new DotNetMSBuildSettings()
            {
                BinaryLogger = new MSBuildBinaryLoggerSettings()
                {
                    Enabled = true,
                    FileName = buildlog.FullPath
                },
                Verbosity = DotNetVerbosity.Normal
            }
        };
        DotNetPack(project.FullPath, setting);
    });
RunTarget(target);