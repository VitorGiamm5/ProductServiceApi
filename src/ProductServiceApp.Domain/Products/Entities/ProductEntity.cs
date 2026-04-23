using System.ComponentModel.DataAnnotations;

namespace ProductServiceApp.Domain.Products.Entities;

public class ProductEntity
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public ProductTypeEnum Type { get; set; }
}
