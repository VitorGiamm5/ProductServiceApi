using ProductServiceApp.Domain.Enums.Products;

namespace ProductServiceApp.Domain.Services.Products.Dtos;

/// <summary>
/// Data Transfer Object (DTO) for creating a new product, encapsulating the necessary information for product creation.
/// </summary>
[Serializable]
public class CreateProductRequest
{
    public long? Id { get; set; } = 0L;

    public string? Name { get; set; } = string.Empty;

    public decimal? Price { get; set; } = decimal.Zero;

    public ProductsTypeEnum? Type { get; set; } = (byte)ProductsTypeEnum.Default;

    public bool? IsActive { get; set; } = true;

    public bool? IsDeleted { get; set; } = false;
}
