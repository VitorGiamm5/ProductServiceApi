using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Products.Commands;

public class ProductCommandRepository : BaseCommandRepository<ProductEntity>, IProductCommandRepository<ProductEntity>
{
    public ProductCommandRepository(ApplicationDbContext context) : base(context)
    {
    }
}
