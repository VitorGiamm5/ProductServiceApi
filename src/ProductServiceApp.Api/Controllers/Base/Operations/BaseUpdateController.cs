using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace ProductServiceApp.Api.Controllers.Base.Operations;

public abstract class BaseUpdateController<TUpdateRequest, TUpdateResponse, TUpdateCommand>(
    Channel<(TUpdateCommand, TaskCompletionSource<TUpdateResponse>, CancellationToken)> channel)
    : BaseApiController
    where TUpdateRequest : class
    where TUpdateResponse : class
    where TUpdateCommand : class
{
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual Task<IActionResult> Update(long id, [FromBody] TUpdateRequest request, CancellationToken cancellationToken)
        => ExecuteChannelAsync<TUpdateCommand, TUpdateResponse>(
            channel, BuildUpdateCommand(id, request), cancellationToken);

    protected abstract TUpdateCommand BuildUpdateCommand(long id, TUpdateRequest request);
}
