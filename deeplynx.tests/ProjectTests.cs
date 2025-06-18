using Microsoft.EntityFrameworkCore;
using deeplynx.models;

namespace deeplynx.tests;
public class ProjectTests : IClassFixture<IntegrationTestBase>
{
    private readonly IntegrationTestBase _fixture;

    public ProjectTests(IntegrationTestBase fixture)
    {
        _fixture = fixture;
    }

    // [Fact]
    // public async Task CreateProject_Should_Add_Project()
    // {
    //     var dto = new ProjectRequestDto { Name = "Test Project", Abbreviation = "TP" };
    //     var project = await _fixture._ProjectBusiness.CreateProject(dto);
    //     Assert.NotNull(project);
    //     Assert.Equal("Test Project", project.Name);
    // }
    //
    // [Fact]
    // public async Task GetAllProjects_Should_Return_All_Projects()
    // {
    //     var projects = await _fixture._ProjectBusiness.GetAllProjects();
    //     Assert.NotNull(projects);
    //     Assert.Equal(3, await _fixture.Context.Projects.CountAsync());
    // }
    //
    // [Fact]
    // public async Task GetProject_Should_Return_Project_If_Exists()
    // {
    //     var created = await _fixture._ProjectBusiness.CreateProject(new ProjectRequestDto { Name = "Hello", Abbreviation = "PR" });
    //     var fetched = await _fixture._ProjectBusiness.GetProject(created.Id);
    //     Assert.NotNull(fetched);
    //     Assert.Equal("Hello", fetched.Name);
    // }
    //
    // // Additional tests...
    //
    // [Fact]
    // public async Task DeleteProject_Should_Throw_If_Not_Exists()
    // {
    //     var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture._ProjectBusiness.DeleteProject(999));
    //     Assert.Equal("Project not found.", ex.Message);
    // }
}