using Asp.Versioning;
using ProductServiceApp.Api.Controllers.Base;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Api.Controllers.Products;

[ApiVersion("1.0")]
public class ProductController(
    Channel<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> createChannel,
    Channel<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> updateChannel,
    Channel<(DeleteProductCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)> deleteChannel,
    Channel<(GetAllProductQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)> getAllChannel,
    Channel<(GetByIdProductQuery, TaskCompletionSource<ProductResponse>, CancellationToken)> getByIdChannel)
    : BaseCrudApiController<
        ProductResponse,
        CreateProductRequest, ProductResponse, CreateProductCommand,
        UpdateProductRequest, ProductResponse, UpdateProductCommand,
        DeleteProductCommand, BooleanResponse,
        GetByIdProductQuery,
        GetAllProductQuery>(
        createChannel,
        updateChannel,
        deleteChannel,
        getAllChannel,
        getByIdChannel)
{
    protected override GetAllProductQuery BuildGetAllQuery()
    {
        return new();
    }

    protected override GetByIdProductQuery BuildGetByIdQuery(long id)
    {
        return new(id);
    }

    protected override CreateProductCommand BuildCreateCommand(CreateProductRequest request)
    {
        return new(request);
    }

    protected override UpdateProductCommand BuildUpdateCommand(long id, UpdateProductRequest request)
    {
        request.Id = id;

        return new(request);
    }

    protected override DeleteProductCommand BuildDeleteCommand(long id)
    {
        return new(id);
    }
}
