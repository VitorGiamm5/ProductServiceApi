using Bogus;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ProductServiceApp.Application.Business.Orders.Create;
using ProductServiceApp.Application.Business.Products.GetByIdList;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Orders.AdditionalFeaturesBusiness.OrderDiscount;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;
using ProductServiceApp.Domain.Services.Products.Dtos;
using Xunit;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace ProductServiceApp.UnitTests.Orders;

public class CreateOrderBusinessTests
{
    private static readonly Faker Faker = new("pt_BR");

    private static readonly Faker<CreateOrderRequest> CreateOrderRequestFaker = new Faker<CreateOrderRequest>("pt_BR")
        .RuleFor(order => order.Id, faker => faker.Random.Long(1, 100_000))
        .RuleFor(order => order.IsActive, faker => faker.Random.Bool())
        .RuleFor(order => order.IsDeleted, faker => faker.Random.Bool());

    private static readonly Faker<OrderProductRequest> OrderProductRequestFaker = new Faker<OrderProductRequest>("pt_BR")
        .RuleFor(product => product.ProductId, faker => faker.Random.Long(1, 100_000))
        .RuleFor(product => product.Quantity, faker => faker.Random.Int(1, 5));

    private static readonly Faker<ProductEntity> ProductFaker = new Faker<ProductEntity>("pt_BR")
        .RuleFor(product => product.Id, faker => faker.IndexFaker + 1L)
        .RuleFor(product => product.Name, faker => faker.Commerce.ProductName())
        .RuleFor(product => product.Type, faker => faker.PickRandom<ProductsTypeEnum>())
        .RuleFor(product => product.Price, faker => faker.Finance.Amount(1, 200))
        .RuleFor(product => product.IsActive, true)
        .RuleFor(product => product.IsDeleted, false);

    private static readonly Faker<OrderDiscountRuleEntity> OrderDiscountRuleFaker = new Faker<OrderDiscountRuleEntity>("pt_BR")
        .RuleFor(rule => rule.Id, faker => faker.IndexFaker + 1L)
        .RuleFor(rule => rule.HasSandwich, faker => faker.Random.Bool())
        .RuleFor(rule => rule.HasFries, faker => faker.Random.Bool())
        .RuleFor(rule => rule.HasRefreshment, faker => faker.Random.Bool())
        .RuleFor(rule => rule.DiscountPercentage, faker => faker.Finance.Amount(1, 50))
        .RuleFor(rule => rule.IsActive, true)
        .RuleFor(rule => rule.IsDeleted, false);

    private static readonly Faker<OrderEntity> OrderFaker = new Faker<OrderEntity>("pt_BR")
        .RuleFor(order => order.Id, faker => faker.Random.Long(1, 100_000))
        .RuleFor(order => order.IsActive, faker => faker.Random.Bool())
        .RuleFor(order => order.IsDeleted, faker => faker.Random.Bool());

    private static readonly Faker<OrderAuditEntity> OrderAuditFaker = new Faker<OrderAuditEntity>("pt_BR")
        .RuleFor(audit => audit.Id, faker => faker.Random.Long(1, 100_000))
        .RuleFor(audit => audit.CreatedDate, faker => DateTime.SpecifyKind(faker.Date.Past(), DateTimeKind.Utc))
        .RuleFor(audit => audit.IsActive, true)
        .RuleFor(audit => audit.IsDeleted, false);

    private static readonly Faker<OrderProductEntity> OrderProductFaker = new Faker<OrderProductEntity>("pt_BR")
        .RuleFor(orderProduct => orderProduct.Id, faker => faker.Random.Long(1, 100_000))
        .RuleFor(orderProduct => orderProduct.Quantity, faker => faker.Random.Int(1, 5));

    [Fact]
    public async Task PreProcessAsync_Should_Validate_LoadProducts_LoadRules_CalculateDiscount_And_Return_Intermediate()
    {
        var products = new List<ProductEntity>
        {
            Product(ProductsTypeEnum.Sandwich),
            Product(ProductsTypeEnum.Refreshment)
        };
        var command = Command(products.Select(product => product.Id), isActive: true, isDeleted: false);
        var rules = new List<OrderDiscountRuleEntity>
        {
            Rule(hasSandwich: true, hasFries: false, hasRefreshment: true, discountPercentage: 15m)
        };
        var calculated = DiscountResult();
        var context = TestContext.Create();
        context.Validator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.ProductRepository
            .Setup(item => item.GetByIdsAsync(It.Is<IEnumerable<long>>(ids => ids.ToHashSet().SetEquals(command.Products.Select(product => product.ProductId))), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        context.DiscountRuleRepository
            .Setup(item => item.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);
        context.Calculator
            .Setup(item => item.ExecuteAsync(
                It.Is<OrderDiscountRequest>(request =>
                    request.Products.Count == products.Count &&
                    request.Products.All(item => products.Contains(item.Product)) &&
                    request.Rules.Count == rules.Count &&
                    request.Rules.All(rules.Contains)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(calculated);

        var result = await context.Business.PreProcessForTestAsync(command, CancellationToken.None);

        Assert.Same(command, result.Input);
        Assert.Same(calculated, result.OrderCalculated);
        Assert.Equal(products.Count, result.Products.Count);
        Assert.All(products, product =>
        {
            var resultProduct = Assert.Single(result.Products, item => item.Product.Id == product.Id);
            Assert.Same(product, resultProduct.Product);
            Assert.Equal(command.Products.First(item => item.ProductId == product.Id).Quantity, resultProduct.Quantity);
        });
        Assert.InRange(result.CreatedDate, DateTime.UtcNow.AddSeconds(-10), DateTime.UtcNow.AddSeconds(10));
        context.Validator.Verify(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        context.ProductRepository.Verify(item => item.GetByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()), Times.Once);
        context.DiscountRuleRepository.Verify(item => item.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Calculator.Verify(item => item.ExecuteAsync(It.IsAny<OrderDiscountRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PreProcessAsync_Should_Throw_ValidationException_And_Not_Call_Dependencies_When_Command_Is_Invalid()
    {
        var command = Command([ProductId()], isActive: true, isDeleted: false);
        var context = TestContext.Create();
        context.Validator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure(nameof(CreateOrderCommand.Products), "Produto invalido.")]));

        var act = () => context.Business.PreProcessForTestAsync(command, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Contains("Produto invalido", exception.Message);
        context.ProductRepository.Verify(item => item.GetByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()), Times.Never);
        context.DiscountRuleRepository.Verify(item => item.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Never);
        context.Calculator.Verify(item => item.ExecuteAsync(It.IsAny<OrderDiscountRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        context.Repository.Verify(item => item.CreateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PreProcessAsync_Should_Propagate_Product_Load_Exception_And_Not_Calculate_Discount()
    {
        var product = Product(ProductsTypeEnum.Sandwich);
        var command = Command([product.Id, product.Id + 10_000], isActive: true, isDeleted: false);
        var context = TestContext.Create();
        context.Validator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.ProductRepository
            .Setup(item => item.GetByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([product]);

        var act = () => context.Business.PreProcessForTestAsync(command, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentException>(act);
        Assert.Equal("Um ou mais produtos informados no pedido nao foram encontrados.", exception.Message);
        context.DiscountRuleRepository.Verify(item => item.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Never);
        context.Calculator.Verify(item => item.ExecuteAsync(It.IsAny<OrderDiscountRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void MapToIntermediate_Should_Map_Full_Order_Contract()
    {
        var createdDate = CreatedDate();
        var products = new List<ProductEntity>
        {
            Product(ProductsTypeEnum.Sandwich),
            Product(ProductsTypeEnum.Refreshment)
        };
        var command = Command(products.Select(product => product.Id), isActive: false, isDeleted: true);
        var calculated = DiscountResult();

        var result = CreateOrderBusiness.MapToProcess(new CreateOrderToProcess(
            command,
            products.Select(product => new OrderDiscountProduct(product, command.Products.First(item => item.ProductId == product.Id).Quantity)).ToArray(),
            calculated,
            createdDate));

        Assert.Equal(createdDate, result.CreatedDate);
        Assert.Equal(0L, result.CreatedByUserId);
        Assert.False(result.IsActive);
        Assert.True(result.IsDeleted);
        Assert.Equal(calculated.SubTotalValue, result.SubTotalValue);
        Assert.Equal(calculated.TotalValue, result.TotalValue);
        Assert.Equal(calculated.DiscountPercentage, result.DiscountPercentage);
        Assert.Equal(calculated.DiscountValue, result.DiscountValue);
        Assert.NotNull(result.OrdersAudit);
        Assert.Equal(createdDate, result.OrdersAudit.CreatedDate);
        Assert.Equal(0L, result.OrdersAudit.CreatedByUserId);
        Assert.True(result.OrdersAudit.IsActive);
        Assert.False(result.OrdersAudit.IsDeleted);
        Assert.Collection(
            result.OrderProducts,
            orderProduct =>
            {
                Assert.Equal(products[0].Id, orderProduct.ProductId);
                Assert.Equal(command.Products.First(item => item.ProductId == products[0].Id).Quantity, orderProduct.Quantity);
                Assert.Equal(products[0].Price, orderProduct.UnitPrice);
                Assert.Same(products[0], orderProduct.Product);
            },
            orderProduct =>
            {
                Assert.Equal(products[1].Id, orderProduct.ProductId);
                Assert.Equal(command.Products.First(item => item.ProductId == products[1].Id).Quantity, orderProduct.Quantity);
                Assert.Equal(products[1].Price, orderProduct.UnitPrice);
                Assert.Same(products[1], orderProduct.Product);
            });
    }

    [Fact]
    public async Task ProcessAsync_Should_Call_CreateAsync_And_Return_Created_Order()
    {
        var context = TestContext.Create();
        var product = Product(ProductsTypeEnum.Sandwich);
        var command = Command([product.Id], isActive: true, isDeleted: false);
        var calculated = DiscountResult(discountPercentage: 0m, discountValue: 0m);
        var input = new CreateOrderToProcess(
            command,
            [new OrderDiscountProduct(product, command.Products.First().Quantity)],
            calculated,
            CreatedDate());
        var created = OrderEntity();
        context.Repository
            .Setup(item => item.CreateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await context.Business.ProcessForTestAsync(input, CancellationToken.None);

        Assert.Same(created, result.CreatedOrder);
        context.Repository.Verify(item => item.CreateAsync(
            It.Is<OrderEntity>(entity =>
                entity.SubTotalValue == calculated.SubTotalValue &&
                entity.TotalValue == calculated.TotalValue &&
                entity.OrderProducts.Count == 1 &&
                entity.OrderProducts.First().ProductId == product.Id &&
                entity.OrderProducts.First().Quantity == command.Products.First().Quantity),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PostProcessAsync_Should_Return_OrderResponse_From_OrderEntity()
    {
        var context = TestContext.Create();
        var product = Product(ProductsTypeEnum.Sandwich);
        var order = OrderEntity();
        order.OrdersAudit = OrderAuditFaker.Generate();
        order.OrderProducts = [OrderProduct(product)];

        var result = await context.Business.PostProcessForTestAsync(new CreateOrderToPostProcess(order), CancellationToken.None);

        var response = Assert.IsType<OrderResponse>(result);
        Assert.Equal(order.Id, response.Id);
        Assert.Equal(order.IsActive, response.IsActive);
        Assert.Equal(order.IsDeleted, response.IsDeleted);
        Assert.Equal(order.SubTotalValue, response.SubTotalValue);
        Assert.Equal(order.DiscountPercentage, response.DiscountPercentage);
        Assert.Equal(order.DiscountValue, response.DiscountValue);
        Assert.Equal(order.TotalValue, response.TotalValue);
        Assert.Equal(order.OrdersAudit.CreatedDate, response.CreatedDate);
        var responseProduct = Assert.Single(response.Products);
        Assert.Equal(product.Id, responseProduct.Id);
        Assert.Equal(product.Name, responseProduct.Name);
        Assert.Equal(product.Type, responseProduct.Type);
        Assert.Equal(order.OrderProducts.First().Quantity, responseProduct.Quantity);
        Assert.Equal(order.OrderProducts.First().UnitPrice, responseProduct.Price);
        context.Cache.Verify(item => item.InvalidateAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Cache.Verify(item => item.SetByIdAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateOrderCommand Command(IEnumerable<long> productIds, bool? isActive, bool? isDeleted)
    {
        var request = CreateOrderRequestFaker.Generate();
        request.Products =
        [
            .. productIds.Select(productId =>
            {
                var orderProduct = OrderProductRequestFaker.Generate();
                orderProduct.ProductId = productId;
                return orderProduct;
            })
        ];
        request.IsActive = isActive;
        request.IsDeleted = isDeleted;

        return new CreateOrderCommand(request);
    }

    private static ProductEntity Product(ProductsTypeEnum type)
    {
        var product = ProductFaker.Generate();
        product.Type = type;

        return product;
    }

    private static OrderDiscountRuleEntity Rule(
        bool hasSandwich,
        bool hasFries,
        bool hasRefreshment,
        decimal discountPercentage)
    {
        var rule = OrderDiscountRuleFaker.Generate();
        rule.HasSandwich = hasSandwich;
        rule.HasFries = hasFries;
        rule.HasRefreshment = hasRefreshment;
        rule.DiscountPercentage = discountPercentage;

        return rule;
    }

    private static OrderDiscountResult DiscountResult(
        decimal? discountPercentage = null,
        decimal? discountValue = null)
    {
        var subTotalValue = Faker.Finance.Amount(10, 500);
        var percentage = discountPercentage ?? Faker.Finance.Amount(1, 50);
        var value = discountValue ?? decimal.Round(subTotalValue * percentage / 100m, 2);

        return new OrderDiscountResult
        {
            SubTotalValue = subTotalValue,
            DiscountPercentage = percentage,
            DiscountValue = value,
            TotalValue = subTotalValue - value
        };
    }

    private static OrderEntity OrderEntity()
    {
        var calculated = DiscountResult();
        var order = OrderFaker.Generate();
        order.SubTotalValue = calculated.SubTotalValue;
        order.DiscountPercentage = calculated.DiscountPercentage;
        order.DiscountValue = calculated.DiscountValue;
        order.TotalValue = calculated.TotalValue;

        return order;
    }

    private static OrderProductEntity OrderProduct(ProductEntity product)
    {
        var orderProduct = OrderProductFaker.Generate();
        orderProduct.ProductId = product.Id;
        orderProduct.UnitPrice = product.Price ?? decimal.Zero;
        orderProduct.Product = product;

        return orderProduct;
    }

    private static DateTime CreatedDate()
    {
        return DateTime.SpecifyKind(Faker.Date.Past(), DateTimeKind.Utc);
    }

    private static long ProductId()
    {
        return Faker.Random.Long(1, 100_000);
    }

    private sealed class TestContext
    {
        public Mock<IOrderCommandRepository> Repository { get; } = new();
        public Mock<IProductQueryRepository<ProductEntity>> ProductRepository { get; } = new();
        public Mock<IOrderCacheService> Cache { get; } = new();
        public Mock<IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity>> DiscountRuleRepository { get; } = new();
        public Mock<IOrderDiscountCalculator> Calculator { get; } = new();
        public Mock<IValidator<CreateOrderCommand>> Validator { get; } = new();
        public TestableCreateOrderBusiness Business { get; private set; } = null!;

        public static TestContext Create()
        {
            var context = new TestContext();
            var loadProductsAsync = new LoadProductsAsync(context.ProductRepository.Object);
            context.Business = new TestableCreateOrderBusiness(
                loadProductsAsync,
                context.Repository.Object,
                context.Cache.Object,
                context.DiscountRuleRepository.Object,
                context.Calculator.Object,
                context.Validator.Object);

            return context;
        }
    }

    private sealed class TestableCreateOrderBusiness(
            LoadProductsAsync loadProductsAsync,
            IOrderCommandRepository repository,
            IOrderCacheService cache,
            IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity> discountRuleRepository,
            IOrderDiscountCalculator calculator,
            IValidator<CreateOrderCommand> validator)
        : CreateOrderBusiness(loadProductsAsync, repository, cache, discountRuleRepository, calculator, validator)
    {
        public Task<CreateOrderToProcess> PreProcessForTestAsync(CreateOrderCommand input, CancellationToken ct)
        {
            return PreProcessAsync(input, ct);
        }

        public Task<CreateOrderToPostProcess> ProcessForTestAsync(CreateOrderToProcess input, CancellationToken ct)
        {
            return ProcessAsync(input, ct);
        }

        public Task<OrderResponse> PostProcessForTestAsync(CreateOrderToPostProcess input, CancellationToken ct)
        {
            return PostProcessAsync(input, ct);
        }
    }
}
