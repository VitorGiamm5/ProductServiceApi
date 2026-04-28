using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace ProductServiceApp.Api.Controllers.Base;

public abstract class BaseCrudApiController<
    TGetResponse,
    TCreateRequest, TCreateResponse, TCreateCommand,
    TUpdateRequest, TUpdateResponse, TUpdateCommand,
    TDeleteRequest, TDeleteResponse, TDeleteCommand,
    TGetByIdQuery,
    TGetAllQuery>(
        Channel<(TCreateCommand, TaskCompletionSource<TCreateResponse>, CancellationToken)> createChannel,
        Channel<(TUpdateCommand, TaskCompletionSource<TUpdateResponse>, CancellationToken)> updateChannel,
        Channel<(TDeleteCommand, TaskCompletionSource<TDeleteResponse>, CancellationToken)> deleteChannel,
        Channel<(TGetAllQuery, TaskCompletionSource<IEnumerable<TGetResponse>>, CancellationToken)> getAllChannel,
        Channel<(TGetByIdQuery, TaskCompletionSource<TGetResponse>, CancellationToken)> getByIdChannel
    )
    : BaseApiController
    where TGetResponse : class
    where TCreateRequest : class
    where TCreateResponse : class
    where TCreateCommand : class
    where TUpdateRequest : class
    where TUpdateResponse : class
    where TUpdateCommand : class
    where TDeleteRequest : class
    where TDeleteResponse : class
    where TDeleteCommand : class
    where TGetByIdQuery : class
    where TGetAllQuery : class
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual Task<IActionResult> GetAll(CancellationToken ct)
        => ExecuteChannelAsync<TGetAllQuery, IEnumerable<TGetResponse>>(
            getAllChannel, BuildGetAllQuery(), ct);

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual Task<IActionResult> GetById(long id, CancellationToken ct)
        => ExecuteChannelAsync<TGetByIdQuery, TGetResponse>(
            getByIdChannel, BuildGetByIdQuery(id), ct);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public virtual Task<IActionResult> Create([FromBody] TCreateRequest request, CancellationToken ct)
        => ExecuteChannelAsync<TCreateCommand, TCreateResponse>(
            createChannel, BuildCreateCommand(request), ct);

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual Task<IActionResult> Update(long id, [FromBody] TUpdateRequest request, CancellationToken ct)
        => ExecuteChannelAsync<TUpdateCommand, TUpdateResponse>(
            updateChannel, BuildUpdateCommand(id, request), ct);

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual Task<IActionResult> Delete(long id, CancellationToken ct)
        => ExecuteChannelAsync<TDeleteCommand, TDeleteResponse>(
            deleteChannel, BuildDeleteCommand(id), ct);

    protected abstract TGetAllQuery BuildGetAllQuery();
    protected abstract TGetByIdQuery BuildGetByIdQuery(long id);
    protected abstract TCreateCommand BuildCreateCommand(TCreateRequest request);
    protected abstract TUpdateCommand BuildUpdateCommand(long id, TUpdateRequest request);
    protected abstract TDeleteCommand BuildDeleteCommand(long id);
}
