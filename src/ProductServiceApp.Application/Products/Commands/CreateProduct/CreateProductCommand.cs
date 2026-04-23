using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Products.Dtos;

namespace ProductServiceApp.Application.Products.Commands.CreateProduct;

public class CreateProductCommand : CreateProductRequest, IFromMapper<CreateProductCommand, CreateProductRequest>
{
    public CreateProductCommand(CreateProductRequest input) => MapFrom(input);

    public CreateProductCommand MapFrom(CreateProductRequest? input)
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
