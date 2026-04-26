using ProductServiceApp.Domain.Products.Entities;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Products.Commands;

public class ProductsCommandRepository : BaseCommandDb<ProductEntity>, IProductsCommandRepository<ProductEntity>
{
    public ProductsCommandRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ProductEntity> CreateAsync(ProductEntity entity)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<ProductEntity> UpdateAsync(ProductEntity entity)
    {
        throw new NotImplementedException();
    }
}
