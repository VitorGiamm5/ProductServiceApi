using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Domain.Products;
using ProductServiceApp.Domain.Products.Dtos;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Products.Queries.GetAll;

public class GetAllProductsQueryHandler : BackgroundService
{
    private readonly Channel<(GetAllProductsQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)> _channel;
    private readonly IServiceScopeFactory _scopeFactory;

    public GetAllProductsQueryHandler(
        Channel<(GetAllProductsQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (query, tcs, requestToken) in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            if (requestToken.IsCancellationRequested)
            {
                tcs.TrySetCanceled(requestToken);
                continue;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, requestToken);

                await using var scope = _scopeFactory.CreateAsyncScope();

                ProductResponse response = new()
                {
                    Id = 1,
                    Name = "Sample Product",
                    Price = 100.0m,
                    Type = ProductTypeEnum.Fries
                };

                tcs.TrySetResult(new[] { response });
            }
            catch (OperationCanceledException ex)
            {
                tcs.TrySetCanceled(ex.CancellationToken);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }
    }
}