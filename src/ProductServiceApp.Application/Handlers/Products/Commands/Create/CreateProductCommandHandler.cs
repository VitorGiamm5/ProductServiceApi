using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Services.Products.Business;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Handlers.Products.Commands.Create;

public class CreateProductCommandHandler(
        Channel<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseCommandHandler<CreateProductCommand, ProductResponse>(channel, scopeFactory)
{
    protected override async Task<ProductResponse> ExecuteCommandAsync(
        CreateProductCommand command,
        IServiceProvider services,
        CancellationToken ct)
    {
        var business = services.GetRequiredService<ICreateProductBusiness>();

        return await business.ExecuteAsync(command, ct);
    }
}
