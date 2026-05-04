using Asp.Versioning;
using ProductServiceApp.Api.Controllers.Base;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;
using System.Threading.Channels;

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
