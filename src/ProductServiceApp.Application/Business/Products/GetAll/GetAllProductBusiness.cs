using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products.GetAll;

public class GetAllProductBusiness(
        IProductQueryRepository<ProductEntity> repository
    )
    : BaseBusinessService<GetAllProductQuery, GetAllProductQuery, IEnumerable<ProductEntity>, IEnumerable<ProductResponse>>,
    IGetAllProductBusiness
{
    // 1️ INBOX
    protected override async Task<GetAllProductQuery> PreProcessAsync(
        GetAllProductQuery input, CancellationToken ct)
    {
        return input;
    }

    // 2️ PROCESS
    protected override async Task<IEnumerable<ProductEntity>> ProcessAsync(
        GetAllProductQuery input, CancellationToken ct)
    {
        return await repository.GetAllAsync(ct);
    }

    // 3️ OUTBOX
    protected override async Task<IEnumerable<ProductResponse>> PostProcessAsync(
        IEnumerable<ProductEntity> result, CancellationToken ct)
    {
        return result.Select(x => new ProductResponse(x));
    }
}
