using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Application.ApiResponse;

namespace ProductServiceApp.Api.Controllers.Base.BaseCrudApiController;

public abstract partial class BaseCrudApiController<TGetResponse, TCreateRequest, TCreateResponse, TUpdateRequest, TUpdateResponse>
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public abstract Task<IActionResult> Create([FromBody] TCreateRequest request);
}
