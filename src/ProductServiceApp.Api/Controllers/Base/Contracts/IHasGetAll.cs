using Microsoft.AspNetCore.Mvc;

namespace ProductServiceApp.Api.Controllers.Base.Contracts;

public interface IHasGetAll<TGetResponse>
    where TGetResponse : class
{
    Task<IActionResult> GetAll(CancellationToken cancellationToken);
}
