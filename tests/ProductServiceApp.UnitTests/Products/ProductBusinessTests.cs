using Bogus;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ProductServiceApp.Application.Business.Products.Create;
using ProductServiceApp.Application.Business.Products.Delete;
using ProductServiceApp.Application.Business.Products.GetAll;
using ProductServiceApp.Application.Business.Products.GetById;
using ProductServiceApp.Application.Business.Products.Update;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace ProductServiceApp.UnitTests.Products;

public class ProductBusinessTests
{
    private static readonly Faker Faker = new("pt_BR");

    private static readonly Faker<ProductEntity> ProductFaker = new Faker<ProductEntity>("pt_BR")
        .RuleFor(product => product.Id, faker => faker.Random.Long(1, 100_000))
        .RuleFor(product => product.Name, faker => faker.Commerce.ProductName())
        .RuleFor(product => product.Price, faker => faker.Finance.Amount(1, 200))
        .RuleFor(product => product.Type, faker => faker.PickRandom<ProductsTypeEnum>())
        .RuleFor(product => product.IsActive, true)
        .RuleFor(product => product.IsDeleted, false);

    [Fact]
    public async Task CreateProductBusiness_Should_Validate_Create_InvalidateCache_And_Return_Response()
    {
        var request = ProductRequest();
        var command = new CreateProductCommand(request);
        var created = Product(command.Id!.Value);
        var context = ProductContext.Create();
        var business = new TestableCreateProductBusiness(context.Repository.Object, context.Cache.Object, context.CreateValidator.Object);
        context.CreateValidator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.Repository
            .Setup(item => item.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var preProcess = await business.PreProcessForTestAsync(command, CancellationToken.None);
        var process = await business.ProcessForTestAsync(preProcess, CancellationToken.None);
        var response = await business.PostProcessForTestAsync(process, CancellationToken.None);

        Assert.Same(command, preProcess.Input);
        Assert.InRange(preProcess.CreatedDate, DateTime.UtcNow.AddSeconds(-10), DateTime.UtcNow.AddSeconds(10));
        Assert.Same(created, process.CreatedProduct);
        Assert.Equal(created.Id, response.Id);
        Assert.Equal(created.Name, response.Name);
        Assert.Equal(created.Price, response.Price);
        Assert.Equal(created.Type, response.Type);
        context.Repository.Verify(item => item.CreateAsync(
            It.Is<ProductEntity>(entity =>
                entity.Id == command.Id &&
                entity.Name == command.Name &&
                entity.Price == command.Price &&
                entity.Type == command.Type &&
                entity.CreatedByUserId == 0L &&
                entity.IsDeleted == false),
            It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.InvalidateAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.SetByIdAsync(created, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductBusiness_Should_Throw_ValidationException_And_Not_Call_Repository_When_Invalid()
    {
        var command = new CreateProductCommand(ProductRequest());
        var context = ProductContext.Create();
        var business = new TestableCreateProductBusiness(context.Repository.Object, context.Cache.Object, context.CreateValidator.Object);
        context.CreateValidator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure(nameof(CreateProductCommand.Name), "Nome invalido.")]));

        var act = () => business.PreProcessForTestAsync(command, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Contains("Nome invalido", exception.Message);
        context.Repository.Verify(item => item.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductBusiness_Should_Validate_Update_InvalidateCache_And_Return_Response()
    {
        var request = ProductUpdateRequest();
        var command = new UpdateProductCommand(request);
        var updated = Product(command.Id!.Value);
        var context = ProductContext.Create();
        var business = new TestableUpdateProductBusiness(context.Repository.Object, context.Cache.Object, context.UpdateValidator.Object);
        context.UpdateValidator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.Repository
            .Setup(item => item.UpdateAsync(It.IsAny<ProductEntity>(), command.Id.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var preProcess = await business.PreProcessForTestAsync(command, CancellationToken.None);
        var process = await business.ProcessForTestAsync(preProcess, CancellationToken.None);
        var response = await business.PostProcessForTestAsync(process, CancellationToken.None);

        Assert.Same(command, preProcess.Input);
        Assert.Same(updated, process.UpdatedProduct);
        Assert.Equal(updated.Id, response.Id);
        context.Repository.Verify(item => item.UpdateAsync(
            It.Is<ProductEntity>(entity =>
                entity.Id == command.Id &&
                entity.Name == command.Name &&
                entity.Price == command.Price &&
                entity.Type == command.Type &&
                entity.UpdatedByUserId == 0L),
            command.Id.Value,
            It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.InvalidateAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.SetByIdAsync(updated, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductBusiness_Should_Load_Delete_InvalidateCache_And_Return_Success()
    {
        var product = Product();
        var command = new DeleteProductCommand(product.Id);
        var context = ProductContext.Create();
        var business = new TestableDeleteProductBusiness(context.Repository.Object, context.Query.Object, context.Cache.Object, context.DeleteValidator.Object);
        context.DeleteValidator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.Query
            .Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        context.Repository
            .Setup(item => item.DeleteAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var preProcess = await business.PreProcessForTestAsync(command, CancellationToken.None);
        var process = await business.ProcessForTestAsync(preProcess, CancellationToken.None);
        var response = await business.PostProcessForTestAsync(process, CancellationToken.None);

        Assert.Same(command, preProcess.Input);
        Assert.Same(product, preProcess.ProductToDelete);
        Assert.Same(product, process.DeletedProduct);
        Assert.IsType<BooleanResponse>(response);
        Assert.True(response.IsSuccess);
        context.Repository.Verify(item => item.DeleteAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.InvalidateAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.InvalidateByIdAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductBusiness_Should_Throw_When_Product_Not_Found()
    {
        var command = new DeleteProductCommand(Faker.Random.Long(1, 100_000));
        var context = ProductContext.Create();
        var business = new TestableDeleteProductBusiness(context.Repository.Object, context.Query.Object, context.Cache.Object, context.DeleteValidator.Object);
        context.DeleteValidator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.Query
            .Setup(item => item.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity)null!);

        var act = () => business.PreProcessForTestAsync(command, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains($"Produto {command.Id} nao encontrado", exception.Message);
        context.Repository.Verify(item => item.DeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdProductBusiness_Should_Return_Cached_Product_Without_Querying_Repository()
    {
        var product = Product();
        var query = new GetByIdProductQuery(product.Id);
        var context = ProductContext.Create();
        var business = new TestableGetByIdProductBusiness(context.Query.Object, context.Cache.Object, context.GetByIdValidator.Object);
        context.GetByIdValidator
            .Setup(item => item.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.Cache
            .Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var preProcess = await business.PreProcessForTestAsync(query, CancellationToken.None);
        var process = await business.ProcessForTestAsync(preProcess, CancellationToken.None);
        var response = await business.PostProcessForTestAsync(process, CancellationToken.None);

        Assert.Same(query, preProcess.Input);
        Assert.Same(product, process.RetrievedProduct);
        Assert.Equal(product.Id, response.Id);
        context.Query.Verify(item => item.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        context.Cache.Verify(item => item.SetByIdAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdProductBusiness_Should_Load_Product_When_Cache_Misses()
    {
        var product = Product();
        var query = new GetByIdProductQuery(product.Id);
        var context = ProductContext.Create();
        var business = new TestableGetByIdProductBusiness(context.Query.Object, context.Cache.Object, context.GetByIdValidator.Object);
        context.GetByIdValidator
            .Setup(item => item.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.Cache
            .Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);
        context.Query
            .Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var response = await business.ExecuteAsync(query, CancellationToken.None);

        Assert.Equal(product.Id, response.Id);
        context.Query.Verify(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.SetByIdAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdProductBusiness_Should_Throw_When_Product_Not_Found()
    {
        var query = new GetByIdProductQuery(Faker.Random.Long(1, 100_000));
        var context = ProductContext.Create();
        var business = new TestableGetByIdProductBusiness(context.Query.Object, context.Cache.Object, context.GetByIdValidator.Object);
        context.GetByIdValidator
            .Setup(item => item.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.Query
            .Setup(item => item.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity)null!);

        var act = () => business.ExecuteAsync(query, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains($"Produto {query.Id} nao encontrado", exception.Message);
    }

    [Fact]
    public async Task GetAllProductBusiness_Should_Return_Cached_Products_Without_Querying_Repository()
    {
        var products = ProductFaker.Generate(2).ToArray();
        var context = ProductContext.Create();
        var business = new TestableGetAllProductBusiness(context.Query.Object, context.Cache.Object);
        context.Cache
            .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var response = (await business.ExecuteAsync(new GetAllProductQuery(), CancellationToken.None)).ToArray();

        Assert.Equal(products.Length, response.Length);
        Assert.Equal(products.Select(product => product.Id).ToArray(), response.Select(product => product.Id.GetValueOrDefault()).ToArray());
        context.Query.Verify(item => item.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        context.Cache.Verify(item => item.SetAllAsync(It.IsAny<IEnumerable<ProductEntity>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllProductBusiness_Should_Load_Products_When_Cache_Misses()
    {
        var products = ProductFaker.Generate(2).ToArray();
        var context = ProductContext.Create();
        var business = new TestableGetAllProductBusiness(context.Query.Object, context.Cache.Object);
        context.Cache
            .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity[]?)null);
        context.Query
            .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var response = (await business.ExecuteAsync(new GetAllProductQuery(), CancellationToken.None)).ToArray();

        Assert.Equal(products.Select(product => product.Id).ToArray(), response.Select(product => product.Id.GetValueOrDefault()).ToArray());
        context.Query.Verify(item => item.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.SetAllAsync(It.Is<IEnumerable<ProductEntity>>(items => items.SequenceEqual(products)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProductValidators_Should_Accept_Valid_Commands()
    {
        var product = Product();
        var query = new Mock<IProductQueryRepository<ProductEntity>>();
        query.Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var createResult = await new CreateProductValidator().ValidateAsync(new CreateProductCommand(ProductRequest()));
        var updateResult = await new UpdateProductValidator().ValidateAsync(new UpdateProductCommand(ProductUpdateRequest()));
        var getByIdResult = await new GetByIdProductValidator(query.Object).ValidateAsync(new GetByIdProductQuery(product.Id));
        var deleteResult = await new DeleteProductValidator(query.Object).ValidateAsync(new DeleteProductCommand(product.Id));

        Assert.True(createResult.IsValid);
        Assert.True(updateResult.IsValid);
        Assert.True(getByIdResult.IsValid);
        Assert.True(deleteResult.IsValid);
    }

    [Fact]
    public async Task ProductValidators_Should_Report_Invalid_Commands()
    {
        var query = new Mock<IProductQueryRepository<ProductEntity>>();
        query.Setup(item => item.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity)null!);

        var createResult = await new CreateProductValidator().ValidateAsync(new CreateProductCommand(new CreateProductRequest
        {
            Name = "",
            Price = 0m,
            Type = unchecked((ProductsTypeEnum)999)
        }));
        var updateResult = await new UpdateProductValidator().ValidateAsync(new UpdateProductCommand(new UpdateProductRequest { Id = 0 }));
        var missingProductId = Faker.Random.Long(1, 100_000);
        var getByIdResult = await new GetByIdProductValidator(query.Object).ValidateAsync(new GetByIdProductQuery(missingProductId));
        var deleteResult = await new DeleteProductValidator(query.Object).ValidateAsync(new DeleteProductCommand(0));

        Assert.False(createResult.IsValid);
        Assert.Contains(createResult.Errors, error => error.ErrorMessage == "Nome é obrigatório.");
        Assert.Contains(createResult.Errors, error => error.ErrorMessage == "Preço deve ser maior que zero.");
        Assert.Contains(createResult.Errors, error => error.ErrorMessage == "Tipo de produto inválido.");
        Assert.False(updateResult.IsValid);
        Assert.Contains(updateResult.Errors, error => error.ErrorMessage == "Id inválido.");
        Assert.False(getByIdResult.IsValid);
        Assert.Contains(getByIdResult.Errors, error => error.ErrorMessage == $"Produto {missingProductId} não encontrado.");
        Assert.False(deleteResult.IsValid);
        Assert.Contains(deleteResult.Errors, error => error.ErrorMessage == "Id inválido.");
    }

    private static CreateProductRequest ProductRequest()
    {
        return new CreateProductRequest
        {
            Id = Faker.Random.Long(1, 100_000),
            Name = Faker.Commerce.ProductName(),
            Price = Faker.Finance.Amount(1, 200),
            Type = Faker.PickRandom<ProductsTypeEnum>(),
            IsActive = true,
            IsDeleted = false
        };
    }

    private static UpdateProductRequest ProductUpdateRequest()
    {
        var request = ProductRequest();

        return new UpdateProductRequest
        {
            Id = request.Id,
            Name = request.Name,
            Price = request.Price,
            Type = request.Type,
            IsActive = request.IsActive,
            IsDeleted = request.IsDeleted
        };
    }

    private static ProductEntity Product(long? id = null)
    {
        var product = ProductFaker.Generate();
        product.Id = id ?? product.Id;

        return product;
    }

    private sealed class ProductContext
    {
        public Mock<IProductCommandRepository<ProductEntity>> Repository { get; } = new();
        public Mock<IProductQueryRepository<ProductEntity>> Query { get; } = new();
        public Mock<IProductCacheService> Cache { get; } = new();
        public Mock<IValidator<CreateProductCommand>> CreateValidator { get; } = new();
        public Mock<IValidator<UpdateProductCommand>> UpdateValidator { get; } = new();
        public Mock<IValidator<DeleteProductCommand>> DeleteValidator { get; } = new();
        public Mock<IValidator<GetByIdProductQuery>> GetByIdValidator { get; } = new();

        public static ProductContext Create()
        {
            return new ProductContext();
        }
    }

    private sealed class TestableCreateProductBusiness(
            IProductCommandRepository<ProductEntity> repository,
            IProductCacheService cache,
            IValidator<CreateProductCommand> validator)
        : CreateProductBusiness(repository, cache, validator)
    {
        public Task<CreateProductToProcess> PreProcessForTestAsync(CreateProductCommand input, CancellationToken ct) => PreProcessAsync(input, ct);
        public Task<CreateProductToPostProcess> ProcessForTestAsync(CreateProductToProcess input, CancellationToken ct) => ProcessAsync(input, ct);
        public Task<ProductResponse> PostProcessForTestAsync(CreateProductToPostProcess input, CancellationToken ct) => PostProcessAsync(input, ct);
    }

    private sealed class TestableUpdateProductBusiness(
            IProductCommandRepository<ProductEntity> repository,
            IProductCacheService cache,
            IValidator<UpdateProductCommand> validator)
        : UpdateProductBusiness(repository, cache, validator)
    {
        public Task<UpdateProductToProcess> PreProcessForTestAsync(UpdateProductCommand input, CancellationToken ct) => PreProcessAsync(input, ct);
        public Task<UpdateProductToPostProcess> ProcessForTestAsync(UpdateProductToProcess input, CancellationToken ct) => ProcessAsync(input, ct);
        public Task<ProductResponse> PostProcessForTestAsync(UpdateProductToPostProcess input, CancellationToken ct) => PostProcessAsync(input, ct);
    }

    private sealed class TestableDeleteProductBusiness(
            IProductCommandRepository<ProductEntity> repository,
            IProductQueryRepository<ProductEntity> read,
            IProductCacheService cache,
            IValidator<DeleteProductCommand> validator)
        : DeleteProductBusiness(repository, read, cache, validator)
    {
        public Task<DeleteProductToProcess> PreProcessForTestAsync(DeleteProductCommand input, CancellationToken ct) => PreProcessAsync(input, ct);
        public Task<DeleteProductToPostProcess> ProcessForTestAsync(DeleteProductToProcess input, CancellationToken ct) => ProcessAsync(input, ct);
        public Task<BooleanResponse> PostProcessForTestAsync(DeleteProductToPostProcess input, CancellationToken ct) => PostProcessAsync(input, ct);
    }

    private sealed class TestableGetByIdProductBusiness(
            IProductQueryRepository<ProductEntity> repository,
            IProductCacheService cache,
            IValidator<GetByIdProductQuery> validator)
        : GetByIdProductBusiness(repository, cache, validator)
    {
        public Task<GetByIdProductToProcess> PreProcessForTestAsync(GetByIdProductQuery input, CancellationToken ct) => PreProcessAsync(input, ct);
        public Task<GetByIdProductToPostProcess> ProcessForTestAsync(GetByIdProductToProcess input, CancellationToken ct) => ProcessAsync(input, ct);
        public Task<ProductResponse> PostProcessForTestAsync(GetByIdProductToPostProcess input, CancellationToken ct) => PostProcessAsync(input, ct);
    }

    private sealed class TestableGetAllProductBusiness(
            IProductQueryRepository<ProductEntity> repository,
            IProductCacheService cache)
        : GetAllProductBusiness(repository, cache);
}
