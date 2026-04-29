using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;

namespace ProductServiceApp.Application.Business.Orders.GetById;

public class GetByIdOrderBusiness(
        IOrderQueryRepository repository,
        IValidator<GetByIdOrderQuery> validator)
    : BaseBusinessService<GetByIdOrderQuery, GetByIdOrderQuery, OrderEntity, OrderResponse>,
    IGetByIdOrderBusiness
{
    protected override async Task<GetByIdOrderQuery> PreProcessAsync(GetByIdOrderQuery input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        return input;
    }

    protected override async Task<OrderEntity> ProcessAsync(GetByIdOrderQuery input, CancellationToken ct)
    {
        return await repository.GetByIdAsync(input.Id, ct);
    }

    protected override Task<OrderResponse> PostProcessAsync(OrderEntity result, CancellationToken ct)
    {
        return Task.FromResult(new OrderResponse(result));
    }
}
