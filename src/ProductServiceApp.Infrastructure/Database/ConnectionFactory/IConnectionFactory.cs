using Npgsql;

namespace ProductServiceApp.Infrastructure.Database.ConnectionFactory;

public interface IConnectionFactory
{
    NpgsqlConnection CreateWriteConnection();
    NpgsqlConnection CreateReadConnection();
}
