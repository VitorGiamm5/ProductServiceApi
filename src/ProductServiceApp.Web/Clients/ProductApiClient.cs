using ProductServiceApp.Shared.Api;
using ProductServiceApp.Shared.Products;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductServiceApp.Web.Clients;

public sealed class ProductApiClient(
    HttpClient httpClient,
    LoadingState loading,
    ILogger<ProductApiClient> logger)
{
    private const string ProductsEndpoint = "api/v1/Product";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<IReadOnlyList<ProductResponse>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        logger.LogInformation("Calling product API: {BaseAddress}{Endpoint}", httpClient.BaseAddress, ProductsEndpoint);

        var response = await httpClient.GetFromJsonAsync<ApiResponse<List<ProductResponse>>>(
            ProductsEndpoint,
            JsonOptions,
            cancellationToken);

        return response?.Data ?? [];
    }

    public async Task<ProductResponse?> GetProductAsync(long id, CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var response = await httpClient.GetFromJsonAsync<ApiResponse<ProductResponse>>(
            $"{ProductsEndpoint}/{id}",
            JsonOptions,
            cancellationToken);

        return response?.Data;
    }

    public async Task<ProductResponse?> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var httpResponse = await httpClient.PostAsJsonAsync(ProductsEndpoint, request, JsonOptions, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>(
            JsonOptions,
            cancellationToken);

        return response?.Data;
    }

    public async Task<ProductResponse?> UpdateProductAsync(
        long id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var httpResponse = await httpClient.PutAsJsonAsync(
            $"{ProductsEndpoint}/{id}",
            request,
            JsonOptions,
            cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>(
            JsonOptions,
            cancellationToken);

        return response?.Data;
    }

    public async Task DeleteProductAsync(long id, CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var response = await httpClient.DeleteAsync($"{ProductsEndpoint}/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
