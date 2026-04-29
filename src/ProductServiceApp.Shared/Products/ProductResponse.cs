namespace ProductServiceApp.Shared.Products;

public class ProductResponse : CreateProductRequest
{
    public string? TypeName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
