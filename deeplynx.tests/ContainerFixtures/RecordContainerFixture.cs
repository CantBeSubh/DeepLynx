using deeplynx.business;
using deeplynx.datalayer.Models;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

public class RecordContainerFixture : IAsyncLifetime
{
    public DeeplynxContext Context { get; private set; }
    public RecordBusiness RecordBusiness { get; private set; }
    public EdgeBusiness EdgeBusiness { get; private set; }

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        Context = new DeeplynxContext(options);
        await Context.Database.MigrateAsync();

        // Initialize the business classes
        EdgeBusiness = new EdgeBusiness(Context);
        RecordBusiness = new RecordBusiness(Context, EdgeBusiness);
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    public async Task<(long ProjectId, long DataSourceId)> SeedProjectAndDataSource(
        bool deletedProject = false,
        bool deletedDataSource = false)
    {
        var project = new Project { Name = "Proj", Abbreviation = "P" };
        if (deletedProject)
            project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        Context.Projects.Add(project);

        var dataSource = new DataSource { Name = "DS", Project = project };
        if (deletedDataSource)
            dataSource.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        Context.DataSources.Add(dataSource);
        await Context.SaveChangesAsync();
        return (project.Id, dataSource.Id);
    }
}