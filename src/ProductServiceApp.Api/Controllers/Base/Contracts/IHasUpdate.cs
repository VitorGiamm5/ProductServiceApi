using Microsoft.AspNetCore.Mvc;

namespace ProductServiceApp.Api.Controllers.Base.Contracts;

public interface IHasUpdate<TUpdateRequest, TUpdateResponse>
    where TUpdateRequest : class
    where TUpdateResponse : class
{
    Task<IActionResult> Update(long id, TUpdateRequest request, CancellationToken cancellationToken);
}
