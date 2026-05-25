using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace ProductServiceApp.Api.Controllers.Base.Operations;

public abstract class BaseGetAllController<TGetResponse, TGetAllQuery>(
    Channel<(TGetAllQuery, TaskCompletionSource<IEnumerable<TGetResponse>>, CancellationToken)> channel)
    : BaseApiController
    where TGetResponse : class
    where TGetAllQuery : class
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => ExecuteChannelAsync<TGetAllQuery, IEnumerable<TGetResponse>>(
            channel, BuildGetAllQuery(), cancellationToken);

    protected abstract TGetAllQuery BuildGetAllQuery();
}
