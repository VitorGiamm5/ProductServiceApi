using ProductServiceApp.Domain.Services.Products.Base;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Domain.Services.Products.Business;

public interface IGetAllProductBusiness
    : IBaseProductBusinessService<GetAllProductQuery, IEnumerable<ProductResponse>>
{
}
