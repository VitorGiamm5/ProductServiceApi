using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Domain.Controller.BaseApiResponse;

namespace ProductServiceApp.Api.Controllers.Base.BaseCrudApiController;

public abstract partial class BaseCrudApiController<TGetResponse, TCreateRequest, TCreateResponse, TUpdateRequest, TUpdateResponse>
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public abstract Task<IActionResult> GetById(long id, CancellationToken cancellationToken);
}
