using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Products.Commands.Create;

public class CreateProductCommandHandler(
    Channel<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> channel,
    IServiceScopeFactory scopeFactory)
    : BaseChannelHandler<CreateProductCommand, ProductResponse>(channel, scopeFactory)
{
    protected override async Task<ProductResponse> HandleAsync(
        CreateProductCommand command,
        IServiceProvider services,
        CancellationToken ct)
    {
        var business = services.GetRequiredService<ICreateProductBusiness>();

        return await business.ExecuteAsync(command, ct);
    }
}

