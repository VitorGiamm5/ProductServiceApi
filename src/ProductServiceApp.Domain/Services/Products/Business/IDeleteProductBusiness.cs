using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Products.Base;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Domain.Services.Products.Business;

public interface IDeleteProductBusiness
    : IBaseProductBusinessService<DeleteProductCommand, BooleanResponse>
{
}
