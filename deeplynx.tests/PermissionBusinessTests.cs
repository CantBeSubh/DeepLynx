using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using deeplynx.helpers.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class PermissionBusinessTests : IntegrationTestBase
    {
        private Config _config;
        private EventBusiness _eventBusiness;
        private INotificationBusiness _notificationBusiness = null!;
        private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
        private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
        private PermissionBusiness _permissionBusiness;

        public long oid;        // organization ID
        public long pid;        // project ID
        public long lid;        // label IDs
        public long lid2;
        public long permid1;    // permission IDs
        public long permid2;
        public long permid3;
        public long permid4;
        public long permid5;
        public long permid6;
        public long permid7;
        public long permid8;
        public long uid;

        
        public PermissionBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _config = new Config();
            _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
            _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
            _notificationBusiness = new NotificationBusiness(_config, Context, _mockNotificationLogger.Object, _mockHubContext.Object);
            _eventBusiness = new EventBusiness(_config, Context, _cacheBusiness, _notificationBusiness);
            _permissionBusiness = new PermissionBusiness(Context, _eventBusiness, _cacheBusiness);
        }

        #region GetAllPermissions Tests

        [Fact]
        public async Task GetAllPermissions_IncludesIsDefault_WhenNoFilters()
        {
            // Arrange - get expected default permission count
            var expectedDefaultCount = await Context.Permissions
                .CountAsync(p => p.IsDefault);

            // Act
            var result = await _permissionBusiness.GetAllPermissions(null, null, null);
            var permissions = result.ToList();

            // Assert
            var actualDefaultCount = permissions.Count(p => p.IsDefault);
            Assert.Equal(expectedDefaultCount, actualDefaultCount);
            Assert.Contains(permissions, p => p.Id == permid1);
            Assert.Contains(permissions, p => p.Id == permid3);
            Assert.Contains(permissions, p => p.Id == permid5);
            Assert.Contains(permissions, p => p.Id == permid6);
            Assert.Contains(permissions, p => p.Id == permid7);
            Assert.Contains(permissions, p => p.Id == permid8);
        }

        [Fact]
        public async Task GetAllPermissions_FiltersOnLabelId()
        {
            // Arrange - get expected default permission count
            var expectedDefaultCount = await Context.Permissions
                .CountAsync(p => p.IsDefault);

            // Act
            var result = await _permissionBusiness.GetAllPermissions(lid, null, null);
            var permissions = result.ToList();

            // Assert
            Assert.Equal(expectedDefaultCount + 3, permissions.Count);
            Assert.All(permissions, p => Assert.True(p.LabelId == lid || p.IsDefault));
            Assert.Contains(permissions, p => p.Id == permid1);
            Assert.Contains(permissions, p => p.Id == permid3);
            Assert.Contains(permissions, p => p.Id == permid5);
            Assert.Contains(permissions, p => p.Id == permid8);
        }

        [Fact]
        public async Task GetAllPermissions_FiltersOnProjectId()
        {
            // Arrange - get expected default permission count
            var expectedDefaultCount = await Context.Permissions
                .CountAsync(p => p.IsDefault);

            // Act
            var result = await _permissionBusiness.GetAllPermissions(null, pid, null);
            var permissions = result.ToList();

            // Assert
            Assert.Equal(expectedDefaultCount + 2, permissions.Count);
            Assert.All(permissions, p => Assert.True(p.ProjectId == pid || p.IsDefault));
            Assert.Contains(permissions, p => p.Id == permid3);
            Assert.Contains(permissions, p => p.Id == permid6);
            Assert.Contains(permissions, p => p.Id == permid8);
        }

        [Fact]
        public async Task GetAllPermissions_FiltersOnOrganizationId()
        {
            // Arrange - get expected default permission count
            var expectedDefaultCount = await Context.Permissions
                .CountAsync(p => p.IsDefault);

            // Act
            var result = await _permissionBusiness.GetAllPermissions(null, null, oid);
            var permissions = result.ToList();

            // Assert
            Assert.Equal(expectedDefaultCount + 2, permissions.Count);
            Assert.All(permissions, p => Assert.True(p.OrganizationId == oid || p.IsDefault));
            Assert.Contains(permissions, p => p.Id == permid5);
            Assert.Contains(permissions, p => p.Id == permid7);
            Assert.Contains(permissions, p => p.Id == permid8);
        }

        [Fact]
        public async Task GetAllPermissions_IncludesDefault_WhenMultipleFilters()
        {
            // Arrange - get expected default permission count
            var expectedDefaultCount = await Context.Permissions
                .CountAsync(p => p.IsDefault);

            // Act - filter by label and project
            var result = await _permissionBusiness.GetAllPermissions(lid, pid, null);
            var permissions = result.ToList();

            // Assert - should still include default permission permissions even with filters
            Assert.Equal(expectedDefaultCount + 1, permissions.Count);
            Assert.All(permissions, p => Assert.True(p.ProjectId == pid || p.LabelId == lid || p.IsDefault));
            Assert.Contains(permissions, p => p.Id == permid3);
            Assert.Contains(permissions, p => p.Id == permid8);
        }

        [Fact]
        public async Task GetAllPermissions_ExcludesArchived()
        {
            // Act
            var result = await _permissionBusiness.GetAllPermissions(null, null, null, true);
            var permissions = result.ToList();

            // Assert
            Assert.All(permissions, p => Assert.False(p.IsArchived));
            Assert.DoesNotContain(permissions, p => p.Id == permid2); // archived permission
        }

        [Fact]
        public async Task GetAllPermissions_WithHideArchivedFalse_IncludesArchived()
        {
            // Act
            var result = await _permissionBusiness.GetAllPermissions(null, null, null, false);
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
            var result = await _permissionBusiness.GetPermission(permid1);

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
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.GetPermission(permid4)); // deleted permission

            Assert.Contains($"Permission with id {permid4} not found", exception.Message);
        }

        [Fact]
        public async Task GetPermission_Fails_IfArchivedPermission()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.GetPermission(permid2)); // archived permission

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

            // Act
            var result = await _permissionBusiness.CreatePermission(dto, pid, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("New Project Permission", result.Name);
            Assert.Equal(pid, result.ProjectId);
            Assert.Equal(lid, result.LabelId);
            Assert.False(result.IsDefault);

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

            // Act
            var result = await _permissionBusiness.CreatePermission(dto, null, oid);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("New Org Permission", result.Name);
            Assert.Equal(oid, result.OrganizationId);
            Assert.Equal(lid, result.LabelId);
            Assert.False(result.IsDefault);

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
            var result = await _permissionBusiness.CreatePermission(dto, pid, null);

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
        public async Task CreatePermission_Fails_IfBothProjectAndOrgAreSet()
        {
            // Arrange
            var dto = new CreatePermissionRequestDto
            {
                Name = "Dual Permission",
                Action = "test",
                LabelId = lid
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _permissionBusiness.CreatePermission(dto, pid, oid));
            Assert.Contains("Please provide only one of Project ID or Organization ID, not both", exception.Message);

            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreatePermission_Fails_IfNeitherProjectNorOrgAreSet()
        {
            // Arrange
            var dto = new CreatePermissionRequestDto
            {
                Name = "Orphaned Permission",
                Action = "test",
                LabelId = lid
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _permissionBusiness.CreatePermission(dto, null, null));
            Assert.Contains("One of Project ID or Organization ID must be provided", exception.Message);

            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
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
            await Assert.ThrowsAsync<ValidationException>(
                () => _permissionBusiness.CreatePermission(dto, pid, null));

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
            await Assert.ThrowsAsync<ValidationException>(
                () => _permissionBusiness.CreatePermission(dto, pid, null));

            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreatePermission_Fails_IfNoLabelId()
        {
            // Arrange
            var dto = new CreatePermissionRequestDto
            {
                Name = "No Label Permission",
                Action = "test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _permissionBusiness.CreatePermission(dto, pid, null));

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
                Action = "write"
            };

            // Act
            var result = await _permissionBusiness.UpdatePermission(permid1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(permid1, result.Id);
            Assert.Equal("Updated Permission", result.Name);
            Assert.Equal("Now with a description", result.Description);
            Assert.Equal("write", result.Action);

            // Verify it was actually saved to DB
            var savedPermission = await Context.Permissions.FindAsync(permid1);
            Assert.NotNull(savedPermission);
            Assert.Equal("Updated Permission", savedPermission.Name);
            Assert.Equal("Now with a description", savedPermission.Description);

            // Ensure that the Permission create event was logged
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
            var result = await _permissionBusiness.UpdatePermission(permid1, dto);

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
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.UpdatePermission(permid4, dto)); // deleted permission

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
            var result = await _permissionBusiness.UpdatePermission(permid1, dto);

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
            var result = await _permissionBusiness.UpdatePermission(permid1, dto);

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

            var isDefault = await Context.Permissions
                .Where(p => p.IsDefault)
                .FirstOrDefaultAsync();
            Assert.NotNull(isDefault);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.UpdatePermission(isDefault.Id, dto)); // default permission

            Assert.Contains($"Permission with id {isDefault.Id} cannot be updated", exception.Message);

            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        #endregion

        #region ArchivePermission Tests

        [Fact]
        public async Task ArchivePermission_Succeeds_IfNotArchived()
        {
            // Act
            var result = await _permissionBusiness.ArchivePermission(permid1);

            // Assert
            Assert.True(result);

            // Verify it was actually saved to DB
            var savedPermission = await Context.Permissions.FindAsync(permid1);
            Assert.NotNull(savedPermission);
            Assert.True(savedPermission.IsArchived);

            // Ensure that the Permission archive event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];

            Assert.Equal("archive", actualEvent.Operation);
            Assert.Equal("permission", actualEvent.EntityType);
            Assert.Equal(permid1, actualEvent.EntityId);
        }

        [Fact]
        public async Task ArchivePermission_Fails_IfArchived()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.ArchivePermission(permid2)); // already archived

            Assert.Contains($"Permission with id {permid2} not found or is already archived", exception.Message);

            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task ArchivePermission_Fails_IfDefault()
        {
            // Arrange
            var isDefault = await Context.Permissions
                .Where(p => p.IsDefault)
                .FirstOrDefaultAsync();
            Assert.NotNull(isDefault);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.ArchivePermission(isDefault.Id)); // default permission

            Assert.Contains($"Permission with id {isDefault.Id} cannot be updated", exception.Message);

            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        #endregion

        #region UnarchivePermission Tests

        [Fact]
        public async Task UnarchivePermission_Succeeds_IfArchived()
        {
            // Act
            var result = await _permissionBusiness.UnarchivePermission(permid2);

            // Assert
            Assert.True(result);

            // Verify it was actually saved to DB
            var savedPermission = await Context.Permissions.FindAsync(permid2);
            Assert.NotNull(savedPermission);
            Assert.False(savedPermission.IsArchived);

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
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.UnarchivePermission(permid1)); // not archived

            Assert.Contains($"Permission with id {permid1} not found or is not archived", exception.Message);

            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task UnarchivePermission_Fails_IfDefault()
        {
            // Arrange
            var isDefault = await Context.Permissions
                .Where(p => p.IsDefault)
                .FirstOrDefaultAsync();
            Assert.NotNull(isDefault);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.UnarchivePermission(isDefault.Id)); // default permission

            Assert.Contains($"Permission with id {isDefault.Id} cannot be updated", exception.Message);

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
            var result = await _permissionBusiness.DeletePermission(permid1);

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
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.DeletePermission(permid4)); // deleted permission

            Assert.Contains($"Permission with id {permid4} not found", exception.Message);

            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task DeletePermission_Fails_IfDefault()
        {
            // Arrange
            var isDefault = await Context.Permissions
                .Where(p => p.IsDefault)
                .FirstOrDefaultAsync();
            Assert.NotNull(isDefault);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _permissionBusiness.DeletePermission(isDefault.Id)); // default permission

            Assert.Contains($"Permission with id {isDefault.Id} cannot be deleted", exception.Message);

            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        #endregion
        
        #region LastUpdatedBy Tests

        [Fact]
        public async Task CreateObjectStorage_Success_StoresLastUpdatedByUserId()
        {
            // Arrange
            var config = new JsonObject();
            config["mountPath"] = "./test/storage/";
            var testObjectStorage = new ObjectStorage
            {
                Name = "Test Object Storage",
                ProjectId = pid,
                Type = "filesystem",
                Config = config.ToString(),
                Default = false,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid
            };
            
            // Act
            Context.ObjectStorages.Add(testObjectStorage);
            await Context.SaveChangesAsync();

            // Assert
            var savedObjectStorage = await Context.ObjectStorages.FindAsync(testObjectStorage.Id);
            Assert.NotNull(savedObjectStorage);
            Assert.Equal(uid, savedObjectStorage.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateObjectStorage_Success_NavigationPropertyLoadsUser()
        {
            // Arrange
            var config = new JsonObject();
            config["mountPath"] = "./test/storage2/";
            var testObjectStorage = new ObjectStorage
            {
                Name = "Test Object Storage 2",
                ProjectId = pid,
                Type = "filesystem",
                Config = config.ToString(),
                Default = false,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid
            };
            
            Context.ObjectStorages.Add(testObjectStorage);
            await Context.SaveChangesAsync();

            // Act
            var objectStorageWithUser = await Context.ObjectStorages
                .Include(os => os.LastUpdatedByUser)
                .FirstAsync(os => os.Id == testObjectStorage.Id);
            
            // Assert
            Assert.NotNull(objectStorageWithUser.LastUpdatedByUser);
            Assert.Equal("Test User", objectStorageWithUser.LastUpdatedByUser.Name);
            Assert.Equal("test_user@example.com", objectStorageWithUser.LastUpdatedByUser.Email);
            Assert.Equal(uid, objectStorageWithUser.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateObjectStorage_Success_WithNullLastUpdatedBy()
        {
            // Arrange
            var config = new JsonObject();
            config["mountPath"] = "./test/storage3/";
            var testObjectStorage = new ObjectStorage
            {
                Name = "Test Object Storage 3",
                ProjectId = pid,
                Type = "filesystem",
                Config = config.ToString(),
                Default = false,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            // Act
            Context.ObjectStorages.Add(testObjectStorage);
            await Context.SaveChangesAsync();

            // Assert
            var savedObjectStorage = await Context.ObjectStorages.FindAsync(testObjectStorage.Id);
            Assert.NotNull(savedObjectStorage);
            Assert.Null(savedObjectStorage.LastUpdatedBy);
            
            var objectStorageWithUser = await Context.ObjectStorages
                .Include(os => os.LastUpdatedByUser)
                .FirstAsync(os => os.Id == testObjectStorage.Id);
            
            Assert.Null(objectStorageWithUser.LastUpdatedByUser);
        }

        [Fact]
        public async Task UpdateObjectStorage_Success_UpdatesLastUpdatedByUserId()
        {
            // Arrange
            var config = new JsonObject();
            config["mountPath"] = "./test/storage4/";
            var testObjectStorage = new ObjectStorage
            {
                Name = "Test Object Storage 4",
                ProjectId = pid,
                Type = "filesystem",
                Config = config.ToString(),
                Default = false,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.ObjectStorages.Add(testObjectStorage);
            await Context.SaveChangesAsync();

            // Act
            testObjectStorage.LastUpdatedBy = uid;
            testObjectStorage.Name = "Updated Name";
            testObjectStorage.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            
            Context.ObjectStorages.Update(testObjectStorage);
            await Context.SaveChangesAsync();

            // Assert
            var updatedObjectStorage = await Context.ObjectStorages
                .Include(os => os.LastUpdatedByUser)
                .FirstAsync(os => os.Id == testObjectStorage.Id);
            
            Assert.Equal(uid, updatedObjectStorage.LastUpdatedBy);
            Assert.NotNull(updatedObjectStorage.LastUpdatedByUser);
            Assert.Equal("Test User", updatedObjectStorage.LastUpdatedByUser.Name);
            Assert.Equal("Updated Name", updatedObjectStorage.Name);
        }

        #endregion

        protected override async Task SeedTestDataAsync()
        {
            // await CleanupTestData();
            await base.SeedTestDataAsync();
            
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
            var project = new Project { Name = "Test Project" };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;

            // create test label (sensitivity label)
            var label = new SensitivityLabel { Name = "Test Label" };
            var label2 = new SensitivityLabel { Name = "Other Label" };
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
                IsDefault = false
            };
            var permission2 = new Permission
            {
                Name = "Archived Permission",
                Action = "write",
                LabelId = lid,
                IsDefault = false,
                IsArchived = true
            };
            var permission3 = new Permission
            {
                Name = "Permission with Project",
                Action = "execute",
                LabelId = lid,
                ProjectId = pid,
                IsDefault = false
            };
            var permission4 = new Permission
            {
                Name = "Deleted Permission",
                Action = "delete",
                LabelId = lid,
                IsDefault = false
            };
            var permission5 = new Permission
            {
                Name = "Permission with Organization",
                Action = "manage",
                LabelId = lid,
                OrganizationId = oid,
                IsDefault = false
            };
            var permission6 = new Permission
            {
                Name = "Second Permission Same Project",
                Action = "sing",
                LabelId = lid2,
                ProjectId = pid,
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

        private async Task CleanupTestData()
        {
            // Remove all permissions (or just test-specific ones)
            var existingPerms = await Context.Permissions.ToListAsync();
            Context.Permissions.RemoveRange(existingPerms);
            await Context.SaveChangesAsync();
        }
    }
}
