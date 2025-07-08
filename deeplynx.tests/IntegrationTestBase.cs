// using deeplynx.datalayer.Models;
// using deeplynx.tests;
// using Testcontainers.PostgreSql;
// using Microsoft.EntityFrameworkCore;
// using Xunit;
//
//
// public class TestSuiteFixture : IAsyncLifetime
// {
//     private readonly PostgreSqlContainer _container; 
//     protected DeeplynxContext Context { get; private set; }
//     public async Task InitializeAsync()
//     {
//         await _container.StartAsync();
//
//         // Create DbContext with container connection string
//         var options = new DbContextOptionsBuilder<DeeplynxContext>()
//             .UseNpgsql(_container.GetConnectionString())
//             .Options;
//
//         Context = new DeeplynxContext(options);
//
//         // Apply migrations to ensure database is created and up-to-date
//         await Context.Database.MigrateAsync();
//         await SeedData.SeedDatabase(Context);
//         Console.WriteLine("Initializing database...");
//     }
//
//     public async Task DisposeAsync()
//     {
//         await Context.DisposeAsync();
//         await _container.DisposeAsync();
//         Console.WriteLine("Disposing database...");
//     }
// }
//
// [CollectionDefinition("Test Suite Collection")]
// public class TestSuiteCollection : ICollectionFixture<TestSuiteFixture>
// {
//     // This class has no code, and is never created. Its purpose is simply
//     // to be the place to apply [CollectionDefinition] and ICollectionFixture<> interfaces.
// }
//
// [Collection("Test Suite Collection")]
// public class IntegrationTestBase : IAsyncLifetime
// {
//     private readonly PostgreSqlContainer _container; 
//     protected DeeplynxContext Context { get; private set; }
//
//     protected IntegrationTestBase()
//     {
//         _container = new PostgreSqlBuilder()
//             .WithImage("postgres:15-alpine")
//             .Build();
//     }
//
//     public async virtual Task InitializeAsync()
//     {
//         
//     }
//
//     public async Task DisposeAsync()
//     {
//         // await Context.DisposeAsync();
//         // await _container.DisposeAsync();
//     }
//
//     /// <summary>
//     /// Clean database between tests
//     /// </summary>
//     protected async Task CleanDatabaseAsync()
//     {
//         var projects = await Context.Projects.ToListAsync();
//         Context.Projects.RemoveRange(projects);
//         var datasources = await Context.DataSources.ToListAsync();
//         Context.DataSources.RemoveRange(datasources);
//         var classes = await Context.Classes.ToListAsync();
//         Context.Classes.RemoveRange(classes);
//         var records = await Context.Records.ToListAsync();
//         Context.Records.RemoveRange(records);
//         await Context.SaveChangesAsync();
//     }
// }


// using deeplynx.datalayer.Models;
// using deeplynx.tests;
// using Testcontainers.PostgreSql;
// using Microsoft.EntityFrameworkCore;
// using Xunit;
//
// public class TestSuiteFixture : IAsyncLifetime
// {
//     private readonly PostgreSqlContainer _container;
//     public DeeplynxContext Context { get; private set; }
//
//     public TestSuiteFixture()
//     {
//         _container = new PostgreSqlBuilder()
//             .WithImage("postgres:15-alpine")
//             .Build();
//     }
//
//     public async Task InitializeAsync()
//     {
//         await _container.StartAsync();
//
//         // Create DbContext with container connection string
//         var options = new DbContextOptionsBuilder<DeeplynxContext>()
//             .UseNpgsql(_container.GetConnectionString())
//             .Options;
//
//         Context = new DeeplynxContext(options);
//
//         // Apply migrations to ensure database is created and up-to-date
//         await Context.Database.MigrateAsync();
//         await SeedData.SeedDatabase(Context);
//         Console.WriteLine("Initializing database...");
//     }
//
//     public async Task DisposeAsync()
//     {
//         await Context.DisposeAsync();
//         await _container.DisposeAsync();
//         Console.WriteLine("Disposing database...");
//     }
// }
//
// [CollectionDefinition("Test Suite Collection")]
// public class TestSuiteCollection : ICollectionFixture<TestSuiteFixture>
// {
//     // This class has no code, and is never created. Its purpose is simply
//     // to be the place to apply [CollectionDefinition] and ICollectionFixture<> interfaces.
// }
//
// [Collection("Test Suite Collection")]
// public class IntegrationTestBase : IAsyncLifetime
// {
//     protected DeeplynxContext Context { get; private set; }
//     private readonly TestSuiteFixture _fixture;
//
//     protected IntegrationTestBase(TestSuiteFixture fixture)
//     {
//         _fixture = fixture;
//         Context = _fixture.Context;
//     }
//
//     public async virtual Task InitializeAsync()
//     {
//         // Any test-specific initialization can be done here
//     }
//
//     public async Task DisposeAsync()
//     {
//         // Any test-specific cleanup can be done here
//     }
//
//     /// <summary>
//     /// Clean database between tests
//     /// </summary>
//     protected async Task CleanDatabaseAsync()
//     {
//         var projects = await Context.Projects.ToListAsync();
//         Context.Projects.RemoveRange(projects);
//         var datasources = await Context.DataSources.ToListAsync();
//         Context.DataSources.RemoveRange(datasources);
//         var classes = await Context.Classes.ToListAsync();
//         Context.Classes.RemoveRange(classes);
//         var records = await Context.Records.ToListAsync();
//         Context.Records.RemoveRange(records);
//         await Context.SaveChangesAsync();
//     }
// }


using deeplynx.datalayer.Models;
using deeplynx.tests;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

public class TestSuiteFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    public string ConnectionString { get; private set; }
    public DeeplynxContext Context { get; private set; }

    public TestSuiteFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        Context = new DeeplynxContext(options);
        
        // Apply migrations and seed database only once
        await Context.Database.MigrateAsync();
        await SeedData.SeedDatabase(Context);
        Console.WriteLine("Initializing database...");
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _container.DisposeAsync();
        Console.WriteLine("Disposing database...");
    }
}

[CollectionDefinition("Test Suite Collection")]
public class TestSuiteCollection : ICollectionFixture<TestSuiteFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and ICollectionFixture<> interfaces.
}

[Collection("Test Suite Collection")]
public class IntegrationTestBase : IAsyncLifetime
{
    protected DeeplynxContext Context { get; private set; }
    private readonly TestSuiteFixture _fixture;

    protected IntegrationTestBase(TestSuiteFixture fixture)
    {
        _fixture = fixture;
        Context = new DeeplynxContext(new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options);
    }

    public async virtual Task InitializeAsync()
    {
        // var options = new DbContextOptionsBuilder<DeeplynxContext>()
        //     .UseNpgsql(_fixture.ConnectionString)
        //     .Options;
        //
        // Context = new DeeplynxContext(options);
        //
        // Context.ChangeTracker.Clear();
        //
        // // Apply migrations to ensure database is created and up-to-date
        // await Context.Database.MigrateAsync();
        // await SeedData.SeedDatabase(Context);
        await CleanDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
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