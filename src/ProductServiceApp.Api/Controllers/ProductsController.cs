using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Api.Controllers.Base.BaseCrudApiController;
using ProductServiceApp.Application.Products.Commands.CreateProduct;
using ProductServiceApp.Application.Products.Commands.DeleteProduct;
using ProductServiceApp.Application.Products.Commands.UpdateProduct;
using ProductServiceApp.Application.Products.Queries.GetAll;
using ProductServiceApp.Application.Products.Queries.GetById;
using ProductServiceApp.Domain.Products.Dtos;
using System.Threading.Channels;

namespace ProductServiceApp.Api.Controllers;

[ApiVersion("1.0")]
public class ProductsController : BaseCrudApiController<
    ProductsResponse,
    CreateProductsRequest,
    ProductsResponse,
    UpdateProductsRequest,
    ProductsResponse>
{
    private readonly Channel<(CreateProductCommand, TaskCompletionSource<ProductsResponse>, CancellationToken)> _createChannel;
    private readonly Channel<(UpdateProductCommand, TaskCompletionSource<ProductsResponse>, CancellationToken)> _updateChannel;
    private readonly Channel<(DeleteProductCommand, TaskCompletionSource<bool>, CancellationToken)> _deleteChannel;
    private readonly Channel<(GetProductByIdQuery, TaskCompletionSource<ProductsResponse>, CancellationToken)> _getByIdChannel;
    private readonly Channel<(GetAllProductsQuery, TaskCompletionSource<IEnumerable<ProductsResponse>>, CancellationToken)> _getAllChannel;

    public ProductsController(
        Channel<(CreateProductCommand, TaskCompletionSource<ProductsResponse>, CancellationToken)> createChannel,
        Channel<(UpdateProductCommand, TaskCompletionSource<ProductsResponse>, CancellationToken)> updateChannel,
        Channel<(DeleteProductCommand, TaskCompletionSource<bool>, CancellationToken)> deleteChannel,
        Channel<(GetAllProductsQuery, TaskCompletionSource<IEnumerable<ProductsResponse>>, CancellationToken)> getAllChannel,
        Channel<(GetProductByIdQuery, TaskCompletionSource<ProductsResponse>, CancellationToken)> getByIdChannel)
    {
        _createChannel = createChannel;
        _updateChannel = updateChannel;
        _deleteChannel = deleteChannel;
        _getAllChannel = getAllChannel;
        _getByIdChannel = getByIdChannel;
    }

    public override async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return await ExecuteAsync<IEnumerable<ProductsResponse>>(
            (tcs, token) => _getAllChannel.Writer
                .WriteAsync((new GetAllProductsQuery(), tcs, token), token)
                .AsTask(),
            cancellationToken);
    }

    public override async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        return await ExecuteAsync<ProductsResponse>(
            (tcs, token) => _getByIdChannel.Writer
                .WriteAsync((new GetProductByIdQuery(id), tcs, token), token)
                .AsTask(),
            cancellationToken);
    }

    public override async Task<IActionResult> Create([FromBody] CreateProductsRequest request, CancellationToken cancellationToken)
    {
        request.Id = 0; // Ensure ID is not set by the client

        return await ExecuteAsync<ProductsResponse>(
            (tcs, token) => _createChannel.Writer
                .WriteAsync((new CreateProductCommand(request), tcs, token), token)
                .AsTask(),
            cancellationToken);
    }

    public override async Task<IActionResult> Update(long id, [FromBody] UpdateProductsRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteAsync<ProductsResponse>(
            (tcs, token) => _updateChannel.Writer
                .WriteAsync((new UpdateProductCommand(request), tcs, token), token)
                .AsTask(),
            cancellationToken);
    }

    public override async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        return await ExecuteAsync<bool>(
            (tcs, token) => _deleteChannel.Writer
                .WriteAsync((new DeleteProductCommand(id), tcs, token), token)
                .AsTask(),
            cancellationToken);
    }
}
