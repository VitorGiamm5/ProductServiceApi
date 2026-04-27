using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products;

public class CreateProductBusiness(
    IProductCommandRepository<ProductEntity> repository)
    : BaseBusinessService<CreateProductCommand, ProductResponse>(),
    ICreateProductBusiness
{
    protected override Task<CreateProductCommand> PreProcessAsync(
    CreateProductCommand input, CancellationToken ct)
    {
        return Task.FromResult(input);
    }

    protected override Task<ProductResponse> ProcessAsync(
        CreateProductCommand input, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    protected override Task<ProductResponse> PostProcessAsync(
    ProductResponse result, CancellationToken ct)
    {
        return Task.FromResult(result);
    }

}
