using Microsoft.EntityFrameworkCore;

namespace ProductServiceApp.Infrastructure.Database.Contexts;

// Contexto dedicado para leitura — aponta para a réplica
public class ReadOnlyDbContext : ApplicationDbContext
{
    public ReadOnlyDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public override int SaveChanges()
        => throw new InvalidOperationException("ReadOnlyDbContext não permite escrita.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("ReadOnlyDbContext não permite escrita.");
}
