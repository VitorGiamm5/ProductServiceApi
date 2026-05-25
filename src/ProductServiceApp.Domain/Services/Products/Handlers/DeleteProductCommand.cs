using System.ComponentModel.DataAnnotations;
using ProductServiceApp.Domain.Identifications;

namespace ProductServiceApp.Domain.Services.Products.Handlers;

public class DeleteProductCommand : IIdentifiableLong
{
    public DeleteProductCommand(long id)
    {
        Id = id;
    }

    [Required]
    public long Id { get; set; }
}
