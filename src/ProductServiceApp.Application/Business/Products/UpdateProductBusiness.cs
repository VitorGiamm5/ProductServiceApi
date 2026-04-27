using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products;

public class UpdateProductBusiness(
    IProductCommandRepository<ProductEntity> repository)
    : BaseBusinessService<UpdateProductCommand, ProductResponse>(),
    IUpdateProductBusiness
{
    protected override Task<UpdateProductCommand> PreProcessAsync(
    UpdateProductCommand input, CancellationToken ct)
    {
        return Task.FromResult(input);
    }

    protected override Task<ProductResponse> ProcessAsync(
        UpdateProductCommand input, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    protected override Task<ProductResponse> PostProcessAsync(
    ProductResponse result, CancellationToken ct)
    {
        return Task.FromResult(result);
    }
}
