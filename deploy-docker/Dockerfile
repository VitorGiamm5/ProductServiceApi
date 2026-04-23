FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source

COPY ../ProductServiceApp.slnx .
COPY ../src/ProductServiceApp.Api/ProductServiceApp.Api.csproj src/ProductServiceApp.Api/

COPY src/ src/

RUN dotnet publish src/ProductServiceApp.Api/ProductServiceApp.Api.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# useradd no lugar de adduser (compatível com a imagem base)
RUN useradd -m appuser && \
    chown -R appuser:appuser /app

USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_HTTP_PORTS=9005

EXPOSE 9005

ENTRYPOINT ["dotnet", "ProductServiceApp.Api.dll"]