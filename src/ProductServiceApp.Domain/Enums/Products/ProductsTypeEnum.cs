using System.ComponentModel;

namespace ProductServiceApp.Domain.Enums.Products;

public enum ProductsTypeEnum : byte
{
    [Description("Nao definido")]
    Default = 0,
    [Description("Sanduiche")]
    Sandwich = 1,
    [Description("Batata frita")]
    Fries = 2,
    [Description("Refrigerante")]
    Refreshment = 3
}
