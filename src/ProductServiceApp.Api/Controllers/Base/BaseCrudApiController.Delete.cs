using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Domain.ApiResponseBase;

namespace ProductServiceApp.Api.Controllers.Base.BaseCrudApiController;

public abstract partial class BaseCrudApiController<TGetResponse, TCreateRequest, TCreateResponse, TUpdateRequest, TUpdateResponse>
{
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public abstract Task<IActionResult> Delete(long id, CancellationToken cancellationToken);
}