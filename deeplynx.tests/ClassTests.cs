
using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.models;
using FluentAssertions;
using Xunit;


namespace deeplynx.tests;

public class ClassIntegrationTests : IntegrationTestBase
{
    private ClassBusiness _classBusiness = null!;
    private EdgeMappingBusiness _edgeMapping = null!;
    private RecordMappingBusiness _recordMapping = null!;
    private RecordBusiness _recordBusiness = null!;
    private RelationshipBusiness _relationshipBusiness = null!;
    public long pid; 

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _classBusiness = new ClassBusiness(Context, _edgeMapping, _recordBusiness, _recordMapping, _relationshipBusiness);
    }
    
    [Fact]
    public async Task CreateClass_Success_ReturnsIdAndCreatedAt()
    {
        await SeedTestDataAsync();
        var dto = new ClassRequestDto { Name = "C1", Description = "D1", Uuid = "U1" };
    
        var result = await _classBusiness.CreateClass(pid, dto);
        result.Name.Should().BeSameAs("C1");
        result.Id.Should().BeGreaterThan(0);
      }
    
    [Fact]
    public async Task CreateClassRequest_Fails_IfNoName()
    { 
        await SeedTestDataAsync();
        var missingNameDto = new ClassRequestDto { Name = null, Description = "D", Uuid = "U" };
        var result  = () => _classBusiness.CreateClass(pid, missingNameDto);
        result.Should().ThrowAsync<ValidationException>();
    }
    
    
    [Fact]
    public async Task CreateClass_Succeeds_IfNoDescriptionOrUuid()
    {
        await SeedTestDataAsync();
        var dto = new ClassRequestDto { Name = "C" };
        var result = await _classBusiness.CreateClass(pid, dto);
    
        result.Name.Should().Be("C");
        result.Id.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task CreateClass_Fails_IfProjectNotFound()
    {
        await SeedTestDataAsync();
        var missing = pid + 999;
        var dto = new ClassRequestDto { Name = "C" };
    
        var result = () => _classBusiness.CreateClass(missing, dto);
        result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task CreateClass_Fails_IfProjectDeleted()
    {
        await SeedTestDataAsync(true);
        var dto = new ClassRequestDto { Name = "C" };
        var result = () => _classBusiness.CreateClass(pid, dto);
        result.Should().ThrowAsync<KeyNotFoundException>();
    }
    //
    //
    // [Fact]
    // public async Task GetAllClasses_ReturnsOnlyForProject()
    // {
    //     var p1 =  _fixture.pid;
    //     var p2 = new Project { Name = "ExtraProj" };
    //     _fixture.Context.Projects.Add(p2);
    //     await _fixture.Context.SaveChangesAsync();
    //
    //     await _fixture.ClassBusiness.CreateClass(p1, new ClassRequestDto { Name = "C1" });
    //     await _fixture.ClassBusiness.CreateClass(p2.Id, new ClassRequestDto { Name = "C2" });
    //
    //     var list = await _fixture.ClassBusiness.GetAllClasses(p1);
    //     Assert.All(list, c => Assert.Equal(p1, c.ProjectId));
    // }
    //
    //
    // [Fact]
    // public async Task GetAllClasses_ExcludesSoftDeleted()
    // {
    //     var pid = _fixture.pid;
    //     var class1 = new Class { Name = "Proj", ProjectId = pid};
    //     var class2 = new Class { Name = "Proj2", ProjectId = pid, DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified) };
    //     _fixture.Context.Classes.Add(class1);
    //     _fixture.Context.Classes.Add(class2);
    //     await _fixture.Context.SaveChangesAsync();
    //     
    //     var list = await _fixture.ClassBusiness.GetAllClasses(pid);
    //     Assert.DoesNotContain(list, c => c.Id == class2.Id);
    // }
    //
    // [Fact]
    // public async Task GetClass_Success_WhenExists()
    // {
    //     var pid = _fixture.pid;
    //     var created = await _fixture.ClassBusiness.CreateClass(pid, new ClassRequestDto { Name = "C" });
    //
    //     var result = await _fixture.ClassBusiness.GetClass(pid, created.Id);
    //     Assert.Equal(created.Id, result.Id);
    // }
    //
    // [Fact]
    // public async Task GetClass_Fails_IfNotFound()
    // {
    //     var pid = _fixture.pid;
    //     await Assert.ThrowsAsync<KeyNotFoundException>(
    //         () => _fixture.ClassBusiness.GetClass(pid, 9999));
    // }
    //
    // // [Fact]
    // // public async Task GetClass_Fails_IfDeleted()
    // // {
    // //     var pid = _fixture.pid;
    // //     var created = await _fixture.ClassBusiness.CreateClass(pid, new ClassRequestDto { Name = "C" });
    // //     await _fixture.ClassBusiness.DeleteClass(pid, created.Id);
    // //
    // //     await Assert.ThrowsAsync<KeyNotFoundException>(
    // //         () => _fixture.ClassBusiness.GetClass(pid, created.Id));
    // // }
    // //
    //
    // [Fact]
    // public async Task UpdateClass_Success_ReturnsModifiedAt()
    // {
    //     var pid = _fixture.pid;
    //     var created = await _fixture.ClassBusiness.CreateClass(pid, new ClassRequestDto { Name = "Old" });
    //
    //     var updated = await _fixture.ClassBusiness.UpdateClass(pid, created.Id, new ClassRequestDto { Name = "New" });
    //     Assert.Equal("New", updated.Name);
    //     Assert.NotNull(updated.ModifiedAt);
    // }
    //
    // [Fact]
    // public async Task UpdateClass_Fails_IfNotFound()
    // {
    //     var pid = _fixture.pid;
    //     await Assert.ThrowsAsync<KeyNotFoundException>(
    //         () => _fixture.ClassBusiness.UpdateClass(pid, 9999, new ClassRequestDto { Name = "X" }));
    // }
    
    // [Fact]
    // public async Task UpdateClass_Fails_IfDeleted()
    // {
    //     var pid = _fixture.pid;
    //     var created = await _fixture.ClassBusiness.CreateClass(pid, new ClassRequestDto { Name = "C" });
    //     await _fixture.ClassBusiness.DeleteClass(pid, created.Id);
    //
    //     await Assert.ThrowsAsync<KeyNotFoundException>(
    //         () => _fixture.ClassBusiness.UpdateClass(pid, created.Id, new ClassRequestDto { Name = "Y" }));
    // }
    //
    // [Fact]
    // public async Task DeleteClass_SoftDelete_SetsDeletedAt()
    // {
    //     var pid = _fixture.pid;
    //     var created = await _fixture.ClassBusiness.CreateClass(pid, new ClassRequestDto { Name = "C" });
    //
    //     var result = await _fixture.ClassBusiness.DeleteClass(pid, created.Id);
    //     Assert.True(result);
    //
    //     var entity = await _fixture.Context.Classes.FindAsync(created.Id);
    //     Assert.NotNull(entity.DeletedAt);
    // }

    [Fact(Skip = "Force delete not implemented yet")]
    public async Task ForceDeleteClass_RemovesFromDatabase()
    {
        //  future force delete logic
    }

    [Fact(Skip = "Cascade delete not implemented yet")]
    public async Task DeleteClass_DeletesDownstreamRelationships()
    {
        // Placeholder for cascading relationship deletion
    }

    [Fact(Skip = "Cascade delete not implemented yet")]
    public async Task DeleteClass_DeletesDownstreamRecords()
    {
        // Placeholder for cascading record deletion
    }

    [Fact(Skip = "Cascade delete not implemented yet")]
    public async Task DeleteClass_DeletesDownstreamRecordMappings()
    {
        // Placeholder for cascading record mapping deletion
    }
    
    private async Task SeedTestDataAsync(bool deleteProject = false)
    {
        await CleanDatabaseAsync();
        
        var project = new Project { Name = "Project 2" };
        if (deleteProject)
        {
            project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        }
        
        Context.Projects.Add(project);
        
        await Context.SaveChangesAsync();
        pid = project.Id;
    }
}