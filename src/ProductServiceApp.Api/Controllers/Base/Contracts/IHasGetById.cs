using Microsoft.AspNetCore.Mvc;

namespace ProductServiceApp.Api.Controllers.Base.Contracts;

public interface IHasGetById<TGetResponse>
    where TGetResponse : class
{
    Task<IActionResult> GetById(long id, CancellationToken cancellationToken);
}
