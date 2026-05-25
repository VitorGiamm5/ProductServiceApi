using System.Text.Json;
using System.Text.Json.Serialization;
using ProductServiceApp.Shared.Api;
using ProductServiceApp.Shared.Orders;

namespace ProductServiceApp.Web.Clients;

public sealed class OrderApiClient(
    HttpClient httpClient,
    LoadingState loading,
    ILogger<OrderApiClient> logger)
{
    private const string OrdersEndpoint = "api/v1/Orders";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<IReadOnlyList<OrderResponse>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        logger.LogInformation("Calling order API: {BaseAddress}{Endpoint}", httpClient.BaseAddress, OrdersEndpoint);

        var response = await httpClient.GetFromJsonAsync<ApiResponse<List<OrderResponse>>>(
            OrdersEndpoint,
            JsonOptions,
            cancellationToken);

        return response?.Data ?? [];
    }

    public async Task<OrderResponse?> GetOrderAsync(long id, CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var response = await httpClient.GetFromJsonAsync<ApiResponse<OrderResponse>>(
            $"{OrdersEndpoint}/{id}",
            JsonOptions,
            cancellationToken);

        return response?.Data;
    }

    public async Task<OrderResponse?> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var httpResponse = await httpClient.PostAsJsonAsync(OrdersEndpoint, request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(httpResponse, cancellationToken);

        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<OrderResponse>>(
            JsonOptions,
            cancellationToken);

        return response?.Data;
    }

    public async Task<OrderResponse?> UpdateOrderAsync(
        long id,
        UpdateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var httpResponse = await httpClient.PutAsJsonAsync(
            $"{OrdersEndpoint}/{id}",
            request,
            JsonOptions,
            cancellationToken);
        await EnsureSuccessAsync(httpResponse, cancellationToken);

        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<OrderResponse>>(
            JsonOptions,
            cancellationToken);

        return response?.Data;
    }

    public async Task DeleteOrderAsync(long id, CancellationToken cancellationToken = default)
    {
        using var _ = loading.Begin();

        var response = await httpClient.DeleteAsync($"{OrdersEndpoint}/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(
            JsonOptions,
            cancellationToken);

        var message = apiResponse?.Errors.Count > 0
            ? string.Join(" ", apiResponse.Errors.Select(error => error.Message))
            : response.ReasonPhrase;

        throw new HttpRequestException(message, null, response.StatusCode);
    }
}
