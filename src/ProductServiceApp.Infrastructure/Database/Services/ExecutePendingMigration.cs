using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using ProductServiceApp.Infrastructure.Database.Contexts;

namespace ProductServiceApp.Infrastructure.Database.Services;

public static class ExecutePendingMigration
{
    public static async Task ExecuteAsync(IServiceProvider serviceProvider)
    {
        var retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                MaxRetryAttempts = 10,
                Delay = TimeSpan.FromSeconds(5),
                BackoffType = DelayBackoffType.Constant,
                OnRetry = args =>
                {
                    Console.WriteLine($"[Migration] Tentativa {args.AttemptNumber + 1} — aguardando banco subir... ({args.Outcome.Exception?.Message})");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        try
        {
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await retryPipeline.ExecuteAsync(async ct =>
            {
                Console.WriteLine("[Migration] Validating if there are pending migrations...");

                var migrations = await dbContext.Database.GetPendingMigrationsAsync(ct);

                if (migrations.Any())
                {
                    await dbContext.Database.MigrateAsync(ct);

                    Console.WriteLine("[Migration] Applied migrations!");
                }
                else
                {
                    Console.WriteLine("[Migration] Has no pending migrations.");
                }

            });

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error to apply migrations: {ex.Message}");

            throw;
        }
    }
}
