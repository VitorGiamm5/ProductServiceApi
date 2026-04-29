using Microsoft.EntityFrameworkCore;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Infrastructure.Database.Contexts;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Orders.Queries;

public class OrderDiscountRuleQueryRepository(ReadOnlyDbContext context) : IOrderDiscountRuleQueryRepository
{
    public async Task<IEnumerable<OrderDiscountRuleEntity>> GetActiveAsync(CancellationToken cancellationToken)
    {
        return await context.Set<OrderDiscountRuleEntity>()
            .Where(rule => rule.IsActive != false)
            .ToListAsync(cancellationToken);
    }
}
