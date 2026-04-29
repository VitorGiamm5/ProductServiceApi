namespace ProductServiceApp.Shared.Products;

public class CreateProductRequest
{
    public long? Id { get; set; } = 0L;
    public string? Name { get; set; } = string.Empty;
    public decimal? Price { get; set; } = decimal.Zero;
    public ProductsType? Type { get; set; } = ProductsType.Default;
    public bool? IsActive { get; set; } = true;
    public bool? IsDeleted { get; set; } = false;
}
