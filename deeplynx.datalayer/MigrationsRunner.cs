using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using deeplynx.datalayer.Models;

namespace deeplynx.datalayer.MigrationRunner
{
    public static class MigrationRunner
    {
        public static void ApplyMigrations(string connectionString)
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection, connectionString);

                var serviceProvider = serviceCollection.BuildServiceProvider();

                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DeeplynxContext>();
                    dbContext.Database.Migrate();
                }

                Console.WriteLine("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
                //Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Is the database connection string correct?");
                Console.WriteLine("Migrations were NOT applied.");
            }
        }

        private static void ConfigureServices(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<DeeplynxContext>(options =>
                options.UseNpgsql(connectionString));
        }
    }
}