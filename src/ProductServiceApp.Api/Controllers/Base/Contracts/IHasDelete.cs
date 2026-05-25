using Microsoft.AspNetCore.Mvc;

namespace ProductServiceApp.Api.Controllers.Base.Contracts;

public interface IHasDelete<TDeleteCommand, TDeleteResponse>
    where TDeleteCommand : class
    where TDeleteResponse : class
{
    Task<IActionResult> Delete(long id, CancellationToken cancellationToken);
}
