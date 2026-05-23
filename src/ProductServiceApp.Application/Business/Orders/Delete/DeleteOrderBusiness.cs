using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Application.Business.Orders.Delete;

public sealed record DeleteOrderIntermediate(DeleteOrderCommand Input);

public class DeleteOrderBusiness(
        IOrderCommandRepository repository,
        IOrderCacheService cache,
        IValidator<DeleteOrderCommand> validator)
    : BaseBusinessService<DeleteOrderCommand, DeleteOrderIntermediate, OrderEntity, BooleanResponse>,
    IDeleteOrderBusiness
{
    #region INBOX

    protected override async Task<DeleteOrderIntermediate> PreProcessAsync(DeleteOrderCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        return new DeleteOrderIntermediate(input);
    }

    #endregion

    #region PROCESS

    protected override async Task<OrderEntity> ProcessAsync(DeleteOrderIntermediate input, CancellationToken ct)
    {
        return await repository.SoftDeleteAsync(input.Input.Id, ct);
    }

    #endregion

    #region OUTBOX

    protected override async Task<BooleanResponse> PostProcessAsync(OrderEntity result, CancellationToken ct)
    {
        await cache.InvalidateAllAsync(ct);
        await cache.InvalidateByIdAsync(result.Id, ct);

        return new BooleanResponse
        {
            IsSuccess = true
        };
    }

    #endregion

}
