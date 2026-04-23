using ProductServiceApp.Domain.Repositories.Base;

namespace ProductServiceApp.Domain.Repositories.Products;

public interface IProductCommandRepository<T> : IBaseCommandRepository<T> where T : class
{

}
