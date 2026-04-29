using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Infrastructure.Database.Configuration.Base;

namespace ProductServiceApp.Infrastructure.Database.Configuration.Orders;

public class OrderAuditConfiguration : BaseAuditConfiguration<OrderAuditEntity>
{
    public override void Configure(EntityTypeBuilder<OrderAuditEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("tb_orders_audit");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn(seed: 100000, increment: 1);
    }
}
