using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Application.Business.Orders.GetById;

public sealed record GetByIdOrderIntermediate(
    GetByIdOrderQuery Input);

public sealed record GetByIdOrderToPostProcess(
    OrderEntity RetrievedOrder);

public class GetByIdOrderBusiness(
        IOrderQueryRepository repository,
        IOrderCacheService cache,
        IValidator<GetByIdOrderQuery> validator)
    : BaseBusinessService<GetByIdOrderQuery, GetByIdOrderIntermediate, GetByIdOrderToPostProcess, OrderResponse>,
    IGetByIdOrderBusiness
{
    #region INBOX

    protected override async Task<GetByIdOrderIntermediate> PreProcessAsync(GetByIdOrderQuery input, CancellationToken ct)
    {
        #region VALIDATION

        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        #endregion

        #region Map

        return new GetByIdOrderIntermediate(input);

        #endregion
    }

    #endregion

    #region PROCESS

    protected override async Task<GetByIdOrderToPostProcess> ProcessAsync(GetByIdOrderIntermediate input, CancellationToken ct)
    {
        var entity = MapToIntermediate(input);

        var cachedOrder = await cache.GetByIdAsync(entity.Id, ct);
        if (cachedOrder is not null)
            return new GetByIdOrderToPostProcess(cachedOrder);

        var result = await repository.GetByIdAsync(entity.Id, ct);
        await cache.SetByIdAsync(result, ct);

        return new GetByIdOrderToPostProcess(result);
    }

    #endregion

    #region OUTBOX

    protected override Task<OrderResponse> PostProcessAsync(GetByIdOrderToPostProcess result, CancellationToken ct)
    {
        return Task.FromResult(new OrderResponse(result.RetrievedOrder));
    }

    #endregion

    #region MAP

    public static OrderEntity MapToIntermediate(GetByIdOrderIntermediate input)
    {
        return new OrderEntity
        {
            Id = input.Input.Id
        };
    }

    //MAP
    public static OrderResponse MapToPostProcess(GetByIdOrderToPostProcess postProcess)
    {
        return new OrderResponse(postProcess.RetrievedOrder);
    }

    #endregion

}
