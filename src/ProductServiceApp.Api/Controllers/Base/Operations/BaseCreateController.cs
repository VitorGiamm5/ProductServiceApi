using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace ProductServiceApp.Api.Controllers.Base.Operations;

public abstract class BaseCreateController<TCreateRequest, TCreateResponse, TCreateCommand>(
    Channel<(TCreateCommand, TaskCompletionSource<TCreateResponse>, CancellationToken)> channel)
    : BaseApiController
    where TCreateRequest : class
    where TCreateResponse : class
    where TCreateCommand : class
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public virtual Task<IActionResult> Create([FromBody] TCreateRequest request, CancellationToken cancellationToken)
        => ExecuteChannelAsync<TCreateCommand, TCreateResponse>(
            channel, BuildCreateCommand(request), cancellationToken);

    protected abstract TCreateCommand BuildCreateCommand(TCreateRequest request);
}
