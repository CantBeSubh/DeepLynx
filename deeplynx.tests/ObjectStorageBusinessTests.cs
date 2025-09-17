using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class ObjectStorageBusinessTests: IntegrationTestBase
{
    private ObjectStorageBusiness _objectStorageBusiness;
    private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
    private Mock<IClassBusiness> _mockClassBusiness = null!;
    private Mock<IDataSourceBusiness> _mockDataSourceBusiness = null!;
    private EventBusiness _eventBusiness = null!;
    private ProjectBusiness _projectBusiness;
    public long pid;
    public long pid2;
    public long os1;
    public long os2;
    public long archivedOs;
    
    public ObjectStorageBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _objectStorageBusiness = new ObjectStorageBusiness(Context, _cacheBusiness);
        _eventBusiness = new EventBusiness(Context,  _cacheBusiness);
        _mockLogger = new Mock<ILogger<ProjectBusiness>>();
        _mockClassBusiness = new Mock<IClassBusiness>();
        _mockDataSourceBusiness = new Mock<IDataSourceBusiness>();
        _projectBusiness = new ProjectBusiness(
            Context,
            _cacheBusiness,
            _mockLogger.Object,
            _mockClassBusiness.Object, 
            _mockDataSourceBusiness.Object, 
            _objectStorageBusiness,
            _eventBusiness);
    }

    [Fact]
    public async Task GetAllObjectStorages_Success_ReturnsAllObjectStoragesInProject()
    {
        var objectStorages = await _objectStorageBusiness.GetAllObjectStorages(pid, true);
        objectStorages.Should().HaveCount(2);
        objectStorages.First().Id.Should().Be(os1);
        objectStorages.Last().Id.Should().Be(os2);
    }
    
    [Fact]
    public async Task GetAllObjectStorages_Success_CanFilterOutArchived()
    {
        var os4Config = new JsonObject();
        os4Config["mountPath"] = "Test Project 4";
        var objectStorage5 = new ObjectStorage
        {
            Name = "Test Object Storage 5",
            Type = "filesystem",
            ProjectId = pid,
            Config = os4Config.ToString(),
            IsArchived = true
        };
        
        Context.ObjectStorages.Add(objectStorage5);
        await Context.SaveChangesAsync();
        
        var objectStorages = await _objectStorageBusiness.GetAllObjectStorages(pid, true);
        objectStorages.Should().HaveCount(2);
        objectStorages.Should().NotContain(os => os.Id == objectStorage5.Id);
        objectStorages.Should().OnlyContain(os => !os.IsArchived);
    }
    
    [Fact]
    public async Task GetAllObjectStorages_Success_CanContainArchived()
    {
        var objectStorages = await _objectStorageBusiness.GetAllObjectStorages(pid, false);
        objectStorages.Should().HaveCount(3);
        objectStorages.Should().Contain(os => os.Id == archivedOs && os.IsArchived);
        objectStorages.Should().OnlyContain(os => os.ProjectId == pid);
    }

    [Fact]
    public async Task GetAllObjectStorages_Fails_WhenProjectDoesNotExist()
    {
        var result = () => _objectStorageBusiness.GetAllObjectStorages(pid2 + 1000, true);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task GetObjectStorage_Success_ReturnsNameAndType()
    {
        var objectStorage = await _objectStorageBusiness.GetObjectStorage(pid, os1, true);
        objectStorage.Should().NotBeNull();
        objectStorage.Id.Should().Be(os1);
        objectStorage.Name.Should().Be("Test Object Storage 1");
    }
    
    [Fact]
    public async Task GetObjectStorage_Success_CanIncludeArchived()
    {
        var os5Config = new JsonObject();
        os5Config["mountPath"] = "Test Project 5";
        var objectStorage5 = new ObjectStorage
        {
            Name = "Test Object Storage 5",
            Type = "filesystem",
            ProjectId = pid,
            Config = os5Config.ToString(),
            IsArchived = true
        };
        
        Context.ObjectStorages.Add(objectStorage5);
        await Context.SaveChangesAsync();
        
        var objectStorage = await _objectStorageBusiness.GetObjectStorage(pid, objectStorage5.Id, false);
        objectStorage.Should().NotBeNull();
        objectStorage.Id.Should().Be(objectStorage5.Id);
        objectStorage.IsArchived.Should().BeTrue();
    }
    
    [Fact]
    public async Task GetObjectStorage_Fails_WhenObjectStorageIsArchived()
    {
        var os5Config = new JsonObject();
        os5Config["mountPath"] = "Test Project 5";
        var objectStorage5 = new ObjectStorage
        {
            Name = "Test Object Storage 5",
            Type = "filesystem",
            ProjectId = pid,
            Config = os5Config.ToString(),
            IsArchived = true
        };
        
        Context.ObjectStorages.Add(objectStorage5);
        await Context.SaveChangesAsync();
        
        var result = () => _objectStorageBusiness.GetObjectStorage(pid, objectStorage5.Id, true);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetDefaultObjectStorage_Success_ReturnsDefaultObject()
    {
        var defaultObjectStorage = await _objectStorageBusiness.GetDefaultObjectStorage(pid);
        defaultObjectStorage.Should().NotBeNull();
        defaultObjectStorage.Id.Should().Be(os1);
    }

    [Fact]
    public async Task GetDefaultObjectStorage_Fails_WhenNoDefault()
    {
        var result = () => _objectStorageBusiness.GetDefaultObjectStorage(pid2);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task GetDefaultObjectStorage_Fails_WhenDefaultIsArchived()
    {
        var os4Config = new JsonObject();
        os4Config["mountPath"] = "Test Project 4";
        var archivedDefaultObjectStorage = new ObjectStorage
        {
            Name = "Test Default Object Storage",
            Type = "filesystem",
            ProjectId = pid2,
            Config = os4Config.ToString(),
            IsArchived = true
        };
        Context.ObjectStorages.Add(archivedDefaultObjectStorage);
        await Context.SaveChangesAsync();
        
        var result = () => _objectStorageBusiness.GetDefaultObjectStorage(pid2);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Create_Success_ReturnsNameAndType()
    {
        var config = new JsonObject();
        config["mountPath"] = "./storage/";
        var dto = new CreateObjectStorageRequestDto
        {
            Name = "Test",
            Config = config,
        };
        
        var objectStorageResponse = await _objectStorageBusiness.CreateObjectStorage(pid, dto, true);
        
        Assert.Equal(dto.Name, objectStorageResponse.Name);
        Assert.Equal("filesystem", objectStorageResponse.Type);
    }
    
    [Fact]
    public async Task Create_Success_ReturnsNameAndCorrectType()
    {
        var config = new JsonObject();
        config["azureConnectionString"] = "example-connection-string";
        var dto = new CreateObjectStorageRequestDto
        {
            Name = "Test",
            Config = config,
        };
        
        var config2 = new JsonObject();
        config2["awsConnectionString"] = "example-connection-string";
        var dto2 = new CreateObjectStorageRequestDto
        {
            Name = "Test 2",
            Config = config2,
        };
        
        var config3 = new JsonObject();
        config3["mountPath"] = "./storage/";
        var dto3 = new CreateObjectStorageRequestDto
        {
            Name = "Test 3",
            Config = config3,
        };
        
        var objectStorageResponse = await _objectStorageBusiness.CreateObjectStorage(pid, dto, true);
        var objectStorageResponse2 = await _objectStorageBusiness.CreateObjectStorage(pid, dto2);
        var objectStorageResponse3 = await _objectStorageBusiness.CreateObjectStorage(pid, dto3);
        
        Assert.Equal(dto.Name, objectStorageResponse.Name);
        Assert.Equal(dto2.Name, objectStorageResponse2.Name);
        Assert.Equal(dto3.Name, objectStorageResponse3.Name);
        Assert.Equal("azure_object", objectStorageResponse.Type);
        Assert.Equal("aws_s3",  objectStorageResponse2.Type);
        Assert.Equal("filesystem", objectStorageResponse3.Type);
    }
    
    [Fact]
    public async Task Create_Fails_WhenConfigIsEmpty()
    {
        var config = new JsonObject();
        var dto = new CreateObjectStorageRequestDto
        {
            Name = "Test",
            Config = config,
        };
        
        var result = () => _objectStorageBusiness.CreateObjectStorage(pid, dto, true);

        await result.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Update_Success_ReturnsUpdatedName()
    {
        var updateDto = new UpdateObjectStorageRequestDto
        {
            Name = "Updated Name",
        };
        var updatedObjectStorage = await _objectStorageBusiness.UpdateObjectStorage(pid, os1, updateDto);
        Assert.Equal(updateDto.Name, updatedObjectStorage.Name);
    }

    [Fact]
    public async Task Update_Fails_WhenObjectStorageDoesNotExist()
    {
        var updateDto = new UpdateObjectStorageRequestDto
        {
            Name = "Updated Name",
        };
        var result = () => _objectStorageBusiness.UpdateObjectStorage(pid, os1 + 1000, updateDto);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task Update_Fails_WhenObjectStorageIsArchived()
    {
        var updateDto = new UpdateObjectStorageRequestDto
        {
            Name = "Updated Name",
        };
        var result = () => _objectStorageBusiness.UpdateObjectStorage(pid, archivedOs, updateDto);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Delete_Success_RemovesObjectStorage()
    {
        var deleted =  await _objectStorageBusiness.DeleteObjectStorage(pid, os2);
        deleted.Should().BeTrue();
        
        var deletedObjectStorage = await Context.ObjectStorages.Where(os => os.ProjectId == pid && os.Id == os2).FirstOrDefaultAsync();
        deletedObjectStorage.Should().BeNull();
    }

    [Fact]
    public async Task Delete_Fails_WhenObjectStorageDoesNotExist()
    {
        var deleted =  () => _objectStorageBusiness.DeleteObjectStorage(pid, os1 + 1000);
        await deleted.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task Delete_Fails_WhenObjectStorageIsDefault()
    {
        var deleted =  () => _objectStorageBusiness.DeleteObjectStorage(pid, os1);
        await deleted.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Archive_Success_ArchivesObjectStorage()
    {
        var archived = await _objectStorageBusiness.ArchiveObjectStorage(pid, os2);
        archived.Should().BeTrue();
        
        var archivedObjectStorage = await Context.ObjectStorages.Where(os => os.Id == os2 && os.ProjectId == pid).FirstOrDefaultAsync();
        archivedObjectStorage.Should().NotBeNull();
        archivedObjectStorage.Id.Should().Be(os2);
        archivedObjectStorage.IsArchived.Should().BeTrue();
    }
    
    [Fact]
    public async Task Archive_Fails_IfObjectStorageDoesNotExist()
    {
        var archived = () => _objectStorageBusiness.ArchiveObjectStorage(pid, os1 + 1000);
        await archived.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task Archive_Fails_IfObjectStorageIsDefault()
    {
        var archived = () => _objectStorageBusiness.ArchiveObjectStorage(pid, os1);
        await archived.Should().ThrowAsync<InvalidOperationException>();
    }
    
    [Fact]
    public async Task Archive_Fails_IfObjectStorageIsAlreadyArchived()
    {
        var archived = () => _objectStorageBusiness.ArchiveObjectStorage(pid, archivedOs);
        await archived.Should().ThrowAsync<InvalidOperationException>();
        
    }

    [Fact]
    public async Task Unarchive_Success_UnarchivesObjectStorage()
    {
        var unarchived = await _objectStorageBusiness.UnarchiveObjectStorage(pid, archivedOs);
        unarchived.Should().BeTrue();
        
        var unarchivedObjectStorage = await Context.ObjectStorages.Where(os => os.Id == archivedOs && os.ProjectId == pid).FirstOrDefaultAsync();
        unarchivedObjectStorage.Should().NotBeNull();
        unarchivedObjectStorage.Id.Should().Be(archivedOs);
        unarchivedObjectStorage.IsArchived.Should().BeFalse();
    }
    
    [Fact]
    public async Task Unarchive_Fails_WhenObjectStorageDoesNotExist()
    {
        var unarchived = () => _objectStorageBusiness.UnarchiveObjectStorage(pid, archivedOs + 1000);
        await unarchived.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task Unarchive_Fails_WhenObjectIsAlreadyUnarchived()
    {
        var unarchived = () => _objectStorageBusiness.UnarchiveObjectStorage(pid, os1);
        await unarchived.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ChangeDefault_Success_ChangesDefault()
    {
        var newDefaultObjectStorage = await _objectStorageBusiness.SetDefaultObjectStorage(pid, os2);
        newDefaultObjectStorage.Should().NotBeNull();
        newDefaultObjectStorage.Id.Should().Be(os2);
        newDefaultObjectStorage.Default.Should().BeTrue();
        
        var oldDefaultObjectStorage = await Context.ObjectStorages.Where(os => os.Id == os1 && os.ProjectId == pid).FirstOrDefaultAsync();
        oldDefaultObjectStorage.Should().NotBeNull();
        oldDefaultObjectStorage.Id.Should().Be(os1);
        oldDefaultObjectStorage.Default.Should().BeFalse();
    }
    
    [Fact]
    public async Task ChangeDefault_Fails_WhenObjectStorageDoesNotExist()
    {
        var result = () => _objectStorageBusiness.SetDefaultObjectStorage(pid, os1 + 1000);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ObjectStoragesArchived_WhenProjectArchived()
    {
        var result = await _projectBusiness.ArchiveProject(pid);
        result.Should().BeTrue();
        
        // need to clear change tracker for stored procedure
        Context.ChangeTracker.Clear();
        
        var archivedObjectStorages = await Context.ObjectStorages.Where(os => os.ProjectId == pid).ToListAsync();
        archivedObjectStorages.Should().NotBeNull();
        archivedObjectStorages.Should().OnlyContain(os => os.IsArchived);
    }

    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();
        var project = new Project() { Name = "Test Project 1" };
        var project2 = new Project() { Name = "Test Project 2" };
        Context.Projects.Add(project);
        Context.Projects.Add(project2);
        await Context.SaveChangesAsync();
        pid = project.Id;
        pid2 = project2.Id;

        var os1Config = new JsonObject();
        os1Config["mountPath"] = "Test Project 1";
        var objectStorage = new ObjectStorage
        {
            Name = "Test Object Storage 1",
            ProjectId = pid,
            Type = "filesystem",
            Config = os1Config.ToString(),
            Default = true
        };
        
        var os2Config = new JsonObject();
        os2Config["mountPath"] = "Test Project 2";
        var objectStorage2 = new ObjectStorage
        {
            Name = "Test Object Storage 2",
            Type = "filesystem",
            ProjectId = pid,
            Config = os2Config.ToString()
        };
        
        var os3Config = new JsonObject();
        os3Config["mountPath"] = "Test Project 3";
        var objectStorage3 = new ObjectStorage
        {
            Name = "Test Object Storage 3",
            ProjectId = pid2,
            Type = "filesystem",
            Config = os3Config.ToString()
        };
        
        var os4Config = new JsonObject();
        os4Config["mountPath"] = "Test Project 4";
        var objectStorage4 = new ObjectStorage
        {
            Name = "Test Object Storage 4",
            Type = "filesystem",
            ProjectId = pid2,
            Config = os4Config.ToString()
        };
        
        var os5Config = new JsonObject();
        os5Config["mountPath"] = "Test Project 5";
        var objectStorage5 = new ObjectStorage
        {
            Name = "Test Object Storage 5",
            Type = "filesystem",
            ProjectId = pid,
            Config = os5Config.ToString(),
            IsArchived = true
        };
        
        Context.ObjectStorages.Add(objectStorage);
        Context.ObjectStorages.Add(objectStorage2);
        Context.ObjectStorages.Add(objectStorage3);
        Context.ObjectStorages.Add(objectStorage4);
        Context.ObjectStorages.Add(objectStorage5);
        await Context.SaveChangesAsync();
        os1 = objectStorage.Id;
        os2 = objectStorage2.Id;
        archivedOs = objectStorage5.Id;
    }
}