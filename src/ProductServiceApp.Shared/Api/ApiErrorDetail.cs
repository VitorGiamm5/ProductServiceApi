namespace ProductServiceApp.Shared.Api;

public sealed class ApiErrorDetail
{
    public short Code { get; set; }
    public string? Message { get; set; }
    public object? Details { get; set; }
}
