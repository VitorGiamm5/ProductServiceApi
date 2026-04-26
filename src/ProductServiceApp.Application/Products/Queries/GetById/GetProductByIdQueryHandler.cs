using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Application.Products.Commands.Create;
using ProductServiceApp.Domain.Products;
using ProductServiceApp.Domain.Products.Dtos;
using ProductServiceApp.Domain.Products.Entities;
using ProductServiceApp.Domain.Repositories.Products;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Products.Queries.GetById;

public class GetProductByIdQueryHandler : BackgroundService
{
    private readonly Channel<(GetProductByIdQuery, TaskCompletionSource<ProductResponse>, CancellationToken)> _channel;
    private readonly IServiceScopeFactory _scopeFactory;


    public GetProductByIdQueryHandler(
        Channel<(GetProductByIdQuery, TaskCompletionSource<ProductResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory
        )
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
                var repository = scope.ServiceProvider.GetRequiredService<IProductQueryRepository<ProductEntity>>();

                var entity = await repository.GetByIdAsync(query.Id, cts.Token);

                var response = new ProductResponse
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    CreatedDate = entity.CreatedDate,
                    Price = entity.Price,
                    Type = entity.Type
                };

                tcs.TrySetResult(response);
            }
            catch (DbUpdateException ex)
            {
                tcs.TrySetException(ex);
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
