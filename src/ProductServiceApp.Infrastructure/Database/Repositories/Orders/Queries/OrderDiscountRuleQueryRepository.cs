using Microsoft.EntityFrameworkCore;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Orders.Queries;

public class OrderDiscountRuleQueryRepository(ReadOnlyDbContext context) : BaseQueryRepository<OrderDiscountRuleEntity>(context), IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity>
{
    public async Task<IEnumerable<OrderDiscountRuleEntity>> GetActiveAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<OrderDiscountRuleEntity>()
            .Where(rule => rule.IsActive != false)
            .ToListAsync(cancellationToken);
    }
}
