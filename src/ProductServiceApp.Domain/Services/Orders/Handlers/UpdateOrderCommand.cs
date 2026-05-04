using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Services.Orders.Dtos;

namespace ProductServiceApp.Domain.Services.Orders.Handlers;

public class UpdateOrderCommand : CreateOrderRequest,
    IFromMapper<UpdateOrderCommand, UpdateOrderRequest>
{
    public UpdateOrderCommand(UpdateOrderRequest input) => MapFrom(input);

    public UpdateOrderCommand MapFrom(UpdateOrderRequest? input)
    {
        if (input is not null)
        {
            Id = input.Id;
            Products = input.Products ?? [];
            IsActive = input.IsActive;
            IsDeleted = input.IsDeleted;
        }

        return this;
    }
}
