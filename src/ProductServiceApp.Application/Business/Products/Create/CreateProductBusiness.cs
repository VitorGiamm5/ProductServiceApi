using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products.Create;

public class CreateProductBusiness(
        IProductCommandRepository<ProductEntity> repository,
        IValidator<CreateProductCommand> validator)
    : BaseBusinessService<CreateProductCommand, ProductEntity, ProductEntity, ProductResponse>,
    ICreateProductBusiness
{
    // 1️ INBOX — valida dados e devolve o comando (tipo esperado pelo base)
    protected override async Task<ProductEntity> PreProcessAsync(
        CreateProductCommand input, CancellationToken ct)
    {
        // validation
        var validation = await validator.ValidateAsync(input, ct);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        // Map the main properties
        ProductEntity entity = input.MapTo();

        // Set Auditing internal properties
        entity.CreatedDate = DateTime.UtcNow;
        entity.IsDeleted = false;
        entity.CreatedByUserId = 0;

        return entity;
    }

    // 2️ PROCESS — persistence
    protected override async Task<ProductEntity> ProcessAsync(
        ProductEntity input, CancellationToken ct)
    {
        return await repository.CreateAsync(input, ct);
    }

    // 3️ OUTBOX
    protected override async Task<ProductResponse> PostProcessAsync(
        ProductEntity result, CancellationToken ct)
    {
        return new ProductResponse(result);
    }
}