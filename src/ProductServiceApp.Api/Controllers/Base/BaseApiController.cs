using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Application.ApiResponseCommom;

namespace ProductServiceApp.Api.Controllers.Base;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public class BaseApiController : ControllerBase
{
    protected async Task<IActionResult> ExecuteAsync<T>(
        Func<TaskCompletionSource<T>, CancellationToken, Task> writeToChannel,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var registration = linkedCts.Token.Register(() => tcs.TrySetCanceled(linkedCts.Token));

        await using (registration)
        {
            await writeToChannel(tcs, linkedCts.Token);

            try
            {
                var result = await tcs.Task;
                return Ok(ApiResponse<T>.Success(result));
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, ApiResponse<T>.SingleFailure(408, "Request timeout."));
            }
        }
    }
}
