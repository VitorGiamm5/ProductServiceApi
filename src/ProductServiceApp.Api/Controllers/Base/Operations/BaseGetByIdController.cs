using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace ProductServiceApp.Api.Controllers.Base.Operations;

public abstract class BaseGetByIdController<TGetResponse, TGetByIdQuery>(
    Channel<(TGetByIdQuery, TaskCompletionSource<TGetResponse>, CancellationToken)> channel)
    : BaseApiController
    where TGetResponse : class
    where TGetByIdQuery : class
{
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => ExecuteChannelAsync<TGetByIdQuery, TGetResponse>(
            channel, BuildGetByIdQuery(id), cancellationToken);

    protected abstract TGetByIdQuery BuildGetByIdQuery(long id);
}