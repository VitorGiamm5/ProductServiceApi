using ProductServiceApp.Domain.Repositories.Base;
using ProductServiceApp.Infrastructure.Database.Contexts;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Base;

public abstract class BaseQueryDb<T> : IBaseRepository where T : class
{
    public readonly ReadOnlyDbContext _context;

    protected BaseQueryDb(ReadOnlyDbContext context)
    {
        _context = context;
    }
}
