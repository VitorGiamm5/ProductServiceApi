using ProductServiceApp.Domain.Products.Entities;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Products.Queries;

public class ProductsQueryRepository : BaseQueryDb<ProductEntity>, IProductsQueryRepository<ProductEntity>
{
    public ProductsQueryRepository(ReadOnlyDbContext context) : base(context)
    {
    }

    public async Task<ProductEntity> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ProductEntity>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

}
