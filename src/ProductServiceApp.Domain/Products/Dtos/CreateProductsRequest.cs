namespace ProductServiceApp.Domain.Products.Dtos;

public class CreateProductsRequest
{
    public long? Id { get; set; } = long.MinValue;
    public string? Name { get; set; } = string.Empty;
    public decimal? Price { get; set; } = decimal.Zero;
    public ProductsTypeEnum? Type { get; set; } = ProductsTypeEnum.Unknown;
}
