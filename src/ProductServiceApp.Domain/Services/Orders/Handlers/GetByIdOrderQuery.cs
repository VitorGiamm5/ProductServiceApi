using System.ComponentModel.DataAnnotations;
using ProductServiceApp.Domain.Identifications;

namespace ProductServiceApp.Domain.Services.Orders.Handlers;

public class GetByIdOrderQuery(long id) : IIdentifiableLong
{
    [Required]
    public long Id { get; set; } = id;
}
