using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Domain.ApiResponseBase;

namespace ProductServiceApp.Api.Controllers.Base.BaseCrudApiController;

public abstract partial class BaseCrudApiController<TGetResponse, TCreateRequest, TCreateResponse, TUpdateRequest, TUpdateResponse>
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public abstract Task<IActionResult> GetAll(CancellationToken cancellationToken);
}
