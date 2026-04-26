using ProductServiceApp.Domain.Repositories.Base;

namespace ProductServiceApp.Domain.Repositories.Products;

public interface IProductsQueryRepository<T> : IBaseQueryRepository<T> where T : class
{
}
