using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Products.Dtos;
using ProductServiceApp.Domain.Products.Entities;

namespace ProductServiceApp.Application.Products.Commands.Create;

public class CreateProductCommand : CreateProductRequest, 
    IFromMapper<CreateProductCommand, CreateProductRequest>,
    IToMapper<ProductEntity>
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
            IsActive = input.IsActive;
        }

        return this;
    }

    public ProductEntity MapTo()
    {
        return new ProductEntity
        {
            Id = Id ?? 0L,
            Name = Name,
            Price = Price,
            Type = Type,
            IsActive = IsActive
        };
    }
}
