using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Domain.Controller.BaseApiResponse;
using System.Threading.Channels;

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
    private const int SecondsToTimeoutRequest = 3;

    protected Task<IActionResult> ExecuteChannelAsync<TMessage, TResponse>(
        Channel<(TMessage, TaskCompletionSource<TResponse>, CancellationToken)> channel,
        TMessage message,
        CancellationToken cancellationToken)
        where TMessage : class
        where TResponse : class
        => ExecuteAsync<TResponse>(
            (tcs, token) => channel.Writer
                .WriteAsync((message, tcs, token), token)
                .AsTask(),
            cancellationToken);

    protected async Task<IActionResult> ExecuteAsync<T>(
        Func<TaskCompletionSource<T>, CancellationToken, Task> writeToChannel,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(SecondsToTimeoutRequest));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var registration = linkedCts.Token.Register(() => tcs.TrySetCanceled(linkedCts.Token));

        await using (registration)
        {
            await writeToChannel(tcs, linkedCts.Token);

            return await ExecuteWithTimeoutAsync(tcs.Task);
        }
    }

    private async Task<IActionResult> ExecuteWithTimeoutAsync<T>(Task<T> task)
    {
        try
        {
            var result = await task;
            return Ok(ApiResponse<T>.Success(result));
        }
        catch (OperationCanceledException)
        {
            return BuildTimeoutResponse<T>();
        }
    }

    private ObjectResult BuildTimeoutResponse<T>()
        => StatusCode(408, ApiResponse<T>.SingleFailure(408, "Request timeout."));
}
