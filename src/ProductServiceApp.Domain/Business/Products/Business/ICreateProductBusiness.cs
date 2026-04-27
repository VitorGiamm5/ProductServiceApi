using ProductServiceApp.Domain.Business.Products.Base;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;

namespace ProductServiceApp.Domain.Business.Products.Business;

public interface ICreateProductBusiness : IBaseProductBusinessService<CreateProductCommand, ProductResponse>
{
}
