using deeplynx.datalayer.Models;
using deeplynx.tests;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

// fixture to allow setting up and breaking down what is needed for each test suite
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
    
    //Runs at the beginning of every test suite
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        // Allows the integration test base to access the connection string to instantiate a new context
        ConnectionString = _container.GetConnectionString();
        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        Context = new DeeplynxContext(options);
        
        // Apply migrations only once
        await Context.Database.MigrateAsync();
    }
    
    //Runs at the end of every test suite
    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _container.DisposeAsync();
    }
}


// Defines a test collection named "Test Suite Collection".
// This collection uses the TestSuiteFixture class for setup and teardown.
[CollectionDefinition("Test Suite Collection")]
public class TestSuiteCollection : ICollectionFixture<TestSuiteFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and ICollectionFixture<> interfaces.
}


// Indicates that this test class is part of the "Test Suite Collection".
// The TestSuiteFixture setup and teardown code will be applied to this class.
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

    //Runs before every test in the test suite
    public virtual async Task InitializeAsync()
    {
        await SeedTestDataAsync();
        // await SeedData.SeedDatabase(Context);
    }

    //Runs after every test in the test suite
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
        var users = await Context.Users.ToListAsync();
        Context.Users.RemoveRange(users);
        var edges = await Context.Edges.ToListAsync();
        Context.Edges.RemoveRange(edges);
        var edgeMappings =  await Context.EdgeMappings.ToListAsync();
        Context.EdgeMappings.RemoveRange(edgeMappings);
        var recordMappings =  await Context.RecordMappings.ToListAsync();
        Context.RecordMappings.RemoveRange(recordMappings);
        var relationships = await Context.Relationships.ToListAsync();
        Context.Relationships.RemoveRange(relationships);
        var tags = await Context.Tags.ToListAsync();
        Context.Tags.RemoveRange(tags);
        var dataSources = await Context.DataSources.ToListAsync();
        Context.DataSources.RemoveRange(dataSources);
        await Context.SaveChangesAsync();
    }
    
    
    protected virtual async Task SeedTestDataAsync()
    {
        await CleanDatabaseAsync();
    }
}