using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.tests;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class IntegrationTestBase : IAsyncLifetime
{
    
    private readonly PostgreSqlContainer _container; 
    protected DeeplynxContext Context { get; private set; }


    protected IntegrationTestBase()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();
    }

    public async virtual Task InitializeAsync()
    {
        await _container.StartAsync();

        // Create DbContext with container connection string
        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        Context = new DeeplynxContext(options);

        // Ensure database is created
        await Context.Database.EnsureCreatedAsync();
        await SeedData.SeedDatabase(Context);
    }


    public async Task DisposeAsync()
     {
         await Context.DisposeAsync();
         await _container.DisposeAsync();
     }

     /// <summary>
     /// Clean database between tests
     /// </summary>
     protected async Task CleanDatabaseAsync()
     {
         var projects = await Context.Projects.ToListAsync();
         Context.Projects.RemoveRange(projects);
         var datasources = await Context.DataSources.ToListAsync();
         Context.DataSources.RemoveRange(datasources);
         var classes = await Context.Classes.ToListAsync();
         Context.Classes.RemoveRange(classes);
         var records = await Context.Records.ToListAsync();
         Context.Records.RemoveRange(records);
         await Context.SaveChangesAsync();
     }
    
}
