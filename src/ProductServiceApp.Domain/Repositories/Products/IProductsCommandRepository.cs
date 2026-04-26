using ProductServiceApp.Domain.Repositories.Base;

namespace ProductServiceApp.Domain.Repositories.Products;

public interface IProductsCommandRepository<T> : IBaseCommandRepository<T> where T : class
{

}
