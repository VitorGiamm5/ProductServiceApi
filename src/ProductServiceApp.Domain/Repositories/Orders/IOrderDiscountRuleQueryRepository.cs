using ProductServiceApp.Domain.Repositories.Base;

namespace ProductServiceApp.Domain.Repositories.Orders;

public interface IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity> : IBaseQueryRepository<OrderDiscountRuleEntity> where OrderDiscountRuleEntity : class
{
    #region Additional Methods

    Task<IEnumerable<OrderDiscountRuleEntity>> GetActiveAsync(CancellationToken cancellationToken);
    #endregion
}
