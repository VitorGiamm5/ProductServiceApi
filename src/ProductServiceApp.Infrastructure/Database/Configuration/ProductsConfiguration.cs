using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductServiceApp.Domain.Products.Entities;
using ProductServiceApp.Infrastructure.Database.Configuration.Base;

namespace ProductServiceApp.Infrastructure.Database.Configuration;

public class ProductsConfiguration : BaseAuditConfiguration<ProductEntity>
{
    public override void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("tb_product");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
               .HasColumnName("name")    // No Postgres, prefira snake_case (minúsculas)
               .HasColumnType("varchar") // Especifica o tipo nativo
               .HasMaxLength(150)        // Limita o tamanho para otimizar índices
               .IsRequired();            // Ajuda o plano de execução a ignorar checagens de nulo

        builder.Property(e => e.Price)
               .HasColumnName("price")
               .HasColumnType("numeric(10,2)"); // Define precisão e escala para valores monetários

        builder.Property(e => e.Type)
               .HasColumnName("type")
               .HasConversion<byte>();  // Armazena como byte no banco

        // Configura o ID para ser Identity e iniciar em 100000
        builder.Property(e => e.Id)
               .UseIdentityColumn(seed: 100000, increment: 1);
    }
}
