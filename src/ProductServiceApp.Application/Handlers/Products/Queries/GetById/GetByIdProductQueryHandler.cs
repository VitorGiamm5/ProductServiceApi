using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Products.Queries.GetById;

public class GetByIdProductQueryHandler(
    Channel<(GetByIdProductQuery, TaskCompletionSource<ProductResponse>, CancellationToken)> channel,
    IServiceScopeFactory scopeFactory)
    : BaseChannelHandler<GetByIdProductQuery, ProductResponse>(channel, scopeFactory)
{
    protected override async Task<ProductResponse> HandleAsync(
        GetByIdProductQuery query, IServiceProvider services, CancellationToken ct)
    {
        var business = services.GetRequiredService<IGetByIdProductBusiness>();

        return await business.ExecuteAsync(query, ct);
    }
}
