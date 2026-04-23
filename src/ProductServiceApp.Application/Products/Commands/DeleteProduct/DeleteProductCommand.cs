using ProductServiceApp.Domain.Identifications;
using System.ComponentModel.DataAnnotations;

namespace ProductServiceApp.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommand : IIdentifiableLong
{
    public DeleteProductCommand(long id)
    {
        Id = id;
    }

    [Required]
    public long Id { get; set; }
}
