using ProductServiceApp.Domain.Identifications;
using System.ComponentModel.DataAnnotations;

namespace ProductServiceApp.Application.Products.Queries.GetById;

public class GetProductByIdQuery : IIdentifiableLong
{
    public GetProductByIdQuery(long id)
    {
        Id = id;
    }

    [Required]
    public long Id { get; set; }
}
