using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests;

public class DataSourceBusinessTests : IntegrationTestBase
{
    private readonly EventBusiness _eventBusiness;
    private readonly Mock<IEdgeBusiness> _mockEdgeBusiness;
    private readonly Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
    private readonly Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
    private readonly Mock<IRecordBusiness> _mockRecordBusiness;
    private readonly INotificationBusiness _notificationBusiness = null!;
    private DataSourceBusiness _dataSourceBusiness;
    public long did;
    public long did2;
    public long did3;
    private long oid;
    public long pid;
    public long pid2;
    private long uid;

    public DataSourceBusinessTests(TestSuiteFixture fixture) : base(fixture)
    {
        _mockEdgeBusiness = new Mock<IEdgeBusiness>();
        _mockRecordBusiness = new Mock<IRecordBusiness>();
        _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
        _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
        _notificationBusiness =
            new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
        _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _dataSourceBusiness = new DataSourceBusiness(
            Context,
            _cacheBusiness,
            _mockEdgeBusiness.Object,
            _mockRecordBusiness.Object,
            _eventBusiness);
    }

    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();

        var org = new Organization { Name = "Test Org" };
        Context.Organizations.Add(org);
        await Context.SaveChangesAsync();
        oid = org.Id;

        var testUser = new User
        {
            Name = "John Smith",
            Email = "john.smith@company.com",
            Password = "test_password",
            IsArchived = false
        };
        Context.Users.Add(testUser);
        await Context.SaveChangesAsync();
        uid = testUser.Id;

        var project = new Project { Name = "Project 1", OrganizationId = oid };
        var project2 = new Project { Name = "Project2", OrganizationId = oid };
        Context.Projects.Add(project);
        Context.Projects.Add(project2);

        await Context.SaveChangesAsync();
        pid = project.Id;
        pid2 = project2.Id;

        var dataSource = new DataSource
        {
            Name = "Customer CRM Database",
            Description = "Primary customer relationship management database",
            Abbreviation = "CRM_DB",
            Type = "SQL Server",
            BaseUri = "Server=crm-prod.company.com;Database=CustomerData;",
            Config =
                @"{""driver"":""sqlserver"",""host"":""crm-prod.company.com"",""port"":1433,""database"":""CustomerData"",""ssl_enabled"":true}",
            ProjectId = pid,
            LastUpdatedBy = testUser.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
            IsArchived = false,
            OrganizationId = oid
        };
        var dataSource2 = new DataSource
        {
            Name = "Customer CRM Database",
            Description = "Primary customer relationship management database",
            Abbreviation = "CRM_DB",
            Type = "SQL Server",
            BaseUri = "Server=crm-prod.company.com;Database=CustomerData;",
            Config =
                @"{""driver"":""sqlserver"",""host"":""crm-prod.company.com"",""port"":1433,""database"":""CustomerData"",""ssl_enabled"":true}",
            ProjectId = pid,
            LastUpdatedBy = testUser.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
            IsArchived = false,
            OrganizationId = oid
        };
        var dataSource3 = new DataSource
        {
            Name = "Customer CRM Database",
            Description = "Primary customer relationship management database",
            Abbreviation = "CRM_DB",
            Type = "SQL Server",
            BaseUri = "Server=crm-prod.company.com;Database=CustomerData;",
            Config =
                @"{""driver"":""sqlserver"",""host"":""crm-prod.company.com"",""port"":1433,""database"":""CustomerData"",""ssl_enabled"":true}",
            ProjectId = pid,
            LastUpdatedBy = testUser.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
            IsArchived = true,
            OrganizationId = oid
        };
        Context.DataSources.Add(dataSource);
        Context.DataSources.Add(dataSource2);
        Context.DataSources.Add(dataSource3);
        await Context.SaveChangesAsync();
        did = dataSource.Id;
        did2 = dataSource2.Id;
        did3 = dataSource3.Id;

        // var tag = new Tag { Name = "Tag 1", ProjectId = pid};
        // Context.Tags.Add(tag);
        // await Context.SaveChangesAsync();
        // tid = tag.Id;
        // var testClass = new Class{Name = "Class 1", ProjectId = pid};
        // Context.Classes.Add(testClass);
        // await Context.SaveChangesAsync();
        // var testClass2 = new Class{Name = "Class 2", ProjectId = pid};
        // Context.Classes.Add(testClass2);
        // await Context.SaveChangesAsync();
        // cid = testClass.Id;
        // var dataSource1 = new DataSource { Name = "DataSource 1", ProjectId = pid };
        // Context.DataSources.Add(dataSource1);
        // await Context.SaveChangesAsync();
        // did = dataSource1.Id;
    }

    #region GetAllDataSources Tests

    [Fact]
    public async Task GetAllDataSources_ValidProjectId_ReturnsActiveDataSources()
    {
        // Act
        var result = await _dataSourceBusiness.GetAllDataSources(pid, true);
        var dataSources = result.ToList();

        // Assert
        Assert.Equal(2, dataSources.Count);
        Assert.All(dataSources, ds => Assert.Equal(pid, ds.ProjectId));
        Assert.All(dataSources, ds => Assert.False(ds.IsArchived));
        Assert.Contains(dataSources, ds => ds.Id == did);
        Assert.Contains(dataSources, ds => ds.Id == did2);
        Assert.DoesNotContain(dataSources, ds => ds.Id == did3);
    }

    [Fact]
    public async Task GetAllDataSources_NonExistentProjectWithNoDataSources_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.GetAllDataSources(999, false));

        Assert.Contains("Project with id 999 not found", exception.Message);
    }

    [Fact]
    public async Task GetAllDataSources_DifferentProject_ReturnsCorrectDataSources()
    {
        // Arrange
        Context.DataSources.Add(new DataSource { Name = "Project 2 Data Source", ProjectId = pid2, OrganizationId = oid });
        await Context.SaveChangesAsync();

        // Act
        var result = await _dataSourceBusiness.GetAllDataSources(pid, true);
        var dataSources = result.ToList();

        // Assert
        Assert.Equal(2, dataSources.Count);
        Assert.DoesNotContain(dataSources, ds => ds.Name == "Project 2 Data Source");
        Assert.All(dataSources, ds => Assert.Equal(pid, ds.ProjectId));
    }

    [Fact]
    public async Task GetAllDataSources_ConfigParsing_ReturnsValidJsonObject()
    {
        // Act
        var result = await _dataSourceBusiness.GetAllDataSources(pid, false);
        var dataSource = result.First(ds => ds.Id == did);

        // Assert
        Assert.NotNull(dataSource.Config);
        Assert.Equal("sqlserver", dataSource.Config["driver"]?.ToString());
        Assert.Equal("crm-prod.company.com", dataSource.Config["host"]?.ToString());
        Assert.Equal(1433, dataSource.Config["port"]?.GetValue<int>());
    }

    [Fact]
    public async Task GetAllDataSources_NullConfig_ReturnsEmptyJsonObject()
    {
        // Arrange
        var dataSourceWithNullConfig = new DataSource
        {
            Name = "Null Config Test",
            Description = "Primary customer relationship management database",
            Abbreviation = "CRM_DB",
            Type = "SQL Server",
            BaseUri = "Server=crm-prod.company.com;Database=CustomerData;",
            Config = null,
            ProjectId = pid,
            LastUpdatedBy = uid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
            IsArchived = false,
            OrganizationId = oid
        };
        await Context.DataSources.AddAsync(dataSourceWithNullConfig);
        await Context.SaveChangesAsync();

        // Act
        var result = await _dataSourceBusiness.GetAllDataSources(pid, false);
        var dataSource = result.First(ds => ds.Name == "Null Config Test");

        // Assert
        Assert.NotNull(dataSource.Config);
        Assert.Empty(dataSource.Config);
    }

    #endregion

    #region GetAllDataSourcesMultiProject Tests

    [Fact]
    public async Task GetAllDataSourcesMultiProject_ValidProjectIds_ReturnsDataSourcesFromAllProjects()
    {
        // Arrange
        var projectIds = new[] { pid, pid2 };

        // Act
        var result = await _dataSourceBusiness.GetAllDataSourcesMultiProject(projectIds, true);
        var dataSources = result.ToList();

        // Assert
        Assert.Equal(2, dataSources.Count); // 2 from pid (did and did2), 0 from pid2
        Assert.Contains(dataSources, ds => ds.Id == did && ds.ProjectId == pid);
        Assert.Contains(dataSources, ds => ds.Id == did2 && ds.ProjectId == pid);
        Assert.DoesNotContain(dataSources, ds => ds.Id == did3); // archived
    }

    [Fact]
    public async Task GetAllDataSourcesMultiProject_SingleProjectId_ReturnsSameAsGetAllDataSources()
    {
        // Arrange
        var projectIds = new[] { pid };

        // Act
        var multiProjectResult = await _dataSourceBusiness.GetAllDataSourcesMultiProject(projectIds, true);
        var singleProjectResult = await _dataSourceBusiness.GetAllDataSources(pid, true);

        // Assert
        Assert.Equal(singleProjectResult.Count(), multiProjectResult.Count);
        Assert.All(multiProjectResult, ds => Assert.Equal(pid, ds.ProjectId));
    }

    [Fact]
    public async Task GetAllDataSourcesMultiProject_EmptyProjectIdsArray_ReturnsEmptyList()
    {
        // Arrange
        var projectIds = Array.Empty<long>();

        // Act
        var result = await _dataSourceBusiness.GetAllDataSourcesMultiProject(projectIds, true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllDataSourcesMultiProject_NonExistentProjectIds_ReturnsEmptyList()
    {
        // Arrange
        var projectIds = new long[] { 999, 998 };

        // Act
        var result = await _dataSourceBusiness.GetAllDataSourcesMultiProject(projectIds, true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllDataSourcesMultiProject_HideArchivedFalse_ReturnsArchivedDataSources()
    {
        // Arrange
        var projectIds = new[] { pid };

        // Act
        var result = await _dataSourceBusiness.GetAllDataSourcesMultiProject(projectIds, false);
        var dataSources = result.ToList();

        // Assert
        Assert.Equal(3, dataSources.Count); // did, did2, and did3 (archived)
        Assert.Contains(dataSources, ds => ds.Id == did3 && ds.IsArchived);
    }

    [Fact]
    public async Task GetAllDataSourcesMultiProject_HideArchivedTrue_ExcludesArchivedDataSources()
    {
        // Arrange
        var projectIds = new[] { pid };

        // Act
        var result = await _dataSourceBusiness.GetAllDataSourcesMultiProject(projectIds, true);
        var dataSources = result.ToList();

        // Assert
        Assert.Equal(2, dataSources.Count);
        Assert.DoesNotContain(dataSources, ds => ds.Id == did3);
        Assert.All(dataSources, ds => Assert.False(ds.IsArchived));
    }

    [Fact]
    public async Task GetAllDataSourcesMultiProject_ConfigParsing_ReturnsValidJsonObject()
    {
        // Arrange
        var projectIds = new[] { pid };

        // Act
        var result = await _dataSourceBusiness.GetAllDataSourcesMultiProject(projectIds, false);
        var dataSource = result.First(ds => ds.Id == did);

        // Assert
        Assert.NotNull(dataSource.Config);
        Assert.Equal("sqlserver", dataSource.Config["driver"]?.ToString());
        Assert.Equal("crm-prod.company.com", dataSource.Config["host"]?.ToString());
        Assert.Equal(1433, dataSource.Config["port"]?.GetValue<int>());
    }

    [Fact]
    public async Task GetAllDataSourcesMultiProject_NullConfig_ReturnsEmptyJsonObject()
    {
        // Arrange
        var dataSourceWithNullConfig = new DataSource
        {
            Name = "Null Config Multi Test",
            Config = null,
            ProjectId = pid,
            LastUpdatedBy = uid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            IsArchived = false,
            OrganizationId = oid
        };
        await Context.DataSources.AddAsync(dataSourceWithNullConfig);
        await Context.SaveChangesAsync();

        var projectIds = new[] { pid };

        // Act
        var result = await _dataSourceBusiness.GetAllDataSourcesMultiProject(projectIds, false);
        var dataSource = result.First(ds => ds.Name == "Null Config Multi Test");

        // Assert
        Assert.NotNull(dataSource.Config);
        Assert.Empty(dataSource.Config);
    }

    #endregion

    #region GetDataSource Tests

    [Fact]
    public async Task GetDataSource_ValidIds_ReturnsDataSource()
    {
        // Act
        var result = await _dataSourceBusiness.GetDataSource(pid, did, false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(did, result.Id);
        Assert.Equal("Customer CRM Database", result.Name);
        Assert.Equal("Primary customer relationship management database", result.Description);
        Assert.Equal("CRM_DB", result.Abbreviation);
        Assert.Equal("SQL Server", result.Type);
        Assert.Equal("Server=crm-prod.company.com;Database=CustomerData;", result.BaseUri);
        Assert.Equal(pid, result.ProjectId);
        Assert.NotNull(result.Config);
    }

    [Fact]
    public async Task GetDataSource_NonExistentDataSource_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.GetDataSource(pid, 999, false));

        Assert.Contains("Data Source with id 999 not found", exception.Message);
    }

    [Fact]
    public async Task GetDataSource_WrongProject_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.GetDataSource(pid2, did,
                false)); // DataSource belongs to project with pid, not pid2

        Assert.Contains($"Data Source with id {did} not found", exception.Message);
    }

    [Fact]
    public async Task GetDataSource_ArchivedDataSource_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.GetDataSource(pid, did3, true)); // did3 is archived

        Assert.Contains($"Data Source with id {did3} is archived", exception.Message);
    }

    [Fact]
    public async Task GetDataSource_ValidDataSource_ParsesConfigCorrectly()
    {
        // Act
        var result = await _dataSourceBusiness.GetDataSource(pid, did, false);

        // Assert
        Assert.NotNull(result.Config);
        Assert.Equal("sqlserver", result.Config["driver"]?.ToString());
        Assert.Equal("crm-prod.company.com", result.Config["host"]?.ToString());
        Assert.Equal(1433, result.Config["port"]?.GetValue<int>());
    }

    #endregion

    #region CreateDataSource Tests

    [Fact]
    public async Task CreateDataSource_ValidDto_ReturnsCorrectValues()
    {
        // Arrange
        var config = new JsonObject
        {
            ["driver"] = "postgresql",
            ["host"] = "localhost",
            ["port"] = 5432
        };

        var dto = new CreateDataSourceRequestDto
        {
            Name = "New Test Data Source",
            Description = "A newly created test data source",
            Abbreviation = "NEW_TEST",
            Type = "PostgreSQL",
            BaseUri = "Server=localhost;Database=NewTest;",
            Config = config
        };

        // Act
        var result = await _dataSourceBusiness.CreateDataSource(uid, pid, dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("New Test Data Source", result.Name);
        Assert.Equal("A newly created test data source", result.Description);
        Assert.Equal("NEW_TEST", result.Abbreviation);
        Assert.Equal("PostgreSQL", result.Type);
        Assert.Equal("Server=localhost;Database=NewTest;", result.BaseUri);
        Assert.Equal(pid, result.ProjectId);
        Assert.NotNull(result.Config);
        Assert.Equal("postgresql", result.Config["driver"]?.ToString());
        Assert.Equal(uid, result.LastUpdatedBy);

        // Verify it was actually saved to database
        var savedDataSource = await Context.DataSources.FindAsync(result.Id);
        Assert.NotNull(savedDataSource);
        Assert.Equal("New Test Data Source", savedDataSource.Name);

        // Ensure that datasource create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("create", actualEvent.Operation);
        Assert.Equal("data_source", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task CreateDataSource_NullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _dataSourceBusiness.CreateDataSource(uid, pid, null));

        // Ensure that datasource create event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task CreateDataSource_NullConfig_CreatesWithEmptyConfig()
    {
        // Arrange
        var dto = new CreateDataSourceRequestDto
        {
            Name = "No Config Data Source",
            Description = "Data source without config",
            Type = "File System"
        };

        // Act
        var result = await _dataSourceBusiness.CreateDataSource(uid, pid, dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Config);
        Assert.Empty(result.Config);

        // Ensure that datasource create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("create", actualEvent.Operation);
        Assert.Equal("data_source", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task CreateDataSource_SetsCreatedAtAndCreatedBy()
    {
        // Arrange
        var dto = new CreateDataSourceRequestDto
        {
            Name = "Timestamp Test",
            Type = "Test"
        };

        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _dataSourceBusiness.CreateDataSource(uid, pid, dto);

        // Assert
        Assert.True(result.LastUpdatedAt >= beforeCreate);
        Assert.True(result.LastUpdatedAt <= DateTime.UtcNow);
        Assert.Equal(uid, result.LastUpdatedBy);
        // Ensure that datasource create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("create", actualEvent.Operation);
        Assert.Equal("data_source", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    #endregion

    #region UpdateDataSource Tests

    [Fact]
    public async Task UpdateDataSource_ValidUpdate_UpdatesDataSource()
    {
        // Arrange
        var config = new JsonObject
        {
            ["driver"] = "mysql",
            ["host"] = "updated.com",
            ["port"] = 3306
        };

        var dto = new UpdateDataSourceRequestDto
        {
            Name = "Updated Test Data Source",
            Description = "Updated description",
            Abbreviation = "UPD_TEST",
            Type = "MySQL",
            BaseUri = "Server=updated.com;Database=UpdatedDB;",
            Config = config
        };

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _dataSourceBusiness.UpdateDataSource(uid, pid, did, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(did, result.Id);
        Assert.True(result.LastUpdatedAt >= beforeUpdate);
        Assert.True(result.LastUpdatedAt <= DateTime.UtcNow);
        Assert.Equal("Updated Test Data Source", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("UPD_TEST", result.Abbreviation);
        Assert.Equal("MySQL", result.Type);
        Assert.Equal("Server=updated.com;Database=UpdatedDB;", result.BaseUri);
        Assert.Equal("mysql", result?.Config?["driver"]?.ToString());

        // Verify it was actually updated in database
        var updatedDataSource = await Context.DataSources.FindAsync(did);
        Assert.Equal("Updated Test Data Source", updatedDataSource?.Name);

        // Ensure that datasource update event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("update", actualEvent.Operation);
        Assert.Equal("data_source", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task UpdateDataSource_PartialUpdate_UpdatesDataSource()
    {
        // Arrange
        var updateDto = new UpdateDataSourceRequestDto
        {
            Description = "Updated Description"
        };

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _dataSourceBusiness.UpdateDataSource(uid, pid, did, updateDto);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(did, result.Id);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(pid, result.ProjectId);
        Assert.Equal(uid, result.LastUpdatedBy);
        Assert.True(result.LastUpdatedAt >= beforeUpdate);
        Assert.Equal("Customer CRM Database", result.Name);
        Assert.Equal("CRM_DB", result.Abbreviation);
        Assert.Equal("SQL Server", result.Type);
        Assert.Equal("Server=crm-prod.company.com;Database=CustomerData;", result.BaseUri);
        Assert.NotNull(result.Config);
        Assert.False(result.IsArchived);

        // Verify it was actually updated in database
        var updatedDataSource = await Context.DataSources.FindAsync(did);
        Assert.NotNull(updatedDataSource);
        Assert.Equal("Updated Description", updatedDataSource.Description);
        Assert.NotNull(updatedDataSource?.LastUpdatedAt);

        // Ensure that datasource update event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("update", actualEvent.Operation);
        Assert.Equal("data_source", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task UpdateDataSource_NonExistentDataSource_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateDataSourceRequestDto
        {
            Name = "Update Test",
            Type = "Test"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.UpdateDataSource(uid, pid, 999, dto));

        Assert.Contains("Data Source with id 999 not found", exception.Message);

        // Ensure that datasource update event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UpdateDataSource_WrongProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateDataSourceRequestDto
        {
            Name = "Update Test",
            Type = "Test"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.UpdateDataSource(uid, pid2, did, dto)); // did belongs to pid not pid2

        Assert.Contains($"Data Source with id {did} not found", exception.Message);

        // Ensure that datasource update event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UpdateDataSource_ArchivedDataSource_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateDataSourceRequestDto
        {
            Name = "Update Test",
            Type = "Test"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.UpdateDataSource(uid, pid, did3, dto)); // DataSource 3 is archived

        Assert.Contains($"Data Source with id {did3} not found", exception.Message);

        // Ensure that datasource update event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UpdateDataSource_NullConfig_UpdatesWithEmptyConfig()
    {
        // Arrange
        var dto = new UpdateDataSourceRequestDto
        {
            Name = "No Config Update",
            Type = "Test",
            Config = null
        };

        // Act
        var result = await _dataSourceBusiness.UpdateDataSource(uid, pid, did, dto);

        // Assert
        Assert.NotNull(result.Config);
        Assert.Empty(result.Config);

        // Ensure that datasource update event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("update", actualEvent.Operation);
        Assert.Equal("data_source", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    #endregion

    #region DeleteDataSource Tests

    [Fact]
    public async Task DeleteDataSource_ValidDataSource_DeletesSuccessfully()
    {
        // Act
        var result = await _dataSourceBusiness.DeleteDataSource(pid, did);

        // Assert
        Assert.True(result);

        // Verify it was actually deleted from database
        var deletedDataSource = await Context.DataSources.FindAsync(did);
        Assert.Null(deletedDataSource);
    }

    [Fact]
    public async Task DeleteDataSource_NonExistentDataSource_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.DeleteDataSource(pid, 999));

        Assert.Contains("Data Source with id 999 not found", exception.Message);
    }

    [Fact]
    public async Task DeleteDataSource_WrongProject_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.DeleteDataSource(pid2, did)); // DataSource 1 belongs to project 1

        Assert.Contains($"Data Source with id {did} not found", exception.Message);
    }

    [Fact]
    public async Task DeleteDataSource_ArchivedDataSource_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.DeleteDataSource(pid, 3)); // DataSource 3 is archived

        Assert.Contains("Data Source with id 3 not found", exception.Message);
    }

    #endregion

    #region ArchiveDataSource Tests

    [Fact]
    public async Task ArchiveDataSource_ValidDataSource_ArchivesSuccessfully()
    {
        var now = DateTime.UtcNow;
        // Act
        var result = await _dataSourceBusiness.ArchiveDataSource(uid, pid, did);

        // Assert
        Assert.True(result);

        Context.ChangeTracker.Clear();

        // Verify it was actually archived in database
        var archivedDataSource = await Context.DataSources.FindAsync(did);
        Assert.NotNull(archivedDataSource);
        Assert.Equal(did, archivedDataSource.Id);
        Assert.Equal("Customer CRM Database", archivedDataSource.Name);
        Assert.Equal("Primary customer relationship management database", archivedDataSource.Description);
        Assert.Equal("CRM_DB", archivedDataSource.Abbreviation);
        Assert.Equal("SQL Server", archivedDataSource.Type);
        Assert.Equal("Server=crm-prod.company.com;Database=CustomerData;", archivedDataSource.BaseUri);
        Assert.Equal(pid, archivedDataSource.ProjectId);
        Assert.NotNull(archivedDataSource.Config);
        Assert.True(archivedDataSource.LastUpdatedAt >= now);
        Assert.Equal(uid, archivedDataSource.LastUpdatedBy);
        // Ensure that data source soft delete event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("archive", actualEvent.Operation);
        Assert.Equal("data_source", actualEvent.EntityType);
        Assert.Equal(did, actualEvent.EntityId);
    }

    [Fact]
    public async Task ArchiveDataSource_NonExistentDataSource_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.ArchiveDataSource(uid, pid, 999));

        Assert.Contains("Data Source with id 999 not found", exception.Message);

        // Ensure that data source soft delete event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task ArchiveDataSource_NonExistentProject_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.ArchiveDataSource(uid, 2, 1));

        Assert.Contains("Project with id 2 not found", exception.Message);

        // Ensure that data source soft delete event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task ArchiveDataSource_AlreadyArchivedDataSource_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.ArchiveDataSource(uid, pid, 3)); // DataSource 3 is already archived

        Assert.Contains("Data Source with id 3 not found", exception.Message);

        // Ensure that data source soft delete event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task ArchiveDataSource_ArchivedDataSourceNotReturnedInGetAll()
    {
        // Arrange
        var initialCount = (await _dataSourceBusiness.GetAllDataSources(pid, true)).Count();

        // Act
        await _dataSourceBusiness.ArchiveDataSource(uid, pid, did);
        var finalCount = (await _dataSourceBusiness.GetAllDataSources(pid, true)).Count();

        // Assert
        Assert.Equal(initialCount - 1, finalCount);

        // Ensure that data source soft delete event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("archive", actualEvent.Operation);
        Assert.Equal("data_source", actualEvent.EntityType);
        Assert.Equal(did, actualEvent.EntityId);
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public async Task DataSourceOperations_ConcurrentModification_HandlesCorrectly()
    {
        // This test simulates concurrent operations on the same data source
        // In a real scenario, you might want to test with actual concurrent tasks

        // Arrange
        var dto1 = new UpdateDataSourceRequestDto
        {
            Name = "Concurrent Update 1",
            Type = "Test1"
        };

        var dto2 = new UpdateDataSourceRequestDto
        {
            Name = "Concurrent Update 2",
            Type = "Test2"
        };

        // As noted above, DbContext is not thread-safe so there's not a great way to truly simulate concurrent operations
        // so for now we take the sequential approach

        // Act
        await _dataSourceBusiness.UpdateDataSource(uid, pid, did, dto1);
        await _dataSourceBusiness.UpdateDataSource(uid, pid, did, dto2);

        // Assert
        // Verify it was actually updated in database
        var updatedDataSource = await Context.DataSources.FindAsync(did);
        Assert.NotNull(updatedDataSource);
        Assert.Equal("Concurrent Update 2", updatedDataSource.Name);
    }

    [Fact]
    public async Task DataSourceOperations_LargeConfigJson_HandlesCorrectly()
    {
        // Arrange
        var largeConfig = new JsonObject();
        for (var i = 0; i < 100; i++)
        {
            largeConfig[$"property_{i}"] = $"value_{i}";
            largeConfig[$"nested_{i}"] = new JsonObject
            {
                ["sub_property"] = i,
                ["sub_array"] = new JsonArray { i, i + 1, i + 2 }
            };
        }

        var dto = new CreateDataSourceRequestDto
        {
            Name = "Large Config Test",
            Type = "Test",
            Config = largeConfig
        };

        // Act
        var result = await _dataSourceBusiness.CreateDataSource(uid, pid, dto);

        // Assert
        Assert.NotNull(result.Config);
        Assert.Equal("value_50", result.Config["property_50"]?.ToString());
        Assert.Equal(50, result.Config["nested_50"]?["sub_property"]?.GetValue<int>());
    }

    [Fact]
    public async Task DataSourceOperations_SpecialCharactersInFields_HandlesCorrectly()
    {
        // Arrange
        var dto = new CreateDataSourceRequestDto
        {
            Name = "Test with émojis 🚀 and ñ special chars",
            Description = "Description with quotes \"test\" and 'single quotes'",
            Abbreviation = "SPEC_CHAR",
            Type = "Test & Special",
            BaseUri = "https://test.com/path?param=value&other=123",
            Config = new JsonObject
            {
                ["special"] = "Value with quotes \"test\" and newlines\nand tabs\t",
                ["unicode"] = "Unicode: 中文, العربية, русский"
            }
        };

        // Act
        var result = await _dataSourceBusiness.CreateDataSource(uid, pid, dto);

        // Assert
        Assert.Equal("Test with émojis 🚀 and ñ special chars", result.Name);
        Assert.Contains("quotes \"test\"", result.Description);
        Assert.Equal("Test & Special", result.Type);
        Assert.Contains("Unicode: 中文", result.Config["unicode"]?.ToString());
    }

    #endregion

    #region DataSourceDTO Tests

    [Fact]
    public void DataSourceRequestDto_AllProperties_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        var config = new JsonObject { ["test"] = "value" };
        var dto = new CreateDataSourceRequestDto
        {
            Name = "Test Name",
            Description = "Test Description",
            Abbreviation = "TEST",
            Type = "Test Type",
            BaseUri = "http://test.com",
            Config = config
        };

        // Assert
        Assert.Equal("Test Name", dto.Name);
        Assert.Equal("Test Description", dto.Description);
        Assert.Equal("TEST", dto.Abbreviation);
        Assert.Equal("Test Type", dto.Type);
        Assert.Equal("http://test.com", dto.BaseUri);
        Assert.Equal(config, dto.Config);
    }

    [Fact]
    public void DataSourceResponseDto_AllProperties_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        var config = new JsonObject { ["test"] = "value" };
        var now = DateTime.UtcNow;

        var dto = new DataSourceResponseDto
        {
            Id = 1,
            Name = "Test Name",
            Description = "Test Description",
            Abbreviation = "TEST",
            Type = "Test Type",
            BaseUri = "http://test.com",
            Config = config,
            ProjectId = 1,
            LastUpdatedBy = uid,
            LastUpdatedAt = now,
            IsArchived = false
        };

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal("Test Name", dto.Name);
        Assert.Equal("Test Description", dto.Description);
        Assert.Equal("TEST", dto.Abbreviation);
        Assert.Equal("Test Type", dto.Type);
        Assert.Equal("http://test.com", dto.BaseUri);
        Assert.Equal(config, dto.Config);
        Assert.Equal(1, dto.ProjectId);
        Assert.Equal(uid, dto.LastUpdatedBy);
        Assert.False(dto.IsArchived);
    }

    #endregion

    #region UnarchiveDataSource Tests

    [Fact]
    public async Task UnarchiveDataSource_ValidArchivedDataSource_UnarchivesSuccessfully()
    {
        var now = DateTime.UtcNow;
        // Act
        var result = await _dataSourceBusiness.UnarchiveDataSource(uid, pid, did3);

        // Assert
        Assert.True(result);

        Context.ChangeTracker.Clear();
        var reloaded = await Context.DataSources.FindAsync(did3);
        Assert.NotNull(reloaded);
        Assert.False(reloaded.IsArchived);
        Assert.Equal(uid, reloaded.LastUpdatedBy);
        Assert.True(reloaded.LastUpdatedAt >= now);
        Assert.Equal("Customer CRM Database", reloaded.Name);
        Assert.Equal("Primary customer relationship management database", reloaded.Description);
        Assert.Equal("CRM_DB", reloaded.Abbreviation);
        Assert.Equal("SQL Server", reloaded.Type);
        Assert.Equal("Server=crm-prod.company.com;Database=CustomerData;", reloaded.BaseUri);
        Assert.NotNull(reloaded.Config);
        Assert.Equal(pid, reloaded.ProjectId);
        Assert.Equal(did3, reloaded.Id);
    }

    [Fact]
    public async Task UnarchiveDataSource_NonExistent_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.UnarchiveDataSource(uid, pid, 99999));

        Assert.Contains("Data Source with id 99999 not found", ex.Message);
        // Ensure that data source unarchive event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UnarchiveDataSource_WrongProject_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.UnarchiveDataSource(uid, pid2, did3)); // did3 is archived and belongs to pid

        Assert.Contains($"Data Source with id {did3} not found", ex.Message);
        // Ensure that data source unarchive event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UnarchiveDataSource_NotArchived_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dataSourceBusiness.UnarchiveDataSource(uid, pid, did)); // did is not archived

        Assert.Contains($"Data Source with id {did} not found", ex.Message);
        // Ensure that data source unarchive event was NOT logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion

    #region LastUpdatedBy Tests

    [Fact]
    public async Task CreateDataSource_Success_StoresLastUpdatedByUserId()
    {
        // Arrange
        var testDataSource = new DataSource
        {
            Name = $"Test DataSource with User {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Description = "Test Description with User ID",
            Type = "Test Type",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = uid,
            IsArchived = false,
            OrganizationId = oid
        };

        // Act
        Context.DataSources.Add(testDataSource);
        await Context.SaveChangesAsync();

        // Assert
        var savedDataSource = await Context.DataSources.FindAsync(testDataSource.Id);
        Assert.NotNull(savedDataSource);
        Assert.Equal(uid, savedDataSource.LastUpdatedBy);
    }

    [Fact]
    public async Task CreateDataSource_Success_NavigationPropertyLoadsUser()
    {
        // Arrange
        var testDataSource = new DataSource
        {
            Name = $"Test DataSource Navigation {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Description = "Test Navigation Property",
            Type = "Test Type",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = uid,
            IsArchived = false,
            OrganizationId = oid
        };

        Context.DataSources.Add(testDataSource);
        await Context.SaveChangesAsync();

        // Act
        var dataSourceWithUser = await Context.DataSources
            .Include(ds => ds.LastUpdatedByUser)
            .FirstAsync(ds => ds.Id == testDataSource.Id);

        // Assert
        Assert.NotNull(dataSourceWithUser.LastUpdatedByUser);
        Assert.Equal("John Smith", dataSourceWithUser.LastUpdatedByUser.Name);
        Assert.Equal("john.smith@company.com", dataSourceWithUser.LastUpdatedByUser.Email);
        Assert.Equal(uid, dataSourceWithUser.LastUpdatedBy);
    }

    [Fact]
    public async Task CreateDataSource_Success_WithNullLastUpdatedBy()
    {
        // Arrange
        var testDataSource = new DataSource
        {
            Name = $"Test DataSource Null User {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Description = "Test with null LastUpdatedBy",
            Type = "Test Type",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null,
            IsArchived = false,
            OrganizationId = oid
        };

        // Act
        Context.DataSources.Add(testDataSource);
        await Context.SaveChangesAsync();

        // Assert
        var savedDataSource = await Context.DataSources.FindAsync(testDataSource.Id);
        Assert.NotNull(savedDataSource);
        Assert.Null(savedDataSource.LastUpdatedBy);

        var dataSourceWithUser = await Context.DataSources
            .Include(ds => ds.LastUpdatedByUser)
            .FirstAsync(ds => ds.Id == testDataSource.Id);

        Assert.Null(dataSourceWithUser.LastUpdatedByUser);
    }

    [Fact]
    public async Task UpdateDataSource_Success_UpdatesLastUpdatedByUserId()
    {
        // Arrange
        var testDataSource = new DataSource
        {
            Name = $"Original DataSource {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Description = "Original Description",
            Type = "Original Type",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null,
            OrganizationId = oid
        };
        Context.DataSources.Add(testDataSource);
        await Context.SaveChangesAsync();

        // Act
        testDataSource.LastUpdatedBy = uid;
        testDataSource.Description = "Updated Description";
        testDataSource.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        Context.DataSources.Update(testDataSource);
        await Context.SaveChangesAsync();

        // Assert
        var updatedDataSource = await Context.DataSources
            .Include(ds => ds.LastUpdatedByUser)
            .FirstAsync(ds => ds.Id == testDataSource.Id);

        Assert.Equal(uid, updatedDataSource.LastUpdatedBy);
        Assert.NotNull(updatedDataSource.LastUpdatedByUser);
        Assert.Equal("John Smith", updatedDataSource.LastUpdatedByUser.Name);
        Assert.Equal("Updated Description", updatedDataSource.Description);
    }

    #endregion
}