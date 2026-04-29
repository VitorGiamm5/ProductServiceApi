var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.ProductServiceApp_Api>("product-api");

builder.AddProject<Projects.ProductServiceApp_Web>("product-web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
