using Testcontainers.PostgreSql;
using deeplynx.business; 
using deeplynx.datalayer.Models;
using deeplynx.models;
using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace deeplynx.tests;

public sealed class ProjectTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();
    

    public Task InitializeAsync()
    {   
        return _postgres.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }

    [Fact]
    async public void ShouldReturn3Projects()
    {
        var projectBusiness = new ProjectBusiness(new DeeplynxContext());
        
        projectBusiness.CreateProject(new ProjectRequestDto { Name = "Project One", Abbreviation = "P1" });

        projectBusiness.CreateProject(new ProjectRequestDto { Name = "Project Two", Abbreviation = "P2" });
        projectBusiness.CreateProject(new ProjectRequestDto { Name = "Project Three"});
        var projects = await projectBusiness.GetAllProjects();

        // Then
        Assert.Equal(3, projects.Count());
    }
}