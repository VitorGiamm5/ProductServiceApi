using Microsoft.EntityFrameworkCore;
using ProductServiceApp.Domain.Repositories.Base;
using ProductServiceApp.Domain.Repositories.Exceptions;
using ProductServiceApp.Infrastructure.Database.Contexts;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Base;

public abstract class BaseQueryRepository<T> : IBaseQueryRepository<T> where T : class
{
    protected readonly ReadOnlyDbContext _context;

    protected BaseQueryRepository(ReadOnlyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<T>()
            .ToListAsync(cancellationToken);
    }

    public async Task<T> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        if (id <= 0) throw new ArgumentException("Id inválido.", nameof(id));

        return await _context.Set<T>()
            .FirstOrDefaultAsync(e => EF.Property<long>(e, "Id") == id, cancellationToken)
            ?? throw new NotFoundException(typeof(T).Name, id);
    }
}
