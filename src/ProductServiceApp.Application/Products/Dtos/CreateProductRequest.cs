using ProductServiceApp.Domain.Commom;
using ProductServiceApp.Domain.Products.Entities;
using System.Text.Json.Serialization;

namespace ProductServiceApp.Application.Products.Dtos;

public class CreateProductRequest : ProductEntity, IIdentifiableLong
{
    [JsonIgnore]
    public long Id { get; set; } = 0;
}
