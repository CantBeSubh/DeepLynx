using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Moq;
using Npgsql;
using Xunit;


namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class ClassIntegrationTests : IntegrationTestBase
{
    private ClassBusiness _classBusiness = null!;
    private Mock<IEdgeMappingBusiness> _edgeMapping;
    private Mock<IRecordMappingBusiness> _recordMapping;
    private Mock<IRecordBusiness> _recordBusiness;
    private Mock<IRelationshipBusiness> _relationshipBusiness;
    public long pid;

    public ClassIntegrationTests(TestSuiteFixture fixture) : base(fixture) {}

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _recordBusiness = new Mock<IRecordBusiness>();
        _relationshipBusiness = new Mock<IRelationshipBusiness>();
        _recordMapping = new Mock<IRecordMappingBusiness>();
        _edgeMapping = new Mock<IEdgeMappingBusiness>();
        _classBusiness = new ClassBusiness(Context, _edgeMapping.Object, _recordBusiness.Object, _recordMapping.Object, _relationshipBusiness.Object);
    }
    
    [Fact]
    public async Task CreateClass_Success_ReturnsIdAndCreatedAt()
    {
        var dto = new ClassRequestDto { Name = "C1", Description = "D1", Uuid = "U1" };
    
        var result = await _classBusiness.CreateClass(pid, dto);
        result.Name.Should().BeSameAs("C1");
        result.Id.Should().BeGreaterThan(0);
      }
    
    [Fact]
    public async Task CreateClassRequest_Fails_IfNoName()
    { 
        var missingNameDto = new ClassRequestDto { Name = null, Description = "D", Uuid = "U" };
        var result  = async () => await _classBusiness.CreateClass(pid, missingNameDto);
        await result.Should().ThrowAsync<ValidationException>();
    }
    
    
    [Fact]
    public async Task CreateClass_Succeeds_IfNoDescriptionOrUuid()
    {
        var dto = new ClassRequestDto { Name = "C" };
        var result = await _classBusiness.CreateClass(pid, dto);
    
        result.Name.Should().Be("C");
        result.Id.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task CreateClass_Fails_IfProjectNotFound()
    {
        var missing = pid + 999;
        var dto = new ClassRequestDto { Name = "C" };
    
        var result = () => _classBusiness.CreateClass(missing, dto);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task CreateClass_Fails_IfProjectDeleted()
    {
        var project = await Context.Projects.FindAsync(pid);
        project.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        Context.Projects.Update(project);
        await Context.SaveChangesAsync();
        var dto = new ClassRequestDto { Name = "C" };
        var result = () => _classBusiness.CreateClass(pid, dto);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    
    [Fact]
    public async Task GetAllClasses_ReturnsOnlyForProject()
    {
        var p2 = new Project { Name = "ExtraProj" };
        Context.Projects.Add(p2);
        await Context.SaveChangesAsync();
    
        await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = "C1" });
        await _classBusiness.CreateClass(p2.Id, new ClassRequestDto { Name = "C2" });
    
        var list = await _classBusiness.GetAllClasses(pid);
        Assert.All(list, c => Assert.Equal(pid, c.ProjectId));
    }
    
    
    [Fact]
    public async Task GetAllClasses_ExcludesSoftDeleted()
    {
        var class1 = new Class { Name = "Proj", ProjectId = pid};
        var class2 = new Class { Name = "Proj2", ProjectId = pid, ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified) };
        Context.Classes.Add(class1);
        Context.Classes.Add(class2);
        await Context.SaveChangesAsync();
        
        var list = await _classBusiness.GetAllClasses(pid);
        Assert.DoesNotContain(list, c => c.Id == class2.Id);
    }
    
    [Fact]
    public async Task GetClass_Success_WhenExists()
    {
        var created = await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = "C" });
        var result = await _classBusiness.GetClass(pid, created.Id);
        Assert.Equal(created.Id, result.Id);
    }
    
    [Fact]
    public async Task GetClass_Fails_IfNotFound()
    {
        var result = () => _classBusiness.GetClass(pid, 9999);
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
        var created = await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = "Old" });
        var updated = await _classBusiness.UpdateClass(pid, created.Id, new ClassRequestDto { Name = "New" });
        Assert.Equal("New", updated.Name);
        Assert.NotNull(updated.ModifiedAt);
    }
    
    [Fact]
    public async Task UpdateClass_Fails_IfNotFound()
    {
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
    
    
    
    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();
        
        var project = new Project { Name = "Project 2" };
        
        Context.Projects.Add(project);
        
        await Context.SaveChangesAsync();
        pid = project.Id;
    }
}