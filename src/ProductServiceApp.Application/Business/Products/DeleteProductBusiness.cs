using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products;

public class DeleteProductBusiness(
    IProductCommandRepository<ProductEntity> repository)
    : BaseBusinessService<DeleteProductCommand, BooleanResponse>(),
    IDeleteProductBusiness
{
    protected override Task<DeleteProductCommand> PreProcessAsync(
    DeleteProductCommand input, CancellationToken ct)
    {
        return Task.FromResult(input);
    }

    protected override Task<BooleanResponse> ProcessAsync(
        DeleteProductCommand input, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    protected override Task<BooleanResponse> PostProcessAsync(
    BooleanResponse result, CancellationToken ct)
    {
        return Task.FromResult(result);
    }
}
