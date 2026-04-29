namespace ProductServiceApp.Api.Controllers.Base.BaseApiResponse;

public sealed class ApiErrorDetail
{
    public Int16 Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}
