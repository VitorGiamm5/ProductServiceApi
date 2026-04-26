using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Products.Dtos;

namespace ProductServiceApp.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommand : CreateProductsRequest, IFromMapper<UpdateProductCommand, UpdateProductsRequest>
{
    public UpdateProductCommand(UpdateProductsRequest input) => MapFrom(input);

    public UpdateProductCommand MapFrom(UpdateProductsRequest? input)
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
