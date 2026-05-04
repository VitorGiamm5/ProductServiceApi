using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;

namespace ProductServiceApp.Application.Business.Orders.Delete;

public sealed record DeleteOrderIntermediate(
    DeleteOrderCommand Input,
    OrderEntity OrderToDelete,
    DateTime DeletedDate);

public class DeleteOrderBusiness(
        IOrderCommandRepository repository,
        IOrderQueryRepository readRepository,
        IValidator<DeleteOrderCommand> validator)
    : BaseBusinessService<DeleteOrderCommand, DeleteOrderIntermediate, OrderEntity, BooleanResponse>,
    IDeleteOrderBusiness
{
    private readonly DateTime _deletedDate = DateTime.UtcNow;

    //INBOX
    protected override async Task<DeleteOrderIntermediate> PreProcessAsync(DeleteOrderCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var entity = await readRepository.GetByIdAsync(input.Id, ct);

        return MapToIntermediate(new DeleteOrderIntermediate(input, entity, _deletedDate));
    }

    //PROCESS
    protected override async Task<OrderEntity> ProcessAsync(DeleteOrderIntermediate input, CancellationToken ct)
    {
        return await repository.SoftDeleteAsync(input.OrderToDelete.Id, ct);
    }

    //OUTBOX
    protected override Task<BooleanResponse> PostProcessAsync(OrderEntity result, CancellationToken ct)
    {
        return Task.FromResult(new BooleanResponse
        {
            IsSuccess = true
        });
    }

    //MAP
    public static DeleteOrderIntermediate MapToIntermediate(DeleteOrderIntermediate intermediate)
    {
        intermediate.OrderToDelete.DeletedDate = intermediate.DeletedDate;
        intermediate.OrderToDelete.DeletedByUserId = 0;
        intermediate.OrderToDelete.IsActive = false;
        intermediate.OrderToDelete.IsDeleted = true;

        return intermediate;
    }
}
