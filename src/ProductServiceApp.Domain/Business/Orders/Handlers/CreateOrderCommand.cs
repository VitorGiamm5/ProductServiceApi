using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Mappers;

namespace ProductServiceApp.Domain.Business.Orders.Handlers;

public class CreateOrderCommand : CreateOrderRequest,
    IFromMapper<CreateOrderCommand, CreateOrderRequest>
{
    public CreateOrderCommand(CreateOrderRequest input) => MapFrom(input);

    public CreateOrderCommand MapFrom(CreateOrderRequest? input)
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
