using System.Threading.Channels;
using Asp.Versioning;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Api.Auth;
using ProductServiceApp.Api.Controllers.Base;
using ProductServiceApp.Api.Filters;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Api.Controllers.Orders;

[ApiVersion("1.0")]
public class OrdersController(
    Channel<(CreateOrderCommand, TaskCompletionSource<OrderResponse>, CancellationToken)> createChannel,
    Channel<(UpdateOrderCommand, TaskCompletionSource<OrderResponse>, CancellationToken)> updateChannel,
    Channel<(DeleteOrderCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)> deleteChannel,
    Channel<(GetAllOrderQuery, TaskCompletionSource<IEnumerable<OrderResponse>>, CancellationToken)> getAllChannel,
    Channel<(GetByIdOrderQuery, TaskCompletionSource<OrderResponse>, CancellationToken)> getByIdChannel)
    : BaseCrudApiController<
        OrderResponse,
        CreateOrderRequest, OrderResponse, CreateOrderCommand,
        UpdateOrderRequest, OrderResponse, UpdateOrderCommand,
        DeleteOrderCommand, BooleanResponse,
        GetByIdOrderQuery,
        GetAllOrderQuery>(
        createChannel,
        updateChannel,
        deleteChannel,
        getAllChannel,
        getByIdChannel)
{
    private const double IdempotencyKeyRetentionMilliseconds = 24 * 60 * 60 * 1000;

    [Authorize(Policy = AuthPolicies.OrdersViewAll)]
    public override Task<IActionResult> GetAll(CancellationToken ct) => base.GetAll(ct);

    [Authorize(Policy = AuthPolicies.OrdersRead)]
    public override Task<IActionResult> GetById(long id, CancellationToken ct) => base.GetById(id, ct);

    [Authorize(Policy = AuthPolicies.OrdersWrite)]
    [RequireIdempotencyKey]
    [Idempotent(
        ExpiresInMilliseconds = IdempotencyKeyRetentionMilliseconds,
        HeaderKeyName = "IdempotencyKey",
        DistributedCacheKeysPrefix = "orders:idempotency:",
        CacheOnlySuccessResponses = true)]
    public override Task<IActionResult> Create(CreateOrderRequest request, CancellationToken ct) => base.Create(request, ct);

    [Authorize(Policy = AuthPolicies.OrdersWrite)]
    public override Task<IActionResult> Update(long id, UpdateOrderRequest request, CancellationToken ct) => base.Update(id, request, ct);

    [Authorize(Policy = AuthPolicies.OrdersWrite)]
    public override Task<IActionResult> Delete(long id, CancellationToken ct) => base.Delete(id, ct);

    protected override GetAllOrderQuery BuildGetAllQuery()
    {
        return new();
    }

    protected override GetByIdOrderQuery BuildGetByIdQuery(long id)
    {
        return new(id);
    }

    protected override CreateOrderCommand BuildCreateCommand(CreateOrderRequest request)
    {
        return new(request);
    }

    protected override UpdateOrderCommand BuildUpdateCommand(long id, UpdateOrderRequest request)
    {
        request.Id = id;

        return new(request);
    }

    protected override DeleteOrderCommand BuildDeleteCommand(long id)
    {
        return new(id);
    }
}
