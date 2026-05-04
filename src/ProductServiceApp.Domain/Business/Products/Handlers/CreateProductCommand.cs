using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Mappers;

namespace ProductServiceApp.Domain.Business.Products.Handlers;

public class CreateProductCommand : CreateProductRequest,
    IFromMapper<CreateProductCommand, CreateProductRequest>,
    IToMapper<ProductEntity>
{
    #region Constructors

    public CreateProductCommand(CreateProductRequest input) => MapFrom(input);

    #endregion

    #region Mapping

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
