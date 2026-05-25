using Microsoft.EntityFrameworkCore;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Exceptions;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Products.Commands;

public class ProductCommandRepository(ApplicationDbContext context) : BaseCommandRepository<ProductEntity>(context), IProductCommandRepository<ProductEntity>
{
    public override async Task<ProductEntity> UpdateAsync(ProductEntity entity, long id, CancellationToken cancellationToken)
    {
        var existing = await _context.Set<ProductEntity>()
            .AsTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductEntity), id);

        existing.Name = entity.Name;
        existing.Price = entity.Price;
        existing.Type = entity.Type;
        existing.IsActive = entity.IsActive;
        existing.IsDeleted = entity.IsDeleted;
        existing.UpdatedDate = entity.UpdatedDate;
        existing.UpdatedByUserId = entity.UpdatedByUserId;

        await _context.SaveChangesAsync(cancellationToken);

        return existing;
    }
}
