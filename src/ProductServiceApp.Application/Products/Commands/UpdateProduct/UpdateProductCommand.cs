using ProductServiceApp.Application.Products.Dtos;
using ProductServiceApp.Domain.Commom;
using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Products.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProductServiceApp.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommand : ProductEntity, IIdentifiableLong, IFromMapper<UpdateProductCommand, UpdateProductRequest>
{
    [Required]
    public long Id { get; set; }

    public UpdateProductCommand(UpdateProductRequest input) => MapFrom(input);

    public UpdateProductCommand MapFrom(UpdateProductRequest? input)
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
