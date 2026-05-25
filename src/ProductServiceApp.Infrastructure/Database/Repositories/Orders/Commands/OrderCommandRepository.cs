using Microsoft.EntityFrameworkCore;
using ProductServiceApp.Domain.DateTimes;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Exceptions;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Repositories.Base;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Orders.Commands;

public class OrderCommandRepository(ApplicationDbContext context) : BaseCommandRepository<OrderEntity>(context), IOrderCommandRepository
{
    public new async Task<OrderEntity> CreateAsync(OrderEntity entity, CancellationToken cancellationToken)
    {
        foreach (var item in entity.OrderProducts)
        {
            item.Product = null;
        }

        await _context.Set<OrderEntity>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await LoadOrderAsync(entity.Id, cancellationToken);
    }

    public new async Task<OrderEntity> UpdateAsync(OrderEntity entity, long id, CancellationToken cancellationToken)
    {
        var existing = await _context.Set<OrderEntity>()
            .AsTracking()
            .Include(order => order.OrdersAudit)
            .Include(order => order.OrderProducts)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrderEntity), id);

        var desiredItems = entity.OrderProducts.ToDictionary(item => item.ProductId);

        if (!HasBusinessChanges(existing, entity, desiredItems))
        {
            return await LoadOrderAsync(existing.Id, cancellationToken);
        }

        existing.UpdatedDate = entity.UpdatedDate;
        existing.UpdatedByUserId = entity.UpdatedByUserId;
        existing.IsActive = entity.IsActive;
        existing.IsDeleted = entity.IsDeleted;
        existing.SubTotalValue = entity.SubTotalValue;
        existing.TotalValue = entity.TotalValue;
        existing.DiscountPercentage = entity.DiscountPercentage;
        existing.DiscountValue = entity.DiscountValue;

        if (existing.OrdersAudit is not null)
        {
            existing.OrdersAudit.UpdatedDate = entity.UpdatedDate;
            existing.OrdersAudit.UpdatedByUserId = entity.UpdatedByUserId;
        }

        var removedItems = existing.OrderProducts
            .Where(item => !desiredItems.ContainsKey(item.ProductId))
            .ToList();

        _context.Set<OrderProductEntity>().RemoveRange(removedItems);

        foreach (var currentItem in existing.OrderProducts.Where(item => desiredItems.ContainsKey(item.ProductId)))
        {
            currentItem.UnitPrice = desiredItems[currentItem.ProductId].UnitPrice;
            currentItem.Quantity = desiredItems[currentItem.ProductId].Quantity;
        }

        var currentProductIds = existing.OrderProducts
            .Select(item => item.ProductId)
            .ToHashSet();

        foreach (var newItem in desiredItems.Values.Where(item => !currentProductIds.Contains(item.ProductId)))
        {
            existing.OrderProducts.Add(new OrderProductEntity
            {
                Id = existing.Id,
                ProductId = newItem.ProductId,
                Quantity = newItem.Quantity,
                UnitPrice = newItem.UnitPrice
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await LoadOrderAsync(existing.Id, cancellationToken);
    }

    public async Task<OrderEntity> SoftDeleteAsync(long id, CancellationToken cancellationToken)
    {
        var existing = await _context.Set<OrderEntity>()
            .IgnoreQueryFilters()
            .AsTracking()
            .Include(order => order.OrdersAudit)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrderEntity), id);

        if (existing.IsDeleted == true)
        {
            return existing;
        }

        var now = DateTimeProvider.UtcNowAsUnspecified();

        existing.IsDeleted = true;
        existing.IsActive = false;
        existing.DeletedDate = now;
        existing.DeletedByUserId = 0;

        if (existing.OrdersAudit is not null)
        {
            existing.OrdersAudit.IsDeleted = true;
            existing.OrdersAudit.IsActive = false;
            existing.OrdersAudit.DeletedDate = now;
            existing.OrdersAudit.DeletedByUserId = 0;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return existing;
    }

    private static bool HasBusinessChanges(
        OrderEntity existing,
        OrderEntity desired,
        IReadOnlyDictionary<long, OrderProductEntity> desiredItems)
    {
        if (existing.IsActive != desired.IsActive ||
            existing.IsDeleted != desired.IsDeleted ||
            existing.SubTotalValue != desired.SubTotalValue ||
            existing.TotalValue != desired.TotalValue ||
            existing.DiscountPercentage != desired.DiscountPercentage ||
            existing.DiscountValue != desired.DiscountValue)
        {
            return true;
        }

        if (existing.OrderProducts.Count != desiredItems.Count)
        {
            return true;
        }

        foreach (var currentItem in existing.OrderProducts)
        {
            if (!desiredItems.TryGetValue(currentItem.ProductId, out var desiredItem))
            {
                return true;
            }

            if (currentItem.Quantity != desiredItem.Quantity ||
                currentItem.UnitPrice != desiredItem.UnitPrice)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<OrderEntity> LoadOrderAsync(long id, CancellationToken cancellationToken)
    {
        return await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(order => order.OrdersAudit)
            .Include(order => order.OrderProducts)
            .ThenInclude(item => item.Product)
            .FirstAsync(order => order.Id == id, cancellationToken);
    }
}
