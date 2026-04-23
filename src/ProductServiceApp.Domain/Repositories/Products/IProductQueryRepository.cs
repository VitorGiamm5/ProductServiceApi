using ProductServiceApp.Domain.Repositories.Base;

namespace ProductServiceApp.Domain.Repositories.Products;

public interface IProductQueryRepository<T> : IBaseQueryRepository<T> where T : class
{
}
