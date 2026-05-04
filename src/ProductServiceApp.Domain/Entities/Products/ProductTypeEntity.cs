using ProductServiceApp.Domain.Enums.Products;

namespace ProductServiceApp.Domain.Entities.Products;

public class ProductTypeEntity
{
    public ProductsTypeEnum Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public ICollection<ProductEntity> Products { get; set; } = [];
}
