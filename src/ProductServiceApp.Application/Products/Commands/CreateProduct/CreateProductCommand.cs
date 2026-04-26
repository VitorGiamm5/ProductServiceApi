using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Products.Dtos;

namespace ProductServiceApp.Application.Products.Commands.CreateProduct;

public class CreateProductCommand : CreateProductsRequest, IFromMapper<CreateProductCommand, CreateProductsRequest>
{
    public CreateProductCommand(CreateProductsRequest input) => MapFrom(input);

    public CreateProductCommand MapFrom(CreateProductsRequest? input)
    {
        if (input != null)
        {
            Id = input.Id;
            Name = input.Name;
            Price = input.Price;
            Type = input.Type;
        }

        return this;
    }
}
