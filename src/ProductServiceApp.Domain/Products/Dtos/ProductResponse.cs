namespace ProductServiceApp.Domain.Products.Dtos;

public class ProductResponse : CreateProductRequest
{
    public string? ProductName { get; set; }
    public DateTime? CreatedDate { get; set; }
}
