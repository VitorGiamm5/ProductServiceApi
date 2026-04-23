namespace ProductServiceApp.Domain.Products.Dtos;

public class CreateProductRequest
{
    public long? Id { get; set; } = long.MinValue;
    public string? Name { get; set; } = string.Empty;
    public decimal? Price { get; set; } = decimal.Zero;
    public ProductTypeEnum? Type { get; set; } = ProductTypeEnum.Unknown;
}
