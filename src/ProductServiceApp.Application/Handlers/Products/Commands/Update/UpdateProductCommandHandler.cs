using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Services.Products.Business;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Handlers.Products.Commands.Update;

public class UpdateProductCommandHandler(
        Channel<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseCommandHandler<UpdateProductCommand, ProductResponse>(channel, scopeFactory)
{
    protected override async Task<ProductResponse> ExecuteCommandAsync(
        UpdateProductCommand command,
        IServiceProvider services,
        CancellationToken ct)
    {
        var business = services.GetRequiredService<IUpdateProductBusiness>();

        return await business.ExecuteAsync(command, ct);
    }
}
