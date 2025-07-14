
using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.models;
using FluentAssertions;
using Npgsql;
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
        var result  = async () => await _classBusiness.CreateClass(pid, missingNameDto);
        await result.Should().ThrowAsync<ValidationException>();
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
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task CreateClass_Fails_IfProjectDeleted()
    {
        await SeedTestDataAsync(true);
        var dto = new ClassRequestDto { Name = "C" };
        var result = () => _classBusiness.CreateClass(pid, dto);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    
    [Fact]
    public async Task GetAllClasses_ReturnsOnlyForProject()
    {
        await SeedTestDataAsync();
        var p2 = new Project { Name = "ExtraProj" };
        Context.Projects.Add(p2);
        await Context.SaveChangesAsync();
    
        await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = "C1" });
        await _classBusiness.CreateClass(p2.Id, new ClassRequestDto { Name = "C2" });
    
        var list = await _classBusiness.GetAllClasses(pid, false);
        Assert.All(list, c => Assert.Equal(pid, c.ProjectId));
    }
    
    
    [Fact]
    public async Task GetAllClasses_ExcludesSoftDeleted()
    {
        await SeedTestDataAsync();
        var class1 = new Class { Name = "Proj", ProjectId = pid};
        var class2 = new Class { Name = "Proj2", ProjectId = pid, ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified) };
        Context.Classes.Add(class1);
        Context.Classes.Add(class2);
        await Context.SaveChangesAsync();
        
        var list = await _classBusiness.GetAllClasses(pid, true);
        Assert.DoesNotContain(list, c => c.Id == class2.Id);
    }
    
    [Fact]
    public async Task GetClass_Success_WhenExists()
    {
        await SeedTestDataAsync();
        var created = await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = "C" });
        var result = await _classBusiness.GetClass(pid, created.Id, false);
        Assert.Equal(created.Id, result.Id);
    }
    
    [Fact]
    public async Task GetClass_Fails_IfNotFound()
    {
        await SeedTestDataAsync();
        var result = () => _classBusiness.GetClass(pid, 9999, false);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    // [Fact]
    // public async Task GetClass_Fails_IfDeleted()
    // {
    //     await SeedTestDataAsync();
    //     var created = await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = "C" });
    //     await _classBusiness.DeleteClass(pid, created.Id);
    //     var result = await _classBusiness.GetClass(pid, created.Id);
    //    Assert.Null(result);
    // }
    
    
    [Fact]
    public async Task UpdateClass_Success_ReturnsModifiedAt()
    {
        await SeedTestDataAsync();
        var created = await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = "Old" });
        var updated = await _classBusiness.UpdateClass(pid, created.Id, new ClassRequestDto { Name = "New" });
        Assert.Equal("New", updated.Name);
        Assert.NotNull(updated.ModifiedAt);
    }
    
    [Fact]
    public async Task UpdateClass_Fails_IfNotFound()
    {
        await SeedTestDataAsync();
        var result = () => _classBusiness.UpdateClass(pid, 9999, new ClassRequestDto { Name = "X" });
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    // [Fact]
    // public async Task UpdateClass_Fails_IfDeleted()
    // {
    //     await SeedTestDataAsync();
    //     var created = await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = "C" });
    //     await _classBusiness.DeleteClass(pid, created.Id);
    //     
    //     var result = () => _classBusiness.UpdateClass(pid, created.Id, new ClassRequestDto { Name = "Y" });
    //     await result.Should().ThrowAsync<KeyNotFoundException>();
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
            project.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        }
        
        Context.Projects.Add(project);
        
        await Context.SaveChangesAsync();
        pid = project.Id;
    }
}