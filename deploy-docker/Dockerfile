FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source

COPY src/ src/

RUN dotnet publish src/ProductServiceApp.Api/ProductServiceApp.Api.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# useradd no lugar de adduser (compatível com a imagem base)
RUN useradd -m appuser && \
    chown -R appuser:appuser /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=9005 \
    ConnectionStrings__PostgresWrite="Server=localhost;Port=5432;Database=dbproducts;Username=randandan;Password=randandan_XLR;" \
    ConnectionStrings__PostgresRead="Server=localhost;Port=5433;Database=dbproducts;Username=read_randandan;Password=read_randandan_XLR;" \
    DatabaseSettings__EnableLoadBalancing=true \
    WorkersReplicas__ReplicasCreateProduct=1 \
    WorkersReplicas__ReplicasUpdateProduct=1 \
    WorkersReplicas__ReplicasDeleteProduct=1 \
    WorkersReplicas__ReplicasGetAllProduct=1 \
    WorkersReplicas__ReplicasGetByIdProduct=1 \
    WorkersReplicas__ReplicasCreateOrder=1 \
    WorkersReplicas__ReplicasUpdateOrder=1 \
    WorkersReplicas__ReplicasDeleteOrder=1 \
    WorkersReplicas__ReplicasGetAllOrder=1 \
    WorkersReplicas__ReplicasGetByIdOrder=1 \
    QueueCapacity__QueueCreateProduct=100 \
    QueueCapacity__QueueUpdateProduct=100 \
    QueueCapacity__QueueDeleteProduct=100 \
    QueueCapacity__QueueCreateOrder=100 \
    QueueCapacity__QueueUpdateOrder=100 \
    QueueCapacity__QueueDeleteOrder=100 \
    RetryPolicy__MaxRetryAttempts=3 \
    RetryPolicy__DelayBetweenRetriesInSeconds=2 \
    KeepAliveTimeoutSeconds=30 \
    RequestTimeoutSeconds=15 \
    Serilog__MinimumLevel__Default=Information \
    Serilog__MinimumLevel__Override__Microsoft=Warning \
    Serilog__MinimumLevel__Override__Microsoft.EntityFrameworkCore.Database.Command=Warning

EXPOSE 9005

ENTRYPOINT ["dotnet", "ProductServiceApp.Api.dll"]
