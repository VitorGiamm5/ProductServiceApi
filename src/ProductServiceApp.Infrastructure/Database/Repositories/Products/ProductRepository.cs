using ProductServiceApp.Infrastructure.Database.ConnectionFactory;
using ProductServiceApp.Infrastructure.Database.Contexts;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Products;

public class UserRepository
{
    private readonly ApplicationDbContext _write;
    private readonly ReadOnlyDbContext _read;
    private readonly IConnectionFactory _factory;

    public UserRepository(ApplicationDbContext write, ReadOnlyDbContext read, IConnectionFactory factory)
    {
        _write = write;
        _read = read;
        _factory = factory;
    }
}
