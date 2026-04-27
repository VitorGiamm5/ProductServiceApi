using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace ProductServiceApp.Api.Controllers.Base.Operations;

public abstract class BaseDeleteController<TDeleteCommand, TDeleteResponse>(
    Channel<(TDeleteCommand, TaskCompletionSource<TDeleteResponse>, CancellationToken)> channel)
    : BaseApiController
    where TDeleteCommand : class
    where TDeleteResponse : class
{
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        => ExecuteChannelAsync<TDeleteCommand, TDeleteResponse>(
            channel, BuildDeleteCommand(id), cancellationToken);

    protected abstract TDeleteCommand BuildDeleteCommand(long id);
}
