using System.Security.Claims;
using ProductServiceApp.Domain.Security;

namespace ProductServiceApp.Api.Auth;

public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public string UserId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        httpContextAccessor.HttpContext?.User.FindFirstValue("sub") ??
        "anonymous";

    public string UserName =>
        httpContextAccessor.HttpContext?.User.Identity?.Name ??
        httpContextAccessor.HttpContext?.User.FindFirstValue("preferred_username") ??
        "anonymous";

    public string? CorrelationId =>
        httpContextAccessor.HttpContext?.TraceIdentifier;
}
