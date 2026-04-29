using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Mappers;

namespace ProductServiceApp.Domain.Business.Orders.Handlers;

public class UpdateOrderCommand : CreateOrderRequest,
    IFromMapper<UpdateOrderCommand, UpdateOrderRequest>
{
    public UpdateOrderCommand(UpdateOrderRequest input) => MapFrom(input);

    public UpdateOrderCommand MapFrom(UpdateOrderRequest? input)
    {
        if (input is not null)
        {
            Id = input.Id;
            ProductIds = input.ProductIds ?? [];
            IsActive = input.IsActive;
            IsDeleted = input.IsDeleted;
        }

        return this;
    }
}
