var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("barkfest-sql")
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithDataVolume("barkfest-sql-data");

var storage = builder.AddAzureStorage("barkfest-storage")
                     .RunAsEmulator(e => e
                         .WithLifetime(ContainerLifetime.Persistent)
                         .WithDataVolume("barkfest-blobs-data"));

var blobs = storage.AddBlobs("barkfest-blobs");

var api = builder.AddProject<Projects.Barkfest_API>("barkfest-api")
                 .WithReference(sql)
                 .WithReference(blobs)
                 .WaitFor(sql)
                 .WaitFor(blobs);

builder.AddViteApp("barkfest-ui", "../../barkfest-ui")
       .WithPnpm()
       .WithHttpEndpoint(port: 5173, isProxied: false)
       .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("https"))
       .WaitFor(api);

builder.Build().Run();
