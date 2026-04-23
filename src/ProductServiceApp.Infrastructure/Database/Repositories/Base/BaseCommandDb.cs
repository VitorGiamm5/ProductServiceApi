using ProductServiceApp.Domain.Repositories.Base;
using ProductServiceApp.Infrastructure.Database.Contexts;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Base;

public abstract class BaseCommandDb<T> : IBaseRepository where T : class
{
    public readonly ApplicationDbContext _context;

    public BaseCommandDb(ApplicationDbContext context)
    {
        _context = context;
    }
}
