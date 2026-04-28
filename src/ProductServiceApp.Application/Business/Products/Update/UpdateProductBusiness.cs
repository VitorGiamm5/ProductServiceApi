using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products.Update;

public class UpdateProductBusiness(
        IProductCommandRepository<ProductEntity> repository,
        IValidator<UpdateProductCommand> validator
    )
    : BaseBusinessService<UpdateProductCommand, ProductEntity, ProductEntity, ProductResponse>(),
    IUpdateProductBusiness
{
    // Inbox
    protected override async Task<ProductEntity> PreProcessAsync(
    UpdateProductCommand input, CancellationToken ct)
    {
        // Validate the input
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        // Map the main properties
        ProductEntity entity = input.MapTo();

        // Set Auditing internal properties
        entity.UpdatedDate = DateTime.UtcNow;
        entity.IsDeleted = input.IsDeleted;
        entity.UpdatedByUserId = 0;

        return entity;
    }

    // Process
    protected override async Task<ProductEntity> ProcessAsync(
        ProductEntity input, CancellationToken ct)
    {
        return await repository.UpdateAsync(input, input.Id, ct);
    }

    // Outbox
    protected override async Task<ProductResponse> PostProcessAsync(
    ProductEntity result, CancellationToken ct)
    {
        return new ProductResponse(result);
    }
}
