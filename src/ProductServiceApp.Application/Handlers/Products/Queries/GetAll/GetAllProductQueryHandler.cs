using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Products.Queries.GetAll;

public class GetAllProductQueryHandler(
    Channel<(GetAllProductQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)> channel,
    IServiceScopeFactory scopeFactory)
    : BaseChannelHandler<GetAllProductQuery, IEnumerable<ProductResponse>>(channel, scopeFactory)
{
    protected override async Task<IEnumerable<ProductResponse>> HandleAsync(
        GetAllProductQuery query, IServiceProvider services, CancellationToken ct)
    {
        var business = services.GetRequiredService<IGetAllProductBusiness>();

        return await business.ExecuteAsync(query, ct);
    }
}