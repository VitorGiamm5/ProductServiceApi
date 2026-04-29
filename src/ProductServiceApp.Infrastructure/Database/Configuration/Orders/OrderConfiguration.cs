using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Infrastructure.Database.Configuration.Base;

namespace ProductServiceApp.Infrastructure.Database.Configuration.Orders;

public class OrderConfiguration : BaseAuditConfiguration<OrderEntity>
{
    public override void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("tb_order");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn(seed: 100000, increment: 1);

        builder.Property(e => e.IdOrdersAudit)
            .HasColumnName("id_orders_audit");

        builder.Property(e => e.SubTotalValue)
            .HasColumnName("subtotal_value")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(e => e.TotalValue)
            .HasColumnName("total_value")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(e => e.DiscountPercentage)
            .HasColumnName("discount_percentage")
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(e => e.DiscountValue)
            .HasColumnName("discount_value")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.HasOne(e => e.OrdersAudit)
            .WithMany()
            .HasForeignKey(e => e.IdOrdersAudit)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
