using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Mappers;

namespace ProductServiceApp.Domain.Business.Products.Dtos;

public class ProductResponse : CreateProductRequest, IFromMapper<ProductResponse, ProductEntity>
{
    public ProductResponse(ProductEntity input) => MapFrom(input);

    #region Additional Properties

    public string? TypeName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

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
        };

        return this;
    }

    #endregion
}
