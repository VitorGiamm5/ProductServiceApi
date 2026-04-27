using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products;

public class GetAllProductBusiness(
    IProductQueryRepository<ProductEntity> repository)
    : BaseBusinessService<GetAllProductQuery, IEnumerable<ProductResponse>>,
    IGetAllProductBusiness
{
    /// <summary>
    /// // 1️ INBOX — validation, enrichment, etc. before processing the main logic
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected override Task<GetAllProductQuery> PreProcessAsync(
        GetAllProductQuery input, CancellationToken ct)
    {
        return Task.FromResult(input);
    }

    /// <summary>
    /// 2️ PROCESS — the main logic of the business service, e.g., database operations, external API calls, etc.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected override async Task<IEnumerable<ProductResponse>> ProcessAsync(
        GetAllProductQuery input, CancellationToken ct)
    {
        var entities = await repository.GetAllAsync(ct);

        //TODO: return entities.Select(e => new ProductResponse().MapFrom(e));
        return entities.Select(e => new ProductResponse
        {
            Id = e.Id,
            Name = e.Name,
            Price = e.Price,
        });
    }

    /// <summary>
    /// 3️ OUTBOX — actions to be taken after the main processing, e.g., logging, notifications, etc.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected override Task<IEnumerable<ProductResponse>> PostProcessAsync(
        IEnumerable<ProductResponse> result, CancellationToken ct)
    {
        return Task.FromResult(result);
    }
}