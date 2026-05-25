using ProductServiceApp.Infrastructure.Database.ConnectionFactory;
using ProductServiceApp.Infrastructure.Database.Contexts;

namespace ProductServiceApp.Infrastructure.Database.Repositories.Products;

public class UserRepository(ApplicationDbContext write, ReadOnlyDbContext read, IConnectionFactory factory)
{
    private readonly ApplicationDbContext _write = write;
    private readonly ReadOnlyDbContext _read = read;
    private readonly IConnectionFactory _factory = factory;
}
