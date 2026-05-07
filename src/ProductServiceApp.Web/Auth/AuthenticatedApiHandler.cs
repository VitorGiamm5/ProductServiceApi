using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace ProductServiceApp.Web.Auth;

public sealed class AuthenticatedApiHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = httpContextAccessor.HttpContext is null
            ? null
            : await httpContextAccessor.HttpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrWhiteSpace(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
