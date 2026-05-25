using Microsoft.AspNetCore.Mvc;

namespace ProductServiceApp.Api.Controllers.Base.Contracts;

public interface IHasCreate<TCreateRequest, TCreateResponse>
    where TCreateRequest : class
    where TCreateResponse : class
{
    Task<IActionResult> Create(TCreateRequest request, CancellationToken cancellationToken);
}
