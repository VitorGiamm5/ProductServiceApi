using ProductServiceApp.Domain.Identifications;
using ProductServiceApp.Domain.Repositories.EntitiesBase;
using System.ComponentModel.DataAnnotations;

namespace ProductServiceApp.Domain.Products.Entities;

public class ProductEntity : BaseAuditEntity, IIdentifiableLong
{
    public long Id { get; set; } = 0L;

    [MaxLength(150)]
    public string? Name { get; set; } = string.Empty;

    [Required]
    public decimal? Price { get; set; } = decimal.Zero;

    [Required]
    public ProductsTypeEnum? Type { get; set; } = ProductsTypeEnum.Unknown;
}
