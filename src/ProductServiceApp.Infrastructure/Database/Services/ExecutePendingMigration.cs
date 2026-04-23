using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProductServiceApp.Infrastructure.Database.Services;

public static class ExecutePendingMigration
{
    public static void Execute(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            var migrations = dbContext.Database.GetPendingMigrations();

            if (migrations.Any())
            {
                Console.WriteLine($"Apply migrations success");

                dbContext.Database.MigrateAsync().Wait();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error to apply migrations: {ex.Message}");

            throw;
        }
    }
}
