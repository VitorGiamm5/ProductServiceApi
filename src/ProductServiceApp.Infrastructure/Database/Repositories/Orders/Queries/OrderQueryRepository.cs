using Microsoft.EntityFrameworkCore;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Exceptions;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Orders.Queries;

public class OrderQueryRepository : BaseQueryRepository<OrderEntity>, IOrderQueryRepository
{
    public OrderQueryRepository(ReadOnlyDbContext context) : base(context)
    {
    }

    public new async Task<IEnumerable<OrderEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<OrderEntity>()
            .Include(order => order.OrdersAudit)
            .Include(order => order.OrderProducts)
            .ThenInclude(item => item.Product)
            .OrderByDescending(order => order.Id)
            .ToListAsync(cancellationToken);
    }

    public new async Task<OrderEntity> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        if (id <= 0) throw new ArgumentException("Id invalido.", nameof(id));

        return await _context.Set<OrderEntity>()
            .Include(order => order.OrdersAudit)
            .Include(order => order.OrderProducts)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrderEntity), id);
    }
}
