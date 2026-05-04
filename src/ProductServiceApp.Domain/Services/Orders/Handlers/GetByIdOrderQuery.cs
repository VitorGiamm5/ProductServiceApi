using ProductServiceApp.Domain.Identifications;
using System.ComponentModel.DataAnnotations;

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
