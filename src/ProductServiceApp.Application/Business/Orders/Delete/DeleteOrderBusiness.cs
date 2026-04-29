using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;

namespace ProductServiceApp.Application.Business.Orders.Delete;

public class DeleteOrderBusiness(
        IOrderCommandRepository repository,
        IOrderQueryRepository readRepository,
        IValidator<DeleteOrderCommand> validator)
    : BaseBusinessService<DeleteOrderCommand, OrderEntity, OrderEntity, BooleanResponse>,
    IDeleteOrderBusiness
{
    protected override async Task<OrderEntity> PreProcessAsync(DeleteOrderCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        return await readRepository.GetByIdAsync(input.Id, ct);
    }

    protected override async Task<OrderEntity> ProcessAsync(OrderEntity input, CancellationToken ct)
    {
        return await repository.SoftDeleteAsync(input.Id, ct);
    }

    protected override Task<BooleanResponse> PostProcessAsync(OrderEntity result, CancellationToken ct)
    {
        return Task.FromResult(new BooleanResponse
        {
            IsSuccess = true
        });
    }
}
