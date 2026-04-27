using Asp.Versioning;
using ProductServiceApp.Api.Controllers.Base;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
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
    protected override GetAllProductQuery BuildGetAllQuery() => new();
    protected override GetByIdProductQuery BuildGetByIdQuery(long id) => new(id);
    protected override CreateProductCommand BuildCreateCommand(CreateProductRequest request) => new(request);
    protected override UpdateProductCommand BuildUpdateCommand(long id, UpdateProductRequest request) => new(request);
    protected override DeleteProductCommand BuildDeleteCommand(long id) => new(id);
}
