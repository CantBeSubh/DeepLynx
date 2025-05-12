using deeplynx.business;
using deeplynx.datalayer.Models;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using deeplynx.models;

namespace deeplynx.tests;

public sealed class ProjectTest : IAsyncLifetime
{
    private DeeplynxContext _context;

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
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    async public void ShouldReturn3Projects()
    {

        var projectBusiness = new ProjectBusiness(_context);

        await projectBusiness.CreateProject(new ProjectRequestDto { Name = "Project One", Abbreviation = "P1" });
        await projectBusiness.CreateProject(new ProjectRequestDto { Name = "Project Two", Abbreviation = "P2" });
        await projectBusiness.CreateProject(new ProjectRequestDto { Name = "Project Three" });
        var project = await projectBusiness.GetAllProjects();

        Assert.Equal(3, project.Count());
    }
}