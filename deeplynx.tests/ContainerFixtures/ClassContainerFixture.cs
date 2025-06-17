using deeplynx.business;
using deeplynx.datalayer.Models;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using deeplynx.models;

public class ClassContainerFixture : IAsyncLifetime
{
    public DeeplynxContext Context { get; private set; }
    public ProjectBusiness ProjectBusiness { get; private set; }
    public TagBusiness TagBusiness { get; private set; }
    public EdgeMappingBusiness EdgeMappingBusiness { get; private set; }
    public RelationshipBusiness RelationshipBusiness { get; private set; }
    public ClassBusiness ClassBusiness { get; private set; }
    public RecordMappingBusiness RecordMappingBusiness { get; private set; }
    public EdgeBusiness EdgeBusiness { get; private set; }
    public DataSourceBusiness DataSourceBusiness { get; private set; }
    public RecordBusiness RecordBusiness { get; private set; }
    public RoleBusiness RoleBusiness { get; private set; }

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
        TagBusiness = new TagBusiness(Context);
        EdgeMappingBusiness = new EdgeMappingBusiness(Context);
        RelationshipBusiness = new RelationshipBusiness(Context);
        ClassBusiness = new ClassBusiness(Context);
        RecordMappingBusiness = new RecordMappingBusiness(Context);
        EdgeBusiness = new EdgeBusiness(Context);
        RecordBusiness = new RecordBusiness(Context, EdgeBusiness);
        RoleBusiness = new RoleBusiness(Context);
        DataSourceBusiness = new DataSourceBusiness(Context, EdgeBusiness, RecordBusiness);

        // Initialize ProjectBusiness with dependencies
        ProjectBusiness = new ProjectBusiness(
            Context,
            TagBusiness,
            EdgeMappingBusiness,
            RelationshipBusiness,
            ClassBusiness,
            RecordMappingBusiness,
            EdgeBusiness,
            DataSourceBusiness,
            RecordBusiness,
            RoleBusiness
        );
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}