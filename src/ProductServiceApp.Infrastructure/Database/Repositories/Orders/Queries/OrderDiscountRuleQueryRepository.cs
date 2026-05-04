using Microsoft.EntityFrameworkCore;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Orders.Queries;

public class OrderDiscountRuleQueryRepository : BaseQueryRepository<OrderDiscountRuleEntity>, IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity>
{
    public OrderDiscountRuleQueryRepository(ReadOnlyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrderDiscountRuleEntity>> GetActiveAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<OrderDiscountRuleEntity>()
            .Where(rule => rule.IsActive != false)
            .ToListAsync(cancellationToken);
    }
}
