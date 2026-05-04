using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products.GetByIdList;

public class LoadProductsAsync(IProductQueryRepository<ProductEntity> productRepository)
{
    public async Task<List<ProductEntity>> ExecuteAsync(IEnumerable<long> productIds, CancellationToken ct)
    {
        var ids = productIds.Distinct().ToHashSet();
        var products = await productRepository.GetByIdsAsync(ids, ct);

        if (products.Count != ids.Count)
        {
            throw new ArgumentException("Um ou mais produtos informados no pedido nao foram encontrados.");
        }

        return products;
    }
}
