using ProductServiceApp.Domain.Identifications;
using System.ComponentModel.DataAnnotations;

namespace ProductServiceApp.Domain.Business.Products.Handlers;

public class GetByIdProductQuery : IIdentifiableLong
{
    public GetByIdProductQuery(long id)
    {
        Id = id;
    }

    [Required]
    public long Id { get; set; }
}
