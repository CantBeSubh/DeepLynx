using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.tests;
using DotNetEnv;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Microsoft.EntityFrameworkCore;

// Fixture to allow setting up and breaking down what is needed for each test suite
public class TestSuiteFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;
    public string PostgresConnectionString { get; private set; }
    public string RedisConnectionString { get; private set; }
    public DeeplynxContext Context { get; private set; }

    public TestSuiteFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
    }
    
    // Runs at the beginning of every test suite
    public async Task InitializeAsync()
    {
        // Apply env variables first
        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
        var envFilePath = Path.Combine(projectRoot, ".env");
        Env.Load(envFilePath);
        
        // Start containers
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();
        
        // CRITICAL: Set Redis connection string as environment variable
        // This must happen BEFORE any CacheBusiness instance is created
        // The Redis test container generates a connection string dynamically
        RedisConnectionString = _redisContainer.GetConnectionString();
        Environment.SetEnvironmentVariable("REDIS_CONNECTION_STRING", RedisConnectionString);
        
        Console.WriteLine($"Redis Connection String set: {RedisConnectionString}");
        
        PostgresConnectionString = _postgresContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(PostgresConnectionString)
            .Options;

        Context = new DeeplynxContext(options);
        
        // Apply migrations only once
        await Context.Database.MigrateAsync();
        
        // ensure the notification service is tested
        Environment.SetEnvironmentVariable("ENABLE_NOTIFICATION_SERVICE", "true");
    }
    
    // Runs at the end of every test suite
    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
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
    protected CacheBusiness _cacheBusiness;

    protected IntegrationTestBase(TestSuiteFixture fixture)
    {
        _fixture = fixture;
        Context = new DeeplynxContext(new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(_fixture.PostgresConnectionString)
            .Options);
        _cacheBusiness = CacheBusiness.Instance;
    }

    // Runs before every test in the test suite
    public virtual async Task InitializeAsync()
    {
        await SeedTestDataAsync();
    }

    // Runs after every test in the test suite
    public async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("CACHE_PROVIDER_TYPE", null);
        await Context.DisposeAsync();
        await _cacheBusiness.FlushAsync();
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
        var relationships = await Context.Relationships.ToListAsync();
        Context.Relationships.RemoveRange(relationships);
        var tags = await Context.Tags.ToListAsync();
        Context.Tags.RemoveRange(tags);
        var dataSources = await Context.DataSources.ToListAsync();
        Context.DataSources.RemoveRange(dataSources);
        var subscriptions = await Context.Subscriptions.ToListAsync();
        Context.Subscriptions.RemoveRange(subscriptions);
        var actions = await Context.Actions.ToListAsync();
        Context.Actions.RemoveRange(actions);
        var events = await Context.Events.ToListAsync();
        Context.Events.RemoveRange(events);
        var permissions = await Context.Permissions.ToListAsync();
        Context.Permissions.RemoveRange(permissions);
        await Context.SaveChangesAsync();
        await _cacheBusiness.FlushAsync();
    }
    
    protected virtual async Task SeedTestDataAsync()
    {
        await CleanDatabaseAsync();
    }
}