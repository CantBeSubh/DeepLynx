using Microsoft.EntityFrameworkCore;
using deeplynx.models;

namespace deeplynx.tests;
public class ProjectTests : IClassFixture<ProjectContainerFixture>
{
    private readonly ProjectContainerFixture _fixture;

    public ProjectTests(ProjectContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateProject_Should_Add_Project()
    {
        var dto = new ProjectRequestDto { Name = "Test Project", Abbreviation = "TP" };
        var project = await _fixture.ProjectBusiness.CreateProject(dto);
        Assert.NotNull(project);
        Assert.Equal("Test Project", project.Name);
        Assert.Equal("TP", project.Abbreviation);
    }

    [Fact]
    public async Task GetAllProjects_Should_Return_All_Projects()
    {
        await _fixture.ProjectBusiness.CreateProject(new ProjectRequestDto { Name = "Proj1", Abbreviation = "P1" });
        await _fixture.ProjectBusiness.CreateProject(new ProjectRequestDto { Name = "Proj2", Abbreviation = "P2" });
        var projects = await _fixture.ProjectBusiness.GetAllProjects();
        Assert.NotNull(projects);
        Assert.Equal(2, await _fixture.Context.Projects.CountAsync());
    }

    [Fact]
    public async Task GetProject_Should_Return_Project_If_Exists()
    {
        var created = await _fixture.ProjectBusiness.CreateProject(new ProjectRequestDto { Name = "Proj", Abbreviation = "PR" });
        var fetched = await _fixture.ProjectBusiness.GetProject(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Proj", fetched.Name);
    }

    // Additional tests...

    [Fact]
    public async Task DeleteProject_Should_Throw_If_Not_Exists()
    {
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.ProjectBusiness.DeleteProject(999));
        Assert.Equal("Project not found.", ex.Message);
    }
}