using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductServiceApp.Domain.Entities.Orders;

namespace ProductServiceApp.Infrastructure.Database.Configuration.Orders;

public class OrderProductConfiguration : IEntityTypeConfiguration<OrderProductEntity>
{
    public void Configure(EntityTypeBuilder<OrderProductEntity> builder)
    {
        builder.ToTable("tb_order_product");

        builder.HasKey(e => new { e.Id, e.ProductId });

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.UnitPrice)
            .HasColumnName("unit_price")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.HasOne(e => e.Order)
            .WithMany(e => e.OrderProducts)
            .HasForeignKey(e => e.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => e.Order == null || e.Order.IsDeleted != true);
    }
}
