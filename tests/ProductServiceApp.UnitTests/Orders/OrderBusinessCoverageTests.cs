using Bogus;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ProductServiceApp.Application.Business.Orders.Delete;
using ProductServiceApp.Application.Business.Orders.GetAll;
using ProductServiceApp.Application.Business.Orders.GetById;
using ProductServiceApp.Application.Business.Orders.Update;
using ProductServiceApp.Application.Business.Products.GetByIdList;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Orders.AdditionalFeaturesBusiness.OrderDiscount;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace ProductServiceApp.UnitTests.Orders;

public class OrderBusinessCoverageTests
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
    public async Task DeleteOrderBusiness_Should_Validate_SoftDelete_InvalidateCache_And_Return_Success()
    {
        var order = Order();
        var command = new DeleteOrderCommand(order.Id);
        var context = OrderContext.Create();
        var business = new TestableDeleteOrderBusiness(context.CommandRepository.Object, context.Cache.Object, context.DeleteValidator.Object);
        context.DeleteValidator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.CommandRepository
            .Setup(item => item.SoftDeleteAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var preProcess = await business.PreProcessForTestAsync(command, CancellationToken.None);
        var process = await business.ProcessForTestAsync(preProcess, CancellationToken.None);
        var response = await business.PostProcessForTestAsync(process, CancellationToken.None);

        Assert.Same(command, preProcess.Input);
        Assert.Same(order, process);
        Assert.IsType<BooleanResponse>(response);
        Assert.True(response.IsSuccess);
        context.CommandRepository.Verify(item => item.SoftDeleteAsync(order.Id, It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.InvalidateAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.InvalidateByIdAsync(order.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderBusiness_Should_Throw_ValidationException_And_Not_Delete_When_Invalid()
    {
        var command = new DeleteOrderCommand(0);
        var context = OrderContext.Create();
        var business = new TestableDeleteOrderBusiness(context.CommandRepository.Object, context.Cache.Object, context.DeleteValidator.Object);
        context.DeleteValidator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure(nameof(DeleteOrderCommand.Id), "Id invalido.")]));

        var act = () => business.PreProcessForTestAsync(command, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Contains("Id invalido", exception.Message);
        context.CommandRepository.Verify(item => item.SoftDeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdOrderBusiness_Should_Return_Cached_Order_Without_Querying_Repository()
    {
        var order = OrderWithProduct();
        var query = new GetByIdOrderQuery(order.Id);
        var context = OrderContext.Create();
        var business = new TestableGetByIdOrderBusiness(context.QueryRepository.Object, context.Cache.Object, context.GetByIdValidator.Object);
        context.GetByIdValidator
            .Setup(item => item.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.Cache
            .Setup(item => item.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var response = await business.ExecuteAsync(query, CancellationToken.None);

        Assert.Equal(order.Id, response.Id);
        Assert.Single(response.Products);
        context.QueryRepository.Verify(item => item.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        context.Cache.Verify(item => item.SetByIdAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdOrderBusiness_Should_Load_Order_When_Cache_Misses()
    {
        var order = OrderWithProduct();
        var query = new GetByIdOrderQuery(order.Id);
        var context = OrderContext.Create();
        var business = new TestableGetByIdOrderBusiness(context.QueryRepository.Object, context.Cache.Object, context.GetByIdValidator.Object);
        context.GetByIdValidator
            .Setup(item => item.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.Cache
            .Setup(item => item.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);
        context.QueryRepository
            .Setup(item => item.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var preProcess = await business.PreProcessForTestAsync(query, CancellationToken.None);
        var process = await business.ProcessForTestAsync(preProcess, CancellationToken.None);
        var response = await business.PostProcessForTestAsync(process, CancellationToken.None);

        Assert.Same(query, preProcess.Input);
        Assert.Same(order, process.RetrievedOrder);
        Assert.Equal(order.Id, response.Id);
        context.QueryRepository.Verify(item => item.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.SetByIdAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllOrderBusiness_Should_Return_Cached_Orders_Without_Querying_Repository()
    {
        var orders = new[] { OrderWithProduct(), OrderWithProduct() };
        var context = OrderContext.Create();
        var business = new TestableGetAllOrderBusiness(context.QueryRepository.Object, context.Cache.Object);
        context.Cache
            .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var response = (await business.ExecuteAsync(new GetAllOrderQuery(), CancellationToken.None)).ToArray();

        Assert.Equal(orders.Select(order => order.Id).ToArray(), response.Select(order => order.Id.GetValueOrDefault()).ToArray());
        context.QueryRepository.Verify(item => item.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        context.Cache.Verify(item => item.SetAllAsync(It.IsAny<IEnumerable<OrderEntity>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllOrderBusiness_Should_Load_Orders_When_Cache_Misses()
    {
        var orders = new[] { OrderWithProduct(), OrderWithProduct() };
        var context = OrderContext.Create();
        var business = new TestableGetAllOrderBusiness(context.QueryRepository.Object, context.Cache.Object);
        context.Cache
            .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity[]?)null);
        context.QueryRepository
            .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var response = (await business.ExecuteAsync(new GetAllOrderQuery(), CancellationToken.None)).ToArray();

        Assert.Equal(orders.Select(order => order.Id).ToArray(), response.Select(order => order.Id.GetValueOrDefault()).ToArray());
        context.QueryRepository.Verify(item => item.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.SetAllAsync(It.Is<IEnumerable<OrderEntity>>(items => items.SequenceEqual(orders)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderBusiness_Should_Validate_LoadProducts_CalculateDiscount_UpdateCache_And_Return_Response()
    {
        var products = new[] { Product(ProductsTypeEnum.Sandwich), Product(ProductsTypeEnum.Refreshment) };
        var command = UpdateOrderCommand(products);
        var calculated = DiscountResult();
        var updated = OrderWithProduct(command.Id!.Value, products[0]);
        var context = OrderContext.Create();
        var loadProductsAsync = new LoadProductsAsync(context.ProductQueryRepository.Object);
        var business = new TestableUpdateOrderBusiness(
            context.CommandRepository.Object,
            context.QueryRepository.Object,
            context.Cache.Object,
            loadProductsAsync,
            context.DiscountRuleRepository.Object,
            context.Calculator.Object,
            context.UpdateValidator.Object);
        context.UpdateValidator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.QueryRepository
            .Setup(item => item.GetByIdAsync(command.Id.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Order(command.Id.Value));
        context.ProductQueryRepository
            .Setup(item => item.GetByIdsAsync(It.Is<IEnumerable<long>>(ids => ids.ToHashSet().SetEquals(products.Select(product => product.Id))), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.ToList());
        context.DiscountRuleRepository
            .Setup(item => item.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new OrderDiscountRuleEntity { HasSandwich = true, HasRefreshment = true, DiscountPercentage = 10m }]);
        context.Calculator
            .Setup(item => item.ExecuteAsync(It.Is<OrderDiscountRequest>(request => request.Products.Count == products.Length), It.IsAny<CancellationToken>()))
            .ReturnsAsync(calculated);
        context.CommandRepository
            .Setup(item => item.UpdateAsync(It.IsAny<OrderEntity>(), command.Id.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var preProcess = await business.PreProcessForTestAsync(command, CancellationToken.None);
        var process = await business.ProcessForTestAsync(preProcess, CancellationToken.None);
        var response = await business.PostProcessForTestAsync(process, CancellationToken.None);

        Assert.Same(command, preProcess.Input);
        Assert.Equal(products.Length, preProcess.Products.Count);
        Assert.Same(calculated, preProcess.OrderCalculated);
        Assert.Same(updated, process.UpdatedOrder);
        Assert.Equal(updated.Id, response.Id);
        context.CommandRepository.Verify(item => item.UpdateAsync(
            It.Is<OrderEntity>(entity =>
                entity.Id == command.Id &&
                entity.UpdatedByUserId == 0L &&
                entity.SubTotalValue == calculated.SubTotalValue &&
                entity.TotalValue == calculated.TotalValue &&
                entity.OrderProducts.Count == products.Length),
            command.Id.Value,
            It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.InvalidateAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.SetByIdAsync(updated, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OrderValidators_Should_Accept_Valid_Commands()
    {
        var product = Product();

        var createResult = await new ProductServiceApp.Application.Business.Orders.Create.CreateOrderValidator().ValidateAsync(new CreateOrderCommand(OrderRequest(product)));
        var updateResult = await new ProductServiceApp.Application.Business.Orders.Update.UpdateOrderValidator().ValidateAsync(UpdateOrderCommand([product]));
        var deleteResult = await new DeleteOrderValidator().ValidateAsync(new DeleteOrderCommand(Faker.Random.Long(1, 100_000)));
        var getByIdResult = await new GetByIdOrderValidator().ValidateAsync(new GetByIdOrderQuery(Faker.Random.Long(1, 100_000)));

        Assert.True(createResult.IsValid);
        Assert.True(updateResult.IsValid);
        Assert.True(deleteResult.IsValid);
        Assert.True(getByIdResult.IsValid);
    }

    [Fact]
    public async Task OrderValidators_Should_Report_Invalid_Commands()
    {
        var invalidCreate = new CreateOrderCommand(new CreateOrderRequest
        {
            Id = 10,
            Products = [new OrderProductRequest { ProductId = 0, Quantity = 0 }]
        });
        var invalidUpdate = new UpdateOrderCommand(new UpdateOrderRequest
        {
            Id = 0,
            Products = []
        });

        var createResult = await new ProductServiceApp.Application.Business.Orders.Create.CreateOrderValidator().ValidateAsync(invalidCreate);
        var updateResult = await new ProductServiceApp.Application.Business.Orders.Update.UpdateOrderValidator().ValidateAsync(invalidUpdate);
        var deleteResult = await new DeleteOrderValidator().ValidateAsync(new DeleteOrderCommand(0));
        var getByIdResult = await new GetByIdOrderValidator().ValidateAsync(new GetByIdOrderQuery(0));

        Assert.False(createResult.IsValid);
        Assert.Contains(createResult.Errors, error => error.ErrorMessage.Contains("Id deve ser zero"));
        Assert.Contains(createResult.Errors, error => error.ErrorMessage == "Produto invalido no pedido.");
        Assert.Contains(createResult.Errors, error => error.ErrorMessage == "A quantidade do produto deve ser maior que zero.");
        Assert.False(updateResult.IsValid);
        Assert.Contains(updateResult.Errors, error => error.ErrorMessage == "Id invalido.");
        Assert.Contains(updateResult.Errors, error => error.ErrorMessage == "Informe pelo menos um produto no pedido.");
        Assert.False(deleteResult.IsValid);
        Assert.Contains(deleteResult.Errors, error => error.ErrorMessage == "Id invalido.");
        Assert.False(getByIdResult.IsValid);
        Assert.Contains(getByIdResult.Errors, error => error.ErrorMessage == "Id invalido.");
    }

    private static ProductEntity Product(ProductsTypeEnum? type = null)
    {
        var product = ProductFaker.Generate();
        product.Type = type ?? product.Type;

        return product;
    }

    private static OrderEntity Order(long? id = null)
    {
        return new OrderEntity
        {
            Id = id ?? Faker.Random.Long(1, 100_000),
            IsActive = true,
            IsDeleted = false,
            SubTotalValue = Faker.Finance.Amount(1, 200),
            TotalValue = Faker.Finance.Amount(1, 200),
            OrdersAudit = new OrderAuditEntity { CreatedDate = DateTime.UtcNow }
        };
    }

    private static OrderEntity OrderWithProduct(long? id = null, ProductEntity? product = null)
    {
        product ??= Product();
        var order = Order(id);
        order.OrderProducts =
        [
            new OrderProductEntity
            {
                ProductId = product.Id,
                Quantity = Faker.Random.Int(1, 5),
                UnitPrice = product.Price ?? decimal.Zero,
                Product = product
            }
        ];

        return order;
    }

    private static CreateOrderRequest OrderRequest(ProductEntity product)
    {
        return new CreateOrderRequest
        {
            Id = 0,
            IsActive = true,
            IsDeleted = false,
            Products = [new OrderProductRequest { ProductId = product.Id, Quantity = Faker.Random.Int(1, 5) }]
        };
    }

    private static UpdateOrderCommand UpdateOrderCommand(IEnumerable<ProductEntity> products)
    {
        return new UpdateOrderCommand(new UpdateOrderRequest
        {
            Id = Faker.Random.Long(1, 100_000),
            IsActive = true,
            IsDeleted = false,
            Products = [.. products.Select(product => new OrderProductRequest
            {
                ProductId = product.Id,
                Quantity = Faker.Random.Int(1, 5)
            })]
        });
    }

    private static OrderDiscountResult DiscountResult()
    {
        var subTotal = Faker.Finance.Amount(20, 300);
        var discount = Faker.Finance.Amount(1, 20);

        return new OrderDiscountResult
        {
            SubTotalValue = subTotal,
            DiscountPercentage = discount,
            DiscountValue = decimal.Round(subTotal * discount / 100m, 2),
            TotalValue = subTotal - decimal.Round(subTotal * discount / 100m, 2)
        };
    }

    private sealed class OrderContext
    {
        public Mock<IOrderCommandRepository> CommandRepository { get; } = new();
        public Mock<IOrderQueryRepository> QueryRepository { get; } = new();
        public Mock<IOrderCacheService> Cache { get; } = new();
        public Mock<IProductQueryRepository<ProductEntity>> ProductQueryRepository { get; } = new();
        public Mock<IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity>> DiscountRuleRepository { get; } = new();
        public Mock<IOrderDiscountCalculator> Calculator { get; } = new();
        public Mock<IValidator<DeleteOrderCommand>> DeleteValidator { get; } = new();
        public Mock<IValidator<GetByIdOrderQuery>> GetByIdValidator { get; } = new();
        public Mock<IValidator<UpdateOrderCommand>> UpdateValidator { get; } = new();

        public static OrderContext Create()
        {
            return new OrderContext();
        }
    }

    private sealed class TestableDeleteOrderBusiness(
            IOrderCommandRepository repository,
            IOrderCacheService cache,
            IValidator<DeleteOrderCommand> validator)
        : DeleteOrderBusiness(repository, cache, validator)
    {
        public Task<DeleteOrderIntermediate> PreProcessForTestAsync(DeleteOrderCommand input, CancellationToken ct) => PreProcessAsync(input, ct);
        public Task<OrderEntity> ProcessForTestAsync(DeleteOrderIntermediate input, CancellationToken ct) => ProcessAsync(input, ct);
        public Task<BooleanResponse> PostProcessForTestAsync(OrderEntity input, CancellationToken ct) => PostProcessAsync(input, ct);
    }

    private sealed class TestableGetByIdOrderBusiness(
            IOrderQueryRepository repository,
            IOrderCacheService cache,
            IValidator<GetByIdOrderQuery> validator)
        : GetByIdOrderBusiness(repository, cache, validator)
    {
        public Task<GetByIdOrderIntermediate> PreProcessForTestAsync(GetByIdOrderQuery input, CancellationToken ct) => PreProcessAsync(input, ct);
        public Task<GetByIdOrderToPostProcess> ProcessForTestAsync(GetByIdOrderIntermediate input, CancellationToken ct) => ProcessAsync(input, ct);
        public Task<OrderResponse> PostProcessForTestAsync(GetByIdOrderToPostProcess input, CancellationToken ct) => PostProcessAsync(input, ct);
    }

    private sealed class TestableGetAllOrderBusiness(
            IOrderQueryRepository repository,
            IOrderCacheService cache)
        : GetAllOrderBusiness(repository, cache);

    private sealed class TestableUpdateOrderBusiness(
            IOrderCommandRepository repository,
            IOrderQueryRepository readRepository,
            IOrderCacheService cache,
            LoadProductsAsync loadProductsAsync,
            IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity> discountRuleRepository,
            IOrderDiscountCalculator calculator,
            IValidator<UpdateOrderCommand> validator)
        : UpdateOrderBusiness(repository, readRepository, cache, loadProductsAsync, discountRuleRepository, calculator, validator)
    {
        public Task<UpdateOrderToProcess> PreProcessForTestAsync(UpdateOrderCommand input, CancellationToken ct) => PreProcessAsync(input, ct);
        public Task<UpdateOrderToPostProcess> ProcessForTestAsync(UpdateOrderToProcess input, CancellationToken ct) => ProcessAsync(input, ct);
        public Task<OrderResponse> PostProcessForTestAsync(UpdateOrderToPostProcess input, CancellationToken ct) => PostProcessAsync(input, ct);
    }
}
