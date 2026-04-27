using ProductServiceApp.Domain.Enums.Products;

namespace ProductServiceApp.Domain.Business.Products.Dtos;

public class CreateProductRequest
{
    public long? Id { get; set; } = 0L;
    public string? Name { get; set; } = string.Empty;
    public decimal? Price { get; set; } = decimal.Zero;
    public ProductsTypeEnum? Type { get; set; } = ProductsTypeEnum.Unknown;
    public bool? IsActive { get; set; } = true;
}
