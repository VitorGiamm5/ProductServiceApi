using System.ComponentModel.DataAnnotations;
using ProductServiceApp.Domain.Identifications;

namespace ProductServiceApp.Domain.Services.Orders.Handlers;

public class DeleteOrderCommand : IIdentifiableLong
{
    public DeleteOrderCommand(long id)
    {
        Id = id;
    }

    [Required]
    public long Id { get; set; }
}
