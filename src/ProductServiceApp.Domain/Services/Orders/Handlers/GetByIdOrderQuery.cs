using System.ComponentModel.DataAnnotations;
using ProductServiceApp.Domain.Identifications;

namespace ProductServiceApp.Domain.Services.Orders.Handlers;

public class GetByIdOrderQuery : IIdentifiableLong
{
    public GetByIdOrderQuery(long id)
    {
        Id = id;
    }

    [Required]
    public long Id { get; set; }
}
