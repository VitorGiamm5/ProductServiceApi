using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Infrastructure.Database.Configuration.Base;

namespace ProductServiceApp.Infrastructure.Database.Configuration.Products;

public class ProductConfiguration : BaseAuditConfiguration<ProductEntity>
{
    public override void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("tb_product");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
               .HasColumnName("name")
               .HasColumnType("varchar")
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(e => e.Price)
               .HasColumnName("price")
               .HasColumnType("numeric(10,2)"); // Define precision and scale for monetary values

        builder.Property(e => e.Type)
               .HasColumnName("type")
               .HasConversion<byte>();  // Stores as byte in the database

        builder.Property(e => e.Id)
               .UseIdentityColumn(seed: 100000, increment: 1);
    }
}
