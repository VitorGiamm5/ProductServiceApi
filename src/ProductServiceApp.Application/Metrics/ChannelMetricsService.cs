using Microsoft.Extensions.Hosting;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Metrics;

public class ChannelMetricsService(
        Channel<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> createChannel,
        Channel<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> updateChannel,
        Channel<(DeleteProductCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)> deleteChannel,
        Channel<(GetAllProductQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)> getAllChannel,
        Channel<(GetByIdProductQuery, TaskCompletionSource<ProductResponse>, CancellationToken)> getByIdChannel)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            AppMetrics.ChannelQueueSize.WithLabels("create").Set(createChannel.Reader.Count);
            AppMetrics.ChannelQueueSize.WithLabels("update").Set(updateChannel.Reader.Count);
            AppMetrics.ChannelQueueSize.WithLabels("delete").Set(deleteChannel.Reader.Count);
            AppMetrics.ChannelQueueSize.WithLabels("getAll").Set(getAllChannel.Reader.Count);
            AppMetrics.ChannelQueueSize.WithLabels("getById").Set(getByIdChannel.Reader.Count);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
