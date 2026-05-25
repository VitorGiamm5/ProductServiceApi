using FluentValidation;

namespace ProductServiceApp.Application.Extensions.AssertionsValidator.Products;

public static class LongIdsValidatorExtensions
{
    public static IRuleBuilderOptions<T, List<long>> NoDuplicateIds<T>(
        this IRuleBuilder<T, List<long>> ruleBuilder)
    {
        return ruleBuilder
            .Must(ids =>
            {
                var list = ids?.ToList();
                return list == null || list.Count == list.Distinct().Count();
            })
            .WithMessage("The list contains duplicate IDs.");
    }

    public static IRuleBuilderOptions<T, List<long>> NoZeroIds<T>(
        this IRuleBuilder<T, List<long>> ruleBuilder)
    {
        return ruleBuilder
            .Must(ids => ids == null || !ids.Any(id => id == 0))
            .WithMessage("The list contains one or more IDs with value zero.");
    }
}
