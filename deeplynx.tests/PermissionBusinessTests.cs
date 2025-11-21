using System.ComponentModel.DataAnnotations;
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

[Collection("Test Suite Collection")]
public class PermissionBusinessTests : IntegrationTestBase
{
    private EventBusiness _eventBusiness;
    private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
    private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
    private INotificationBusiness _notificationBusiness = null!;
    private PermissionBusiness _permissionBusiness;
    public long lid; // label IDs
    public long lid2;

    public long oid; // organization ID
    public long permid1; // permission IDs
    public long permid2;
    public long permid3;
    public long permid4;
    public long permid5;
    public long permid6;
    public long permid7;
    public long permid8;
    public long pid; // project ID
    public long uid;


    public PermissionBusinessTests(TestSuiteFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
        _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
        _notificationBusiness =
            new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
        _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
        _permissionBusiness = new PermissionBusiness(Context, _eventBusiness, _cacheBusiness);
    }

    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();

        // Create test user
        var user = new User
        {
            Name = "Test User",
            Email = "test_user@example.com",
            Password = "test_password",
            IsArchived = false
        };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        uid = user.Id;

        // create test organization
        var organization = new Organization { Name = "Test Org" };
        Context.Organizations.Add(organization);
        await Context.SaveChangesAsync();
        oid = organization.Id;

        // create test project
        var project = new Project { Name = "Test Project", OrganizationId = oid };
        Context.Projects.Add(project);
        await Context.SaveChangesAsync();
        pid = project.Id;

        // create test label (sensitivity label)
        var label = new SensitivityLabel { Name = "Test Label", OrganizationId = oid };
        var label2 = new SensitivityLabel { Name = "Other Label", OrganizationId = oid };
        Context.SensitivityLabels.AddRange(label, label2);
        await Context.SaveChangesAsync();
        lid = label.Id;
        lid2 = label2.Id;

        // create test permissions
        var permission1 = new Permission
        {
            Name = "Basic Permission",
            Action = "read",
            LabelId = lid,
            OrganizationId = oid,
            IsDefault = false
        };
        var permission2 = new Permission
        {
            Name = "Archived Permission",
            Action = "write",
            LabelId = lid,
            OrganizationId = oid,
            IsDefault = false,
            IsArchived = true
        };
        var permission3 = new Permission
        {
            Name = "Permission with Project",
            Action = "execute",
            LabelId = lid,
            ProjectId = pid,
            OrganizationId = oid,
            IsDefault = false
        };
        var permission4 = new Permission
        {
            Name = "Deleted Permission",
            Action = "delete",
            LabelId = lid,
            OrganizationId = oid,
            IsDefault = false
        };
        var permission5 = new Permission
        {
            Name = "Permission with Organization",
            Action = "manage",
            LabelId = lid,
            OrganizationId = oid,
            IsDefault = false, ProjectId = pid
        };
        var permission6 = new Permission
        {
            Name = "Second Permission Same Project",
            Action = "sing",
            LabelId = lid2,
            ProjectId = pid,
            OrganizationId = oid,
            IsDefault = false
        };
        var permission7 = new Permission
        {
            Name = "Second Permission Same Organization",
            Action = "dance",
            LabelId = lid2,
            OrganizationId = oid,
            IsDefault = false
        };
        var permission8 = new Permission
        {
            Name = "Default Permission with Project",
            Action = "write",
            LabelId = lid,
            ProjectId = pid,
            OrganizationId = oid,
            IsDefault = true
        };

        Context.Permissions.AddRange(
            permission1, permission2, permission3, permission4,
            permission5, permission6, permission7, permission8);
        await Context.SaveChangesAsync();
        permid1 = permission1.Id;
        permid2 = permission2.Id;
        permid3 = permission3.Id;
        permid4 = permission4.Id;
        permid5 = permission5.Id;
        permid6 = permission6.Id;
        permid7 = permission7.Id;
        permid8 = permission8.Id;

        // delete permission 4 to test "not found" scenarios
        Context.Permissions.Remove(permission4);
        await Context.SaveChangesAsync();
    }

    #region GetAllPermissions Tests

    [Fact]
    public async Task GetAllPermissions_ReturnsAll_WhenNoFilters()
    {
        // Act
        var result = await _permissionBusiness.GetAllPermissions(null, null, oid);
        var permissions = result.ToList();

        // Assert - should return all non-archived permissions for this organization
        Assert.Equal(6, permissions.Count); // 6 non-archived permissions with oid
        Assert.Contains(permissions, p => p.Id == permid1);
        Assert.Contains(permissions, p => p.Id == permid3);
        Assert.Contains(permissions, p => p.Id == permid5);
        Assert.Contains(permissions, p => p.Id == permid6);
        Assert.Contains(permissions, p => p.Id == permid7);
        Assert.Contains(permissions, p => p.Id == permid8);
        Assert.DoesNotContain(permissions, p => p.Id == permid2); // archived
    }

    [Fact]
    public async Task GetAllPermissions_FiltersOnLabelId()
    {
        // Act
        var result = await _permissionBusiness.GetAllPermissions(lid, null, oid);
        var permissions = result.ToList();

        // Assert - should return only permissions with lid and organizationId = oid
        Assert.Equal(4, permissions.Count);
        Assert.All(permissions, p => Assert.Equal(lid, p.LabelId));
        Assert.All(permissions, p => Assert.Equal(oid, p.OrganizationId));
        Assert.Contains(permissions, p => p.Id == permid1);
        Assert.Contains(permissions, p => p.Id == permid3);
        Assert.Contains(permissions, p => p.Id == permid5);
        Assert.Contains(permissions, p => p.Id == permid8);
    }

    [Fact]
    public async Task GetAllPermissions_FiltersOnProjectId()
    {
        // Act
        var result = await _permissionBusiness.GetAllPermissions(null, pid, oid);
        var permissions = result.ToList();

        // Assert - should return only permissions with pid and organizationId = oid
        Assert.Equal(3, permissions.Count);
        Assert.All(permissions, p => Assert.Equal(pid, p.ProjectId));
        Assert.All(permissions, p => Assert.Equal(oid, p.OrganizationId));
        Assert.Contains(permissions, p => p.Id == permid3);
        Assert.Contains(permissions, p => p.Id == permid6);
        Assert.Contains(permissions, p => p.Id == permid8);
        // Note: permid8 is default but still matches the filter criteria
    }

    [Fact]
    public async Task GetAllPermissions_FiltersOnOrganizationId()
    {
        // Act
        var result = await _permissionBusiness.GetAllPermissions(null, null, oid);
        var permissions = result.ToList();

        // Assert - should return all non-archived permissions for this organization
        Assert.Equal(6, permissions.Count);
        Assert.All(permissions, p => Assert.Equal(oid, p.OrganizationId));
        Assert.Contains(permissions, p => p.Id == permid1);
        Assert.Contains(permissions, p => p.Id == permid3);
        Assert.Contains(permissions, p => p.Id == permid5);
        Assert.Contains(permissions, p => p.Id == permid6);
        Assert.Contains(permissions, p => p.Id == permid7);
        Assert.Contains(permissions, p => p.Id == permid8);
    }

    [Fact]
    public async Task GetAllPermissions_FiltersOnMultiple()
    {
        // Act - filter by label and project
        var result = await _permissionBusiness.GetAllPermissions(lid, pid, oid);
        var permissions = result.ToList();

        // Assert - should return permissions matching all criteria
        Assert.Equal(2, permissions.Count);
        Assert.All(permissions, p => Assert.Equal(lid, p.LabelId));
        Assert.All(permissions, p => Assert.Equal(pid, p.ProjectId));
        Assert.All(permissions, p => Assert.Equal(oid, p.OrganizationId));
        Assert.Contains(permissions, p => p.Id == permid3);
        Assert.Contains(permissions, p => p.Id == permid8);
    }

    [Fact]
    public async Task GetAllPermissions_ExcludesArchived()
    {
        // Act
        var result = await _permissionBusiness.GetAllPermissions(null, null, oid);
        var permissions = result.ToList();

        // Assert
        Assert.All(permissions, p => Assert.False(p.IsArchived));
        Assert.DoesNotContain(permissions, p => p.Id == permid2); // archived permission
    }

    [Fact]
    public async Task GetAllPermissions_WithHideArchivedFalse_IncludesArchived()
    {
        // Act
        var result = await _permissionBusiness.GetAllPermissions(null, null, oid, false);
        var permissions = result.ToList();

        // Assert
        Assert.Contains(permissions, p => p.IsArchived);
        Assert.Contains(permissions, p => p.Id == permid2); // archived permission
    }

    #endregion

    #region GetPermission Tests

    [Fact]
    public async Task GetPermission_Succeeds_WhenExists()
    {
        // Act
        var result = await _permissionBusiness.GetPermission(oid, pid, permid1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(permid1, result.Id);
        Assert.Equal("Basic Permission", result.Name);
        Assert.False(result.IsArchived);
    }

    [Fact]
    public async Task GetPermission_Fails_IfNotFound()
    {
        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.GetPermission(oid, pid, permid4)); // deleted permission

        Assert.Contains($"Permission with id {permid4} not found", exception.Message);
    }

    [Fact]
    public async Task GetPermission_Fails_IfArchivedPermission()
    {
        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.GetPermission(oid, pid, permid2)); // archived permission

        Assert.Contains($"Permission with id {permid2} is archived", exception.Message);
    }

    #endregion

    #region CreatePermission Tests

    [Fact]
    public async Task CreatePermission_Succeeds_WithProjectSupplied()
    {
        // Arrange
        var dto = new CreatePermissionRequestDto
        {
            Name = "New Project Permission",
            Description = "A test permission for projects",
            Action = "test",
            LabelId = lid
        };

        var now = DateTime.UtcNow;

        // Act
        var result = await _permissionBusiness.CreatePermission(uid, dto, pid, oid);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Action, result.Action);
        Assert.Null(result.Resource);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(oid, result.OrganizationId);
        Assert.Equal(pid, result.ProjectId);
        Assert.Equal(lid, result.LabelId);
        Assert.False(result.IsDefault);
        Assert.True(result.LastUpdatedAt >= now);
        Assert.Equal(uid, result.LastUpdatedBy);

        // Verify it was actually saved to DB
        var savedPermission = await Context.Permissions.FindAsync(result.Id);
        Assert.NotNull(savedPermission);
        Assert.Equal("New Project Permission", savedPermission.Name);

        // Ensure that the Permission create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("create", actualEvent.Operation);
        Assert.Equal("permission", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task CreatePermission_Succeeds_WithOrganizationSupplied()
    {
        // Arrange
        var dto = new CreatePermissionRequestDto
        {
            Name = "New Org Permission",
            Description = "A test permission for organizations",
            Action = "test",
            LabelId = lid
        };

        var now = DateTime.UtcNow;

        // Act
        var result = await _permissionBusiness.CreatePermission(uid, dto, null, oid);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Action, result.Action);
        Assert.Null(result.Resource);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(oid, result.OrganizationId);
        Assert.Null(result.ProjectId);
        Assert.Equal(lid, result.LabelId);
        Assert.False(result.IsDefault);
        Assert.True(result.LastUpdatedAt >= now);
        Assert.Equal(uid, result.LastUpdatedBy);

        // Verify it was actually saved to DB
        var savedPermission = await Context.Permissions.FindAsync(result.Id);
        Assert.NotNull(savedPermission);
        Assert.Equal("New Org Permission", savedPermission.Name);

        // Ensure that the Permission create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal("create", actualEvent.Operation);
        Assert.Equal("permission", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task CreatePermission_Success_CreatesEvent()
    {
        // Arrange
        var dto = new CreatePermissionRequestDto
        {
            Name = "Event Permission",
            Description = "A test permission for event logging",
            Action = "test",
            LabelId = lid
        };

        // Act
        var result = await _permissionBusiness.CreatePermission(uid, dto, pid, oid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Event Permission", result.Name);

        // Ensure that the Permission create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal("create", actualEvent.Operation);
        Assert.Equal("permission", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task CreatePermission_Fails_IfNoName()
    {
        // Arrange
        var dto = new CreatePermissionRequestDto
        {
            Action = "test",
            LabelId = lid
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _permissionBusiness.CreatePermission(uid, dto, pid, oid));

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task CreatePermission_Fails_IfNoAction()
    {
        // Arrange
        var dto = new CreatePermissionRequestDto
        {
            Name = "No Action Permission",
            LabelId = lid
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _permissionBusiness.CreatePermission(uid, dto, pid, oid));

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion

    #region UpdatePermission Tests

    [Fact]
    public async Task UpdatePermission_Success_ReturnsPermission()
    {
        // Arrange
        var dto = new UpdatePermissionRequestDto
        {
            Name = "Updated Permission",
            Description = "Now with a description",
            Action = "test action"
        };

        var now = DateTime.UtcNow;

        // Act
        var result = await _permissionBusiness.UpdatePermission(oid, pid, uid, permid1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(permid1, result.Id);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.Action, result.Action);
        Assert.Null(result.Resource);
        Assert.Equal(oid, result.OrganizationId);
        Assert.Null(result.ProjectId);
        Assert.Equal(lid, result.LabelId);
        Assert.False(result.IsDefault);
        Assert.True(result.LastUpdatedAt >= now);
        Assert.Equal(uid, result.LastUpdatedBy);

        // Verify it was actually saved to DB
        var savedPermission = await Context.Permissions.FindAsync(permid1);
        Assert.NotNull(savedPermission);
        Assert.Equal("Updated Permission", savedPermission.Name);
        Assert.Equal("Now with a description", savedPermission.Description);

        // Ensure that the Permission update event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal("update", actualEvent.Operation);
        Assert.Equal("permission", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task UpdatePermission_Success_CreatesEvent()
    {
        // Arrange
        var dto = new UpdatePermissionRequestDto
        {
            Name = "Event Updated Permission"
        };

        // Act
        var result = await _permissionBusiness.UpdatePermission(oid, pid, uid, permid1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Event Updated Permission", result.Name);

        // Ensure that the Permission update event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal("update", actualEvent.Operation);
        Assert.Equal("permission", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task UpdatePermission_Fails_IfNotFound()
    {
        // Arrange
        var dto = new UpdatePermissionRequestDto
        {
            Name = "Updated Permission"
        };

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.UpdatePermission(pid, oid, uid, permid4, dto)); // deleted permission

        Assert.Contains($"Permission with id {permid4} not found", exception.Message);

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UpdatePermission_DoesNot_SetResource()
    {
        // Arrange
        var dto = new UpdatePermissionRequestDto
        {
            Name = "Resource Update Test"
        };

        // Act
        var result = await _permissionBusiness.UpdatePermission(oid, pid, uid, permid1, dto);

        // Assert - Resource should not be modifiable through update
        Assert.Null(result.Resource);

        // Verify in DB
        var savedPermission = await Context.Permissions.FindAsync(permid1);
        Assert.Null(savedPermission.Resource);

        // Ensure that the Permission update event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal("update", actualEvent.Operation);
        Assert.Equal("permission", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task UpdatePermission_DoesNot_SetIsDefault()
    {
        // Arrange
        var dto = new UpdatePermissionRequestDto
        {
            Name = "Default Permission Update Test"
        };

        // Act
        var result = await _permissionBusiness.UpdatePermission(oid, pid, uid, permid1, dto);

        // Assert - IsDefault should remain false for user permissions
        Assert.False(result.IsDefault);

        // Verify in DB
        var savedPermission = await Context.Permissions.FindAsync(permid1);
        Assert.False(savedPermission.IsDefault);

        // Ensure that the Permission update event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal("update", actualEvent.Operation);
        Assert.Equal("permission", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task UpdatePermission_Fails_IfDefault()
    {
        // Arrange
        var dto = new UpdatePermissionRequestDto
        {
            Name = "Cannot Update Default"
        };

        // Act & Assert - permid8 is default
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.UpdatePermission(pid, oid, uid, permid8, dto));

        Assert.Contains($"Permission with id {permid8} cannot be updated", exception.Message);

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion

    #region ArchivePermission Tests

    [Fact]
    public async Task ArchivePermission_Succeeds_IfNotArchived()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var result = await _permissionBusiness.ArchivePermission(oid, pid, uid, permid5);

        // Assert
        Assert.True(result);

        // Verify it was actually saved to DB
        var savedPermission = await Context.Permissions.FindAsync(permid5);
        Assert.NotNull(savedPermission);
        Assert.True(savedPermission.IsArchived);
        
    }

    [Fact]
    public async Task ArchivePermission_Fails_IfArchived()
    {
        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.ArchivePermission(oid, pid, uid, permid2)); // already archived

        Assert.Contains($"Permission with id {permid2} not found or is already archived", exception.Message);

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task ArchivePermission_Fails_IfDefault()
    {
        // Act & Assert - permid8 is default
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.ArchivePermission(oid, pid, uid, permid8));

        Assert.Contains($"Permission with id {permid8} cannot be updated", exception.Message);

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion

    #region UnarchivePermission Tests

    [Fact]
    public async Task UnarchivePermission_Succeeds_IfArchived()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var result = await _permissionBusiness.UnarchivePermission(oid, pid, uid, permid2);

        // Assert
        Assert.True(result);

        // Verify it was actually saved to DB
        var savedPermission = await Context.Permissions.FindAsync(permid2);
        Assert.NotNull(savedPermission);
        Assert.False(savedPermission.IsArchived);

        //Verify other fields were unchanged
        Assert.True(savedPermission.Id > 0);
        Assert.Equal("Archived Permission", savedPermission.Name);
        Assert.Null(savedPermission.Description);
        Assert.Equal("write", savedPermission.Action);
        Assert.Null(savedPermission.Resource);
        Assert.Equal(oid, savedPermission.OrganizationId);
        Assert.Null(savedPermission.ProjectId);
        Assert.Equal(lid, savedPermission.LabelId);
        Assert.False(savedPermission.IsDefault);
        Assert.True(savedPermission.LastUpdatedAt >= now);
        Assert.Equal(uid, savedPermission.LastUpdatedBy);

        // Ensure that the Permission unarchive event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal("unarchive", actualEvent.Operation);
        Assert.Equal("permission", actualEvent.EntityType);
        Assert.Equal(permid2, actualEvent.EntityId);
    }

    [Fact]
    public async Task UnarchivePermission_Fails_IfNotArchived()
    {
        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.UnarchivePermission(oid, pid, uid, permid1)); // not archived

        Assert.Contains($"Permission with id {permid1} not found or is not archived", exception.Message);

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UnarchivePermission_Fails_IfDefault()
    {
        // Act & Assert - permid8 is default
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.UnarchivePermission(pid, oid, uid, permid8));

        Assert.Contains($"Permission with id {permid8} cannot be updated", exception.Message);

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion

    #region DeletePermission Tests

    [Fact]
    public async Task DeletePermission_Succeeds_WhenExists()
    {
        // Act
        var result = await _permissionBusiness.DeletePermission(oid, pid, uid, permid1);

        // Assert
        Assert.True(result);

        // Verify it was actually deleted from DB
        var deletedPermission = await Context.Permissions.FindAsync(permid1);
        Assert.Null(deletedPermission);

        // Ensure that the Permission delete event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];

        Assert.Equal("delete", actualEvent.Operation);
        Assert.Equal("permission", actualEvent.EntityType);
        Assert.Equal(permid1, actualEvent.EntityId);
    }

    [Fact]
    public async Task DeletePermission_Fails_IfNotFound()
    {
        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.DeletePermission(oid, pid, uid, permid4)); // deleted permission

        Assert.Contains($"Permission with id {permid4} not found", exception.Message);

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task DeletePermission_Fails_IfDefault()
    {
        // Act & Assert - permid8 is default
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _permissionBusiness.DeletePermission(oid, pid, uid, permid8));

        Assert.Contains($"Permission with id {permid8} cannot be deleted", exception.Message);

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion
    
    #region UniqueConstraintTests

    [Fact]
    public async Task AddPermission_Fails_WhenDuplicateLabelActionInProject()
    {
        var permission9 = new Permission
        {
            Name = "Duplicate Permission with Project",
            Action = "write",
            LabelId = lid,
            ProjectId = pid,
            OrganizationId = oid,
            IsDefault = true
        };
        Context.Permissions.Add(permission9);
        await Assert.ThrowsAsync<DbUpdateException>(() => Context.SaveChangesAsync());
    }

    [Fact]
    public async Task AddPermission_Fails_WhenDuplicateLabelActionInOrganization()
    {
        var permission1 = new Permission
        {
            Name = "Default Permission 1",
            Action = "read",
            LabelId = lid,
            OrganizationId = oid,
            IsDefault = true
        };

        var permission2 = new Permission
        {
            Name = "Default Permission 2",
            Action = "read",
            LabelId = lid,
            OrganizationId = oid,
            IsDefault = true
        };
        Context.Permissions.Add(permission1);
        Context.Permissions.Add(permission2);
        await Assert.ThrowsAsync<DbUpdateException>(() => Context.SaveChangesAsync());
    }

    [Fact]
    public async Task AddPermission_Fails_WhenDuplicateResourceActionInOrganization()
    {
        var permission1 = new Permission
        {
            Name = "Default Permission with Resource 1",
            Action = "write",
            Resource = "project",
            OrganizationId = oid,
            IsDefault = true
        };

        var permission2 = new Permission
        {
            Name = "Default Permission with Resource 2",
            Action = "write",
            Resource = "project",
            OrganizationId = oid,
            IsDefault = true
        };
        Context.Permissions.Add(permission1);
        Context.Permissions.Add(permission2);
        await Assert.ThrowsAsync<DbUpdateException>(() => Context.SaveChangesAsync());
    }

    [Fact]
    public async Task AddPermission_Fails_WhenDuplicateResourceActionInProject()
    {
        var permission1 = new Permission
        {
            Name = "Default Permission with Resource in Project 1",
            Action = "write",
            Resource = "test duplicate",
            ProjectId = pid,
            OrganizationId = oid,
            IsDefault = true
        };

        var permission2 = new Permission
        {
            Name = "Default Permission with Resource in Project 2",
            Action = "write",
            Resource = "test duplicate",
            ProjectId = pid,
            OrganizationId = oid,
            IsDefault = true
        };
        Context.Permissions.Add(permission1);
        Context.Permissions.Add(permission2);
        await Assert.ThrowsAsync<DbUpdateException>(() => Context.SaveChangesAsync());
    }

    #endregion
}