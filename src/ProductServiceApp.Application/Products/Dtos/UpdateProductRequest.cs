using ProductServiceApp.Domain.Commom;
using ProductServiceApp.Domain.Products.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProductServiceApp.Application.Products.Dtos;

public class UpdateProductRequest : ProductEntity, IIdentifiableLong
{
    [Required]
    public long Id { get; set; }
}
