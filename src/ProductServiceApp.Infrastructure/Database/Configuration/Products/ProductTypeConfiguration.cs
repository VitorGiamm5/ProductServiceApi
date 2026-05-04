using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Exceptions;

namespace ProductServiceApp.Infrastructure.Database.Configuration.Products;

public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductTypeEntity>
{
    public void Configure(EntityTypeBuilder<ProductTypeEntity> builder)
    {
        builder.ToTable("tb_product_type");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasConversion<byte>()
            .ValueGeneratedNever();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnType("varchar")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasData(
            Enum.GetValues<ProductsTypeEnum>()
                .Select(type => new ProductTypeEntity
                {
                    Id = type,
                    Description = type.GetDescription()
                }));
    }
}
