using deeplynx.business;
using deeplynx.datalayer.Models;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using deeplynx.models;

namespace deeplynx.tests;

public sealed class ProjectTests : IAsyncLifetime
{
    private DeeplynxContext _context;
    public ProjectBusiness _projectBusiness; 
    public TagBusiness _tagBusiness; 
    public EdgeMappingBusiness _edgeMappingBusiness;
    public RelationshipBusiness _relationshipBusiness;
    public ClassBusiness _classBusiness;
    public RecordMappingBusiness _recordMappingBusiness;
    public EdgeBusiness _edgeBusiness;
    public DataSourceBusiness _dataSourceBusiness;

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
         .WithImage("postgres:15-alpine")
         .Build();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _context = new DeeplynxContext(options);
        await _context.Database.MigrateAsync(); 
        _projectBusiness = new ProjectBusiness(
            _context,
            _tagBusiness,
            _edgeMappingBusiness,
            _relationshipBusiness,
            _classBusiness,
           _recordMappingBusiness,
            _edgeBusiness,
            _dataSourceBusiness
            );
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
    
    // Creates a project and checks that the created values are not null 
    [Fact]
    public async Task CreateProject_Should_Add_Project()
    {
        var dto = new ProjectRequestDto { Name = "Test Project", Abbreviation = "TP" };
        var project = await _projectBusiness.CreateProject(dto);
        Assert.NotNull(project);
        Assert.Equal("Test Project", project.Name);
        Assert.Equal("TP", project.Abbreviation);
    }
    // Creates two projects and checks that two were creataed 
    [Fact]
    public async Task GetAllProjects_Should_Return_All_Projects()
    {
        await _projectBusiness.CreateProject(new ProjectRequestDto { Name = "Proj1", Abbreviation = "P1" });
        await _projectBusiness.CreateProject(new ProjectRequestDto { Name = "Proj2", Abbreviation = "P2" });
        var projects = await _projectBusiness.GetAllProjects();
        Assert.NotNull(projects);
        Assert.Equal(2, await _context.Projects.CountAsync());
    }
    // Create project compare against returned project with GetProject()
    [Fact]
    public async Task GetProject_Should_Return_Project_If_Exists()
    {
        var created = await _projectBusiness.CreateProject(new ProjectRequestDto { Name = "Proj", Abbreviation = "PR" });
        var fetched = await _projectBusiness.GetProject(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Proj", fetched.Name);
    }
    // Project that doesn't exist throws error 
    [Fact]
    public async Task GetProject_Should_Throw_If_Not_Exists()
    {
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _projectBusiness.GetProject(999));
        Assert.Equal("Project not found.", ex.Message);
    }
    // Create project, update name and abbreviation fields successfully 
    [Fact]
    public async Task UpdateProject_Should_Modify_Fields()
    {
        var created = await _projectBusiness.CreateProject(new ProjectRequestDto { Name = "Old", Abbreviation = "OLD" });
        var updated = await _projectBusiness.UpdateProject(created.Id, new ProjectRequestDto { Name = "New", Abbreviation = "NEW" });
        Assert.Equal("New", updated.Name);
        Assert.Equal("NEW", updated.Abbreviation);
    }
    // Create and delete project successfully 
    [Fact]
    public async Task DeleteProject_Should_Remove_Project()
    {
        var created = await _projectBusiness.CreateProject(new ProjectRequestDto { Name = "ToDelete", Abbreviation = "TD" });
        var result = await _projectBusiness.DeleteProject(created.Id);
        Assert.True(result);
        Assert.Null(await _context.Projects.FindAsync(created.Id));
    }
    // Project that doesn't exist cannot be deleted 
    [Fact]
    public async Task DeleteProject_Should_Throw_If_Not_Exists()
    {
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _projectBusiness.DeleteProject(999));
        Assert.Equal("Project not found.", ex.Message);
    }
}