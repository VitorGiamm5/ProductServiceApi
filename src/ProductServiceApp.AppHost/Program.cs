var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.ProductServiceApp_Api>("product-api");

builder.AddProject<Projects.ProductServiceApp_Web>("product-web")
    .WithEnvironment("ProductApi__BaseAddress", "http://product-api")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
