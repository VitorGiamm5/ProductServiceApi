using Bogus;
using FluentAssertions;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.UnitTests.Domain;

public class ProductMappingTests
{
    private static readonly Faker<CreateProductRequest> CreateProductRequestFaker = new Faker<CreateProductRequest>("pt_BR")
        .RuleFor(product => product.Name, faker => faker.Commerce.ProductName())
        .RuleFor(product => product.Price, faker => faker.Finance.Amount(1, 200))
        .RuleFor(product => product.Type, faker => faker.PickRandom<ProductsTypeEnum>())
        .RuleFor(product => product.IsActive, true)
        .RuleFor(product => product.IsDeleted, false);

    [Fact]
    public void CreateProductCommand_Should_Map_Request_To_Entity()
    {
        var request = CreateProductRequestFaker.Generate();
        var command = new CreateProductCommand(request);

        var entity = command.MapTo();

        entity.Name.Should().Be(request.Name);
        entity.Price.Should().Be(request.Price);
        entity.Type.Should().Be(request.Type);
        entity.IsActive.Should().Be(request.IsActive);
        entity.IsDeleted.Should().BeNull();
    }
}
