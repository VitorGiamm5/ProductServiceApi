using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Infrastructure.Database.Configuration.Base;

namespace ProductServiceApp.Infrastructure.Database.Configuration.Products;

public class ProductConfiguration : BaseAuditConfiguration<ProductEntity>
{
    public override void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("tb_product");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn(seed: 100000, increment: 1);

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

        builder.HasData(
            new ProductEntity
            {
                Id = 100000,
                Name = "Batata frita",
                Price = 2m,
                Type = ProductsTypeEnum.Fries,
                CreatedByUserId = 1,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            },
            new ProductEntity
            {
                Id = 100001,
                Name = "X Burger",
                Price = 5m,
                Type = ProductsTypeEnum.Sandwich,
                CreatedByUserId = 1,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            },
            new ProductEntity
            {
                Id = 100002,
                Name = "X Egg",
                Price = 4.50m,
                Type = ProductsTypeEnum.Sandwich,
                CreatedByUserId = 1,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            },
            new ProductEntity
            {
                Id = 100003,
                Name = "X Bacon",
                Price = 7m,
                Type = ProductsTypeEnum.Sandwich,
                CreatedByUserId = 1,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            },
            new ProductEntity
            {
                Id = 100004,
                Name = "Refrigerante",
                Price = 2.50m,
                Type = ProductsTypeEnum.Refreshment,
                CreatedByUserId = 1,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            }
        );

    }
}
