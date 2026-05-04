using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Domain.DateTimes;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Application.Business.Orders.Delete;

public sealed record DeleteOrderIntermediate(
    DeleteOrderCommand Input,
    OrderEntity OrderToDelete,
    DateTime DeletedDate);

public class DeleteOrderBusiness(
        IOrderCommandRepository repository,
        IOrderQueryRepository readRepository,
        IOrderCacheService cache,
        IValidator<DeleteOrderCommand> validator)
    : BaseBusinessService<DeleteOrderCommand, DeleteOrderIntermediate, OrderEntity, BooleanResponse>,
    IDeleteOrderBusiness
{
    private readonly DateTime _deletedDate = DateTimeProvider.UtcNowAsUnspecified();

    #region INBOX

    protected override async Task<DeleteOrderIntermediate> PreProcessAsync(DeleteOrderCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var entity = await readRepository.GetByIdAsync(input.Id, ct);

        return MapToIntermediate(new DeleteOrderIntermediate(input, entity, _deletedDate));
    }

    #endregion

    #region PROCESS

    protected override async Task<OrderEntity> ProcessAsync(DeleteOrderIntermediate input, CancellationToken ct)
    {
        return await repository.SoftDeleteAsync(input.OrderToDelete.Id, ct);
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

    #region MAP

    public static DeleteOrderIntermediate MapToIntermediate(DeleteOrderIntermediate intermediate)
    {
        intermediate.OrderToDelete.DeletedDate = intermediate.DeletedDate;
        intermediate.OrderToDelete.DeletedByUserId = 0;
        intermediate.OrderToDelete.IsActive = false;
        intermediate.OrderToDelete.IsDeleted = true;

        return intermediate;
    }

    #endregion

}
