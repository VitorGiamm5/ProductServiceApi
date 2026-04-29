using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using ProductServiceApp.Application;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Infrastructure;

namespace ProductServiceApp.UnitTests.Architecture;

public class LayerDependencyTests
{
    private static readonly Assembly ApiAssembly = typeof(global::Program).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(SetupApplication).Assembly;
    private static readonly Assembly DomainAssembly = typeof(ProductEntity).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(SetupInfrastructure).Assembly;

    [Fact]
    public void Domain_Should_Not_Reference_Other_ProductServiceApp_Projects()
    {
        GetProductServiceAppReferences(DomainAssembly).Should().BeEmpty();
    }

    [Fact]
    public void Application_Should_Reference_Domain_Only_From_ProductServiceApp_Projects()
    {
        GetProductServiceAppReferences(ApplicationAssembly)
            .Should()
            .BeEquivalentTo("ProductServiceApp.Domain");
    }

    [Fact]
    public void Infrastructure_Should_Not_Reference_Api_Or_Application()
    {
        GetProductServiceAppReferences(InfrastructureAssembly)
            .Should()
            .NotContain(["ProductServiceApp.Api", "ProductServiceApp.Application"]);
    }

    [Fact]
    public void Api_Should_Be_Composition_Root_And_Reference_Application_Domain_And_Infrastructure()
    {
        GetProductServiceAppReferences(ApiAssembly)
            .Should()
            .BeEquivalentTo(
                "ProductServiceApp.Application",
                "ProductServiceApp.Domain",
                "ProductServiceApp.Infrastructure");
    }

    [Theory]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("ProductServiceApp.Api")]
    [InlineData("ProductServiceApp.Application")]
    [InlineData("ProductServiceApp.Infrastructure")]
    public void Domain_Types_Should_Not_Depend_On_Frameworks_Or_Outer_Layers(string forbiddenDependency)
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(forbiddenDependency)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain must stay pure. Violations: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Theory]
    [InlineData("ProductServiceApp.Api")]
    [InlineData("ProductServiceApp.Infrastructure")]
    public void Application_Types_Should_Not_Depend_On_Outer_Layers(string forbiddenDependency)
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(forbiddenDependency)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application should not depend on outer layers. Violations: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    private static string[] GetProductServiceAppReferences(Assembly assembly)
    {
        return assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null && name.StartsWith("ProductServiceApp.", StringComparison.Ordinal))
            .Select(name => name!)
            .Order()
            .ToArray();
    }
}
