namespace ProductServiceApp.Domain.Products.Dtos;

public class ProductsResponse : CreateProductsRequest
{
    public string? ProductName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool? IsActive { get; set; }
}
