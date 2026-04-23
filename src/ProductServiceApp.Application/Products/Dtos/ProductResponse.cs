using ProductServiceApp.Domain.Commom;
using ProductServiceApp.Domain.Products.Entities;

namespace ProductServiceApp.Application.Products.Dtos;

public class ProductResponse : ProductItem, IIdentifiableLong
{
    public long Id { get; set; }
}

