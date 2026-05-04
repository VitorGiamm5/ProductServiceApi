using Microsoft.EntityFrameworkCore;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Products.Queries;

public class ProductQueryRepository : BaseQueryRepository<ProductEntity>, IProductQueryRepository<ProductEntity>
{
    public ProductQueryRepository(ReadOnlyDbContext context) : base(context)
    {
    }

    public async Task<List<ProductEntity>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken)
    {
        var idsSet = ids.ToHashSet();

        return await _context.Set<ProductEntity>()
            .Where(product => idsSet.Contains(EF.Property<long>(product, nameof(ProductEntity.Id))))
            .ToListAsync(cancellationToken);
    }
}
