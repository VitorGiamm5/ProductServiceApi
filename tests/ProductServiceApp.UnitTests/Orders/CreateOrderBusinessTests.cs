using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ProductServiceApp.Application.Business.Orders.Create;
using ProductServiceApp.Application.Business.Products.GetByIdList;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Domain.Business.Orders.AdditionalFeaturesBusiness.OrderDiscount;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.UnitTests.Orders;

public class CreateOrderBusinessTests
{
    [Fact]
    public async Task PreProcessAsync_Should_Validate_LoadProducts_LoadRules_CalculateDiscount_And_Return_Intermediate()
    {
        var command = Command([1, 2], isActive: true, isDeleted: false);
        var products = new List<ProductEntity>
        {
            Product(1, ProductsTypeEnum.Sandwich, 20m),
            Product(2, ProductsTypeEnum.Refreshment, 10m)
        };
        var rules = new List<OrderDiscountRuleEntity>
        {
            Rule(hasSandwich: true, hasFries: false, hasRefreshment: true, discountPercentage: 15m)
        };
        var calculated = new OrderDiscountResult
        {
            SubTotalValue = 30m,
            DiscountPercentage = 15m,
            DiscountValue = 4.50m,
            TotalValue = 25.50m
        };
        var context = TestContext.Create();
        context.Validator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.ProductRepository
            .Setup(item => item.GetByIdsAsync(It.Is<IEnumerable<long>>(ids => ids.ToHashSet().SetEquals(command.ProductIds)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        context.DiscountRuleRepository
            .Setup(item => item.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);
        context.Calculator
            .Setup(item => item.ExecuteAsync(
                It.Is<OrderDiscountRequest>(request =>
                    request.Products.Count == products.Count &&
                    request.Products.All(products.Contains) &&
                    request.Rules.Count == rules.Count &&
                    request.Rules.All(rules.Contains)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(calculated);

        var result = await context.Business.PreProcessForTestAsync(command, CancellationToken.None);

        result.Should().BeEquivalentTo(new
        {
            Input = command,
            Products = products,
            OrderCalculated = calculated
        });
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        context.Validator.Verify(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        context.ProductRepository.Verify(item => item.GetByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()), Times.Once);
        context.DiscountRuleRepository.Verify(item => item.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Calculator.Verify(item => item.ExecuteAsync(It.IsAny<OrderDiscountRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PreProcessAsync_Should_Throw_ValidationException_And_Not_Call_Dependencies_When_Command_Is_Invalid()
    {
        var command = Command([1], isActive: true, isDeleted: false);
        var context = TestContext.Create();
        context.Validator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure(nameof(CreateOrderCommand.ProductIds), "Produto invalido.")]));

        var act = () => context.Business.PreProcessForTestAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Produto invalido*");
        context.ProductRepository.Verify(item => item.GetByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()), Times.Never);
        context.DiscountRuleRepository.Verify(item => item.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Never);
        context.Calculator.Verify(item => item.ExecuteAsync(It.IsAny<OrderDiscountRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        context.Repository.Verify(item => item.CreateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PreProcessAsync_Should_Propagate_Product_Load_Exception_And_Not_Calculate_Discount()
    {
        var command = Command([1, 99], isActive: true, isDeleted: false);
        var context = TestContext.Create();
        context.Validator
            .Setup(item => item.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        context.ProductRepository
            .Setup(item => item.GetByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Product(1, ProductsTypeEnum.Sandwich, 20m)]);

        var act = () => context.Business.PreProcessForTestAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Um ou mais produtos informados no pedido nao foram encontrados.");
        context.DiscountRuleRepository.Verify(item => item.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Never);
        context.Calculator.Verify(item => item.ExecuteAsync(It.IsAny<OrderDiscountRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void MapToIntermediate_Should_Map_Full_Order_Contract()
    {
        var createdDate = new DateTime(2026, 05, 04, 12, 00, 00, DateTimeKind.Utc);
        var command = Command([1, 2], isActive: false, isDeleted: true);
        var products = new List<ProductEntity>
        {
            Product(1, ProductsTypeEnum.Sandwich, 20m),
            Product(2, ProductsTypeEnum.Refreshment, 10m)
        };
        var calculated = new OrderDiscountResult
        {
            SubTotalValue = 30m,
            DiscountPercentage = 15m,
            DiscountValue = 4.50m,
            TotalValue = 25.50m
        };

        var result = CreateOrderBusiness.MapToProcess(new CreateOrderToProcess(
            command,
            products,
            calculated,
            createdDate));

        result.Should().BeEquivalentTo(new
        {
            CreatedDate = createdDate,
            CreatedByUserId = 0L,
            IsActive = false,
            IsDeleted = true,
            SubTotalValue = 30m,
            TotalValue = 25.50m,
            DiscountPercentage = 15m,
            DiscountValue = 4.50m
        });
        result.OrdersAudit.Should().BeEquivalentTo(new
        {
            CreatedDate = createdDate,
            CreatedByUserId = 0L,
            IsActive = true,
            IsDeleted = false
        });
        result.OrderProducts.Should().HaveCount(2);
        result.OrderProducts.ElementAt(0).Should().BeEquivalentTo(new
        {
            ProductId = 1L,
            UnitPrice = 20m,
            Product = products[0]
        });
        result.OrderProducts.ElementAt(1).Should().BeEquivalentTo(new
        {
            ProductId = 2L,
            UnitPrice = 10m,
            Product = products[1]
        });
    }

    [Fact]
    public async Task ProcessAsync_Should_Call_CreateAsync_And_Return_Created_Order()
    {
        var context = TestContext.Create();
        var input = new CreateOrderToProcess(
            Command([1], isActive: true, isDeleted: false),
            [Product(1, ProductsTypeEnum.Sandwich, 20m)],
            new OrderDiscountResult
            {
                SubTotalValue = 20m,
                TotalValue = 20m,
                DiscountPercentage = 0m,
                DiscountValue = 0m
            },
            new DateTime(2026, 05, 04, 12, 00, 00, DateTimeKind.Utc));
        var created = new OrderEntity { Id = 123, SubTotalValue = 30m, TotalValue = 25.50m };
        context.Repository
            .Setup(item => item.CreateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await context.Business.ProcessForTestAsync(input, CancellationToken.None);

        result.CreatedOrder.Should().BeSameAs(created);
        context.Repository.Verify(item => item.CreateAsync(
            It.Is<OrderEntity>(entity =>
                entity.SubTotalValue == 20m &&
                entity.TotalValue == 20m &&
                entity.OrderProducts.Count == 1 &&
                entity.OrderProducts.First().ProductId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PostProcessAsync_Should_Return_OrderResponse_From_OrderEntity()
    {
        var context = TestContext.Create();
        var order = new OrderEntity
        {
            Id = 123,
            IsActive = true,
            IsDeleted = false,
            SubTotalValue = 30m,
            DiscountPercentage = 15m,
            DiscountValue = 4.50m,
            TotalValue = 25.50m,
            OrdersAudit = new OrderAuditEntity { CreatedDate = new DateTime(2026, 05, 04, 12, 00, 00, DateTimeKind.Utc) },
            OrderProducts =
            [
                new OrderProductEntity
                {
                    ProductId = 1,
                    UnitPrice = 20m,
                    Product = Product(1, ProductsTypeEnum.Sandwich, 20m)
                }
            ]
        };

        var result = await context.Business.PostProcessForTestAsync(new CreateOrderToPostProcess(order), CancellationToken.None);

        result.Should().BeOfType<OrderResponse>();
        result.Should().BeEquivalentTo(new
        {
            Id = 123L,
            ProductIds = new List<long> { 1 },
            IsActive = true,
            IsDeleted = false,
            SubTotalValue = 30m,
            DiscountPercentage = 15m,
            DiscountValue = 4.50m,
            TotalValue = 25.50m,
            CreatedDate = order.OrdersAudit.CreatedDate
        });
    }

    private static CreateOrderCommand Command(List<long> productIds, bool? isActive, bool? isDeleted)
    {
        return new CreateOrderCommand(new CreateOrderRequest
        {
            ProductIds = productIds,
            IsActive = isActive,
            IsDeleted = isDeleted
        });
    }

    private static ProductEntity Product(long id, ProductsTypeEnum type, decimal? price)
    {
        return new ProductEntity
        {
            Id = id,
            Name = type.ToString(),
            Type = type,
            Price = price
        };
    }

    private static OrderDiscountRuleEntity Rule(
        bool hasSandwich,
        bool hasFries,
        bool hasRefreshment,
        decimal discountPercentage)
    {
        return new OrderDiscountRuleEntity
        {
            HasSandwich = hasSandwich,
            HasFries = hasFries,
            HasRefreshment = hasRefreshment,
            DiscountPercentage = discountPercentage,
            IsActive = true,
            IsDeleted = false
        };
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
