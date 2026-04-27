using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products;

public class GetByIdProductBusiness(
    IProductQueryRepository<ProductEntity> repository)
    : BaseBusinessService<GetByIdProductQuery, ProductResponse>,
    IGetByIdProductBusiness
{
    protected override Task<GetByIdProductQuery> PreProcessAsync(
        GetByIdProductQuery input, CancellationToken ct)
    {
        if (input.Id <= 0)
            throw new ArgumentException("Id inválido.");

        return Task.FromResult(input);
    }

    protected override async Task<ProductResponse> ProcessAsync(
        GetByIdProductQuery input, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(input.Id, ct);

        if (entity is null)
            throw new KeyNotFoundException($"Produto {input.Id} não encontrado.");

        // TODO: return new ProductResponse().MapFrom(entity);
        return new ProductResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price,
        };
    }

    protected override Task<ProductResponse> PostProcessAsync(
        ProductResponse result, CancellationToken ct)
    {
        return Task.FromResult(result);
    }
}
