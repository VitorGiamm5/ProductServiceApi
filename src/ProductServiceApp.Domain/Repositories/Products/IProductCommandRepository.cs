using ProductServiceApp.Domain.Repositories.Base;

namespace ProductServiceApp.Domain.Repositories.Products;

public interface IProductCommandRepository<TEntity> : IBaseCommandRepository<TEntity> where TEntity : class
{
}
