using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Services.Products.Dtos;

namespace ProductServiceApp.Domain.Services.Products.Handlers;

public class UpdateProductCommand : CreateProductRequest,
    IFromMapper<UpdateProductCommand, UpdateProductRequest>,
    IToMapper<ProductEntity>
{
    #region Constructors

    public UpdateProductCommand(UpdateProductRequest input) => MapFrom(input);

    #endregion

    #region Mapping

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

    /// <summary>
    /// To entity mapping
    /// </summary>
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

    #endregion

}
