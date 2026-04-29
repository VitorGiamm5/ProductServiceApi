using System.Net.Http.Json;
using ProductServiceApp.Shared.Api;
using ProductServiceApp.Shared.Products;

namespace ProductServiceApp.Web.Clients;

public sealed class ProductApiClient(HttpClient httpClient, LoadingState loading)
{
    private const string ProductsEndpoint = "api/v1/Product";

    public async Task<IReadOnlyList<ProductResponse>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var response = await httpClient.GetFromJsonAsync<ApiResponse<List<ProductResponse>>>(
            ProductsEndpoint,
            cancellationToken);

        return response?.Data ?? [];
    }

    public async Task<ProductResponse?> GetProductAsync(long id, CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var response = await httpClient.GetFromJsonAsync<ApiResponse<ProductResponse>>(
            $"{ProductsEndpoint}/{id}",
            cancellationToken);

        return response?.Data;
    }

    public async Task<ProductResponse?> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var httpResponse = await httpClient.PostAsJsonAsync(ProductsEndpoint, request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>(
            cancellationToken);

        return response?.Data;
    }

    public async Task<ProductResponse?> UpdateProductAsync(
        long id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var httpResponse = await httpClient.PutAsJsonAsync($"{ProductsEndpoint}/{id}", request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>(
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
