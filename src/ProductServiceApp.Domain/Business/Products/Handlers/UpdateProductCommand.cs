using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Mappers;

namespace ProductServiceApp.Domain.Business.Products.Handlers;

public class UpdateProductCommand : CreateProductRequest, IFromMapper<UpdateProductCommand, UpdateProductRequest>
{
    public UpdateProductCommand(UpdateProductRequest input) => MapFrom(input);

    public UpdateProductCommand MapFrom(UpdateProductRequest? input)
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
}
