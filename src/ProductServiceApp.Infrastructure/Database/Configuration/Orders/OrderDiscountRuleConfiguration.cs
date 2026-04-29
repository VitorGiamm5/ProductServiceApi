using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Infrastructure.Database.Configuration.Base;

namespace ProductServiceApp.Infrastructure.Database.Configuration.Orders;

public class OrderDiscountRuleConfiguration : BaseAuditConfiguration<OrderDiscountRuleEntity>
{
    public override void Configure(EntityTypeBuilder<OrderDiscountRuleEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("tb_order_discount_rule");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn(seed: 100000, increment: 1);

        builder.Property(e => e.HasSandwich)
            .HasColumnName("has_sandwich")
            .IsRequired();

        builder.Property(e => e.HasFries)
            .HasColumnName("has_fries")
            .IsRequired();

        builder.Property(e => e.HasRefreshment)
            .HasColumnName("has_refreshment")
            .IsRequired();

        builder.Property(e => e.DiscountPercentage)
            .HasColumnName("discount_percentage")
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.HasIndex(e => new { e.HasSandwich, e.HasFries, e.HasRefreshment })
            .IsUnique()
            .HasFilter("is_deleted <> true");

        builder.HasData(
            new OrderDiscountRuleEntity
            {
                Id = 100000,
                HasSandwich = true,
                HasFries = true,
                HasRefreshment = true,
                DiscountPercentage = 20m,
                IsActive = true,
                IsDeleted = false
            },
            new OrderDiscountRuleEntity
            {
                Id = 100001,
                HasSandwich = true,
                HasFries = false,
                HasRefreshment = true,
                DiscountPercentage = 15m,
                IsActive = true,
                IsDeleted = false
            },
            new OrderDiscountRuleEntity
            {
                Id = 100002,
                HasSandwich = true,
                HasFries = true,
                HasRefreshment = false,
                DiscountPercentage = 10m,
                IsActive = true,
                IsDeleted = false
            });
    }
}
