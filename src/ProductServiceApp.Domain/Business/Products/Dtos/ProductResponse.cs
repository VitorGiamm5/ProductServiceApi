using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Mappers;

namespace ProductServiceApp.Domain.Business.Products.Dtos;

/// <summary>
/// Data Transfer Object (DTO) for representing product information in responses, extending the CreateProductRequest with additional properties for response purposes.
/// </summary>
public class ProductResponse : CreateProductRequest, IFromMapper<ProductResponse, ProductEntity>
{
    #region Constructors

    public ProductResponse(ProductEntity input) => MapFrom(input);

    #endregion

    #region Additional Properties

    public string? TypeName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    #endregion

    #region Mapping

    public ProductResponse MapFrom(ProductEntity? input)
    {
        if (input != null)
        {
            Id = input?.Id;
            Name = input?.Name;
            Price = input?.Price;
            Type = input?.Type;
            TypeName = ((ProductsTypeEnum?)input?.Type)?.ToString();
            IsActive = input?.IsActive;
            IsDeleted = input?.IsDeleted;
            CreatedDate = input?.CreatedDate;
            UpdatedDate = input?.UpdatedDate;
        }
        ;

        return this;
    }

    #endregion

}
