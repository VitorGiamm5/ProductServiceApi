using ProductServiceApp.Domain.Identifications;
using System.ComponentModel.DataAnnotations;

namespace ProductServiceApp.Domain.Business.Orders.Handlers;

public class DeleteOrderCommand : IIdentifiableLong
{
    public DeleteOrderCommand(long id)
    {
        Id = id;
    }

    [Required]
    public long Id { get; set; }
}
