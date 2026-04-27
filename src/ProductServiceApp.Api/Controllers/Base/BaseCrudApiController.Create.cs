using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Api.Controllers.Base.Contracts;
using System.Threading.Channels;

namespace ProductServiceApp.Api.Controllers.Base;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public abstract class BaseCrudApiController<
    TGetResponse,
    TCreateRequest, TCreateResponse, TCreateCommand,
    TUpdateRequest, TUpdateResponse, TUpdateCommand,
    TDeleteCommand, TDeleteResponse,
    TGetByIdQuery,
    TGetAllQuery>(
    Channel<(TCreateCommand, TaskCompletionSource<TCreateResponse>, CancellationToken)> createChannel,
    Channel<(TUpdateCommand, TaskCompletionSource<TUpdateResponse>, CancellationToken)> updateChannel,
    Channel<(TDeleteCommand, TaskCompletionSource<TDeleteResponse>, CancellationToken)> deleteChannel,
    Channel<(TGetAllQuery, TaskCompletionSource<IEnumerable<TGetResponse>>, CancellationToken)> getAllChannel,
    Channel<(TGetByIdQuery, TaskCompletionSource<TGetResponse>, CancellationToken)> getByIdChannel)
    : BaseApiController,
      IHasGetAll<TGetResponse>,
      IHasGetById<TGetResponse>,
      IHasCreate<TCreateRequest, TCreateResponse>,
      IHasUpdate<TUpdateRequest, TUpdateResponse>,
      IHasDelete<TDeleteCommand, TDeleteResponse>
    where TGetResponse : class
    where TCreateRequest : class
    where TCreateResponse : class
    where TCreateCommand : class
    where TUpdateRequest : class
    where TUpdateResponse : class
    where TUpdateCommand : class
    where TDeleteCommand : class
    where TDeleteResponse : class
    where TGetByIdQuery : class
    where TGetAllQuery : class
{
    [HttpGet]
    public virtual Task<IActionResult> GetAll(CancellationToken ct)
    {
        return ExecuteChannelAsync<TGetAllQuery, IEnumerable<TGetResponse>>(
                getAllChannel, BuildGetAllQuery(), ct);
    }

    [HttpGet("{id:long}")]
    public virtual Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        return ExecuteChannelAsync<TGetByIdQuery, TGetResponse>(
                getByIdChannel, BuildGetByIdQuery(id), ct);
    }

    [HttpPost]
    public virtual Task<IActionResult> Create([FromBody] TCreateRequest request, CancellationToken ct)
    {
        return ExecuteChannelAsync<TCreateCommand, TCreateResponse>(
                createChannel, BuildCreateCommand(request), ct);
    }

    [HttpPut("{id:long}")]
    public virtual Task<IActionResult> Update(long id, [FromBody] TUpdateRequest request, CancellationToken ct)
    {
        return ExecuteChannelAsync<TUpdateCommand, TUpdateResponse>(
                updateChannel, BuildUpdateCommand(id, request), ct);
    }

    [HttpDelete("{id:long}")]
    public virtual Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        return ExecuteChannelAsync<TDeleteCommand, TDeleteResponse>(
                deleteChannel, BuildDeleteCommand(id), ct);
    }

    protected abstract TGetAllQuery BuildGetAllQuery();
    protected abstract TGetByIdQuery BuildGetByIdQuery(long id);
    protected abstract TCreateCommand BuildCreateCommand(TCreateRequest request);
    protected abstract TUpdateCommand BuildUpdateCommand(long id, TUpdateRequest request);
    protected abstract TDeleteCommand BuildDeleteCommand(long id);
}