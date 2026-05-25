using System.Threading.Channels;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Api.Auth;
using ProductServiceApp.Api.Controllers.Base;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

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
    [Authorize(Policy = AuthPolicies.ProductsRead)]
    public override Task<IActionResult> GetAll(CancellationToken ct) => base.GetAll(ct);

    [Authorize(Policy = AuthPolicies.ProductsRead)]
    public override Task<IActionResult> GetById(long id, CancellationToken ct) => base.GetById(id, ct);

    [Authorize(Policy = AuthPolicies.ProductsWrite)]
    public override Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct) => base.Create(request, ct);

    [Authorize(Policy = AuthPolicies.ProductsWrite)]
    public override Task<IActionResult> Update(long id, UpdateProductRequest request, CancellationToken ct) => base.Update(id, request, ct);

    [Authorize(Policy = AuthPolicies.ProductsWrite)]
    public override Task<IActionResult> Delete(long id, CancellationToken ct) => base.Delete(id, ct);

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
