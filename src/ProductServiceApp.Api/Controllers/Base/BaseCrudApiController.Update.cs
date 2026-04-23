using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Application.ApiResponseCommom;

namespace ProductServiceApp.Api.Controllers.Base.BaseCrudApiController;

public abstract partial class BaseCrudApiController<TGetResponse, TCreateRequest, TCreateResponse, TUpdateRequest, TUpdateResponse>
{
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public abstract Task<IActionResult> Update(long id, [FromBody] TUpdateRequest request, CancellationToken cancellationToken);
}
