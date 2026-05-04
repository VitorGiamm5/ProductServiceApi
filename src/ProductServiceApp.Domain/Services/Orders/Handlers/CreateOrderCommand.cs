using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Services.Orders.Dtos;

namespace ProductServiceApp.Domain.Services.Orders.Handlers;

public class CreateOrderCommand : CreateOrderRequest,
    IFromMapper<CreateOrderCommand, CreateOrderRequest>
{
    public CreateOrderCommand(CreateOrderRequest input) => MapFrom(input);

    public CreateOrderCommand MapFrom(CreateOrderRequest? input)
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
