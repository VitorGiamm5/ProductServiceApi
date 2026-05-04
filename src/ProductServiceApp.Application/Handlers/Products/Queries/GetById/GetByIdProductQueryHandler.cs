using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Services.Products.Business;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Products.Queries.GetById;

public class GetByIdProductQueryHandler(
        Channel<(GetByIdProductQuery, TaskCompletionSource<ProductResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseQueryHandler<GetByIdProductQuery, ProductResponse>(channel, scopeFactory)
{
    protected override async Task<ProductResponse> ExecuteQueryAsync(
        GetByIdProductQuery query, IServiceProvider services, CancellationToken ct)
    {
        var business = services.GetRequiredService<IGetByIdProductBusiness>();

        return await business.ExecuteAsync(query, ct);
    }
}
