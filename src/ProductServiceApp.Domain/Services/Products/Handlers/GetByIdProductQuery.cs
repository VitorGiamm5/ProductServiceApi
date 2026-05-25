using System.ComponentModel.DataAnnotations;
using ProductServiceApp.Domain.Identifications;

namespace ProductServiceApp.Domain.Services.Products.Handlers;

public class GetByIdProductQuery(long id) : IIdentifiableLong
{
    [Required]
    public long Id { get; set; } = id;
}
