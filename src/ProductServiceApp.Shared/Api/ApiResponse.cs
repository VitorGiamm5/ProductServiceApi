namespace ProductServiceApp.Shared.Api;

public sealed class ApiResponse<T>
{
    public T? Data { get; set; }
    public List<ApiErrorDetail> Errors { get; set; } = [];
}
