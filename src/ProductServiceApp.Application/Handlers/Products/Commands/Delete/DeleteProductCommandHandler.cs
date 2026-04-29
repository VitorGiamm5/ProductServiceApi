using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Products.Commands.Delete;

public class DeleteProductCommandHandler(
        Channel<(DeleteProductCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseCommandHandler<DeleteProductCommand, BooleanResponse>(channel, scopeFactory)
{
    protected override async Task<BooleanResponse> ExecuteCommandAsync(
        DeleteProductCommand command,
        IServiceProvider services,
        CancellationToken ct)
    {
        var business = services.GetRequiredService<IDeleteProductBusiness>();

        return await business.ExecuteAsync(command, ct);
    }
}
