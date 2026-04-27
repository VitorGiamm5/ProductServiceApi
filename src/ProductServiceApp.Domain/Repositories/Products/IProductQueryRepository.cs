using ProductServiceApp.Domain.Repositories.Base;

namespace ProductServiceApp.Domain.Repositories.Products;

public interface IProductQueryRepository<TEntity> : IBaseQueryRepository<TEntity> where TEntity : class
{
}
