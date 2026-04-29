using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using FluentAssertions;

namespace ProductServiceApp.UnitTests.Architecture;

public class LayerDependencyTests
{
    private const string ApiAssembly = "ProductServiceApp.Api";
    private const string ApplicationAssembly = "ProductServiceApp.Application";
    private const string DomainAssembly = "ProductServiceApp.Domain";
    private const string InfrastructureAssembly = "ProductServiceApp.Infrastructure";

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
        var violations = GetTypeReferenceNamespaces(DomainAssembly, forbiddenDependency);

        violations.Should().BeEmpty(
            "Domain must stay pure. Violations: {0}",
            string.Join(", ", violations));
    }

    [Theory]
    [InlineData("ProductServiceApp.Api")]
    [InlineData("ProductServiceApp.Infrastructure")]
    public void Application_Types_Should_Not_Depend_On_Outer_Layers(string forbiddenDependency)
    {
        var violations = GetTypeReferenceNamespaces(ApplicationAssembly, forbiddenDependency);

        violations.Should().BeEmpty(
            "Application should not depend on outer layers. Violations: {0}",
            string.Join(", ", violations));
    }

    private static string[] GetProductServiceAppReferences(string assemblyName)
    {
        using var reader = OpenMetadataReader(assemblyName);
        var metadata = reader.GetMetadataReader();

        return metadata
            .AssemblyReferences
            .Select(metadata.GetAssemblyReference)
            .Select(reference => metadata.GetString(reference.Name))
            .Where(name => name is not null && name.StartsWith("ProductServiceApp.", StringComparison.Ordinal))
            .Order()
            .ToArray();
    }

    private static string[] GetTypeReferenceNamespaces(string assemblyName, string forbiddenDependency)
    {
        using var reader = OpenMetadataReader(assemblyName);
        var metadata = reader.GetMetadataReader();

        return metadata
            .TypeReferences
            .Select(metadata.GetTypeReference)
            .Select(reference => metadata.GetString(reference.Namespace))
            .Where(ns => ns.Equals(forbiddenDependency, StringComparison.Ordinal)
                || ns.StartsWith(forbiddenDependency + ".", StringComparison.Ordinal))
            .Distinct()
            .Order()
            .ToArray();
    }

    private static PEReader OpenMetadataReader(string assemblyName)
    {
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyName + ".dll");
        return new PEReader(File.OpenRead(assemblyPath));
    }
}
