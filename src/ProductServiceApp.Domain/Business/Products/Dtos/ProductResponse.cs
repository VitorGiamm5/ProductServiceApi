namespace ProductServiceApp.Domain.Business.Products.Dtos;

public class ProductResponse : CreateProductRequest
{
    #region Additional Properties

    public string? TypeName { get; set; }
    public string? ProductName { get; set; }
    public DateTime? CreatedDate { get; set; }

    #endregion
}
