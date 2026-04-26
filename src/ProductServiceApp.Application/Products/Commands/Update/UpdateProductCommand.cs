using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Products.Dtos;

namespace ProductServiceApp.Application.Products.Commands.Update;

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
