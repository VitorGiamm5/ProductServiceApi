using ProductServiceApp.Domain.Entities.Products;

namespace ProductServiceApp.Application.Cache.Products;

public interface IProductCacheService
{
    Task<ProductEntity[]?> GetAllAsync(CancellationToken cancellationToken);
    Task SetAllAsync(IEnumerable<ProductEntity> products, CancellationToken cancellationToken);
    Task<ProductEntity?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task SetByIdAsync(ProductEntity product, CancellationToken cancellationToken);
    Task InvalidateAllAsync(CancellationToken cancellationToken);
    Task InvalidateByIdAsync(long id, CancellationToken cancellationToken);
}
