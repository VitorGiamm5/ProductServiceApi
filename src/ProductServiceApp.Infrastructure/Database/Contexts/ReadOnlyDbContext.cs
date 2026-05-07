using Microsoft.EntityFrameworkCore;
using ProductServiceApp.Domain.Security;

namespace ProductServiceApp.Infrastructure.Database.Contexts;

// Contexto dedicado para leitura — aponta para a réplica
public class ReadOnlyDbContext : ApplicationDbContext
{
    public ReadOnlyDbContext(
        DbContextOptions<ReadOnlyDbContext> options,
        ICurrentUserContext currentUserContext) : base(options, currentUserContext)
    {
    }

    public override int SaveChanges()
        => throw new InvalidOperationException("ReadOnlyDbContext não permite escrita.");

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => throw new InvalidOperationException("ReadOnlyDbContext não permite escrita.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("ReadOnlyDbContext não permite escrita.");

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("ReadOnlyDbContext não permite escrita.");
}
