using ProductServiceApp.Domain.Products.Entities;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Products.Queries;

public class ProductQueryRepository : BaseQueryRepository<ProductEntity>, IProductQueryRepository<ProductEntity>
{
    public ProductQueryRepository(ReadOnlyDbContext context) : base(context)
    {
    }
}
