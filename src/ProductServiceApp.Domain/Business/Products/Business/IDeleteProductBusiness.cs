using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Products.Base;
using ProductServiceApp.Domain.Business.Products.Handlers;

namespace ProductServiceApp.Domain.Business.Products.Business;

public interface IDeleteProductBusiness : IBaseProductBusinessService<DeleteProductCommand, BooleanResponse>
{
}
