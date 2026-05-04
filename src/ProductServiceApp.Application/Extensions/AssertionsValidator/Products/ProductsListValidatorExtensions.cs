using FluentValidation;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Exceptions;

namespace ProductServiceApp.Application.Extensions.AssertionsValidator.Products;

public static class ProductsListValidatorExtensions
{
    /// <summary>
    /// Valida se a lista de produtos não contém produtos duplicados do mesmo tipo, exceto do tipo Default.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, IReadOnlyCollection<ProductEntity>> NoDuplicateProductsType<T>(
        this IRuleBuilder<T, IReadOnlyCollection<ProductEntity>> ruleBuilder)
    {
        return ruleBuilder.NoDuplicateProductsType(products => products?.Select(p => p.Type));
    }

    private static IRuleBuilderOptions<T, TProducts> NoDuplicateProductsType<T, TProducts>(
        this IRuleBuilder<T, TProducts> ruleBuilder,
        Func<TProducts?, IEnumerable<ProductsTypeEnum?>?> getProductTypes)
    {
        return ruleBuilder
            .Must(products => !GetDuplicatedTypes(products, getProductTypes).Any())
            .WithMessage((_, products) =>
            {
                var duplicatedTypes = GetDuplicatedTypes(products, getProductTypes)
                    .Select(type => type.GetDescription());

                return $"O pedido não pode conter produtos duplicados dos tipos: {duplicatedTypes.JoinWithLastSeparator()}.";
            });
    }

    private static IEnumerable<ProductsTypeEnum> GetDuplicatedTypes<TProducts>(
        TProducts? products,
        Func<TProducts?, IEnumerable<ProductsTypeEnum?>?> getProductTypes)
    {
        return getProductTypes(products)?
            .Where(type => type is not null and not ProductsTypeEnum.Default)
            .GroupBy(type => type!.Value)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key) ?? [];
    }
}
