using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class RoleBusinessTests : IntegrationTestBase
    {
        private EventBusiness _eventBusiness;
        private INotificationBusiness _notificationBusiness = null!;
        private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
        private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
        private RoleBusiness _roleBusiness;

        public long oid;        // organization ID
        public long pid;        // project ID
        public long rid1;       // role IDs
        public long rid2;
        public long rid3;
        public long rid4;
        public long rid5;
        public long uid;        // user ID
        public long mid;        // project member ID
        public long permid1;    // permission IDs
        public long permid2;
        public long permid3;
        public long permid4;
        
        public RoleBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
            _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
            _notificationBusiness = new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
            _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
            _roleBusiness = new RoleBusiness(Context, _cacheBusiness, _eventBusiness);
        }
        
        #region CreateRole Tests
        
        [Fact]
        public async Task CreateRole_Succeeds_WithProjectSupplied()
        {
            // Arrange
            var dto = new CreateRoleRequestDto
            {
                Name = "Project Role"
            };
            
            // Act
            var result = await _roleBusiness.CreateRole(uid, dto, pid, null);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Project Role", result.Name);
            Assert.Equal(pid, result.ProjectId);
            
            // Verify it was actually saved to DB
            var savedRole = await Context.Roles.FindAsync(result.Id);
            Assert.NotNull(savedRole);
            Assert.Equal("Project Role", savedRole.Name);
            
            // Ensure that the Role create event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("role", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
        }
        
        [Fact]
        public async Task CreateRole_Succeeds_WithOrganizationSupplied()
        {
            // Arrange
            var dto = new CreateRoleRequestDto
            {
                Name = "Org Role"
            };
            
            // Act
            var result = await _roleBusiness.CreateRole(uid, dto, null, oid);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Org Role", result.Name);
            Assert.Equal(oid, result.OrganizationId);
            
            // Verify it was actually saved to DB
            var savedRole = await Context.Roles.FindAsync(result.Id);
            Assert.NotNull(savedRole);
            Assert.Equal("Org Role", savedRole.Name);
            
            // Ensure that the Role create event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("role", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
        }

        [Fact]
        public async Task CreateRole_Success_CreatesEvent()
        {
            // Arrange
            var dto = new CreateRoleRequestDto
            {
                Name = "Event Role"
            };
            
            // Act
            var result = await _roleBusiness.CreateRole(uid, dto, pid, null);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Event Role", result.Name);
            
            // Ensure that the Role create event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("role", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
        }
        
        [Fact]
        public async Task CreateRole_Fails_IfNoName()
        {
            // Arrange
            var dto = new CreateRoleRequestDto
            {
                ProjectId = pid
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _roleBusiness.CreateRole(uid, dto, pid, null));
            
            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        [Fact]
        public async Task CreateRole_Fails_IfNullDto()
        {
            // Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _roleBusiness.CreateRole(uid, null, pid, null));
            
            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        [Fact]
        public async Task CreateRole_Fails_IfBothProjectAndOrgAreSet()
        {
            // Arrange
            var dto = new CreateRoleRequestDto
            {
                Name = "Dual Role"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _roleBusiness.CreateRole(uid, dto, pid, oid));

            Assert.Contains("Please provide only one of Project ID or Organization ID, not both", exception.Message);
            
            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        [Fact]
        public async Task CreateRole_Fails_IfNeitherProjectNorOrgAreSet()
        {
            // Arrange
            var dto = new CreateRoleRequestDto
            {
                Name = "Orphaned Role"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _roleBusiness.CreateRole(uid, dto, null, null));

            Assert.Contains("One of Project ID or Organization ID must be provided", exception.Message);
            
            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        #endregion
        
        #region BulkCreateRoles Tests
        
        [Fact]
        public async Task CreateRoles_Success_OnBulkCreate()
        {
            // Arrange
            var bulkDto = new List<CreateRoleRequestDto>
            {
                new()
                {
                    Name = $"Test Role 1",
                    Description = "Test Description 1"
                },
                new()
                {
                    Name = $"Test Role 2",
                    Description = "Test Description 2"
                }
            };

            // Act
            var result = await _roleBusiness.BulkCreateRoles(uid, pid, bulkDto);
    
            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Test Role 1", result.First().Name);
            Assert.Equal("Test Description 1", result.First().Description);
            Assert.Equal("Test Role 2", result.Last().Name);
            Assert.Equal("Test Description 2", result.Last().Description);

            // Ensure the create event is logged for each role create
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(2, eventList.Count);

            var firstEvent = eventList[0];
            Assert.Equal(pid, firstEvent.ProjectId);
            Assert.Equal("create", firstEvent.Operation);
            Assert.Equal("role", firstEvent.EntityType);
            Assert.Equal(result[0].Id, firstEvent.EntityId);

            var secondEvent = eventList[1];
            Assert.Equal(pid, secondEvent.ProjectId);
            Assert.Equal("create", secondEvent.Operation);
            Assert.Equal("role", secondEvent.EntityType);
            Assert.Equal(result[1].Id, secondEvent.EntityId);
        }
        
        [Fact]
        public async Task BulkCreateRoles_Success_OnNameCollision_UpdatesDescription()
        {
            // Arrange
            var existingRole = new CreateRoleRequestDto
            {
                Name = "Existing Role",
                Description = "Original Description"
            };
    
            var created = await _roleBusiness.BulkCreateRoles(uid, pid, [existingRole]);
            var existingRoleId = created.First().Id;
    
            var bulkDto = new CreateRoleRequestDto
            {
                Name = "Existing Role",
                Description = "Updated Description"
                
            };

            // Act
            var result = await _roleBusiness.BulkCreateRoles(uid, pid, [bulkDto]);
    
            Assert.Single(result);
            Assert.Equal(existingRoleId, result.First().Id); // Same ID as existing role
            Assert.Equal("Existing Role", result.First().Name);
            Assert.Equal("Updated Description", result.First().Description);

            // Ensure the both create events are logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(2, eventList.Count);
            
            var firstEvent = eventList[0];
            
            Assert.Equal(pid, firstEvent.ProjectId);
            Assert.Equal("create", firstEvent.Operation);
            Assert.Equal("role", firstEvent.EntityType);
            Assert.Equal(existingRoleId, firstEvent.EntityId);
            
            var secondEvent = eventList[0];
            
            Assert.Equal(pid, secondEvent.ProjectId);
            Assert.Equal("create", secondEvent.Operation);
            Assert.Equal("role", secondEvent.EntityType);
            Assert.Equal(existingRoleId, secondEvent.EntityId);
        }
        
        #endregion
        
        #region GetAllRole Tests
        
        [Fact]
        public async Task GetAllRoles_ExcludesArchived()
        {
            // Act
            var result = (await _roleBusiness.GetAllRoles(null, null)).ToList();
    
            // Assert - Check that our test roles are present/absent as expected
            Assert.Contains(result, r => r.Id == rid1);
            Assert.DoesNotContain(result, r => r.Id == rid2); // archived
            Assert.DoesNotContain(result, r => r.Id == rid3); // deleted
            Assert.Contains(result, r => r.Id == rid4);
            Assert.Contains(result, r => r.Id == rid5);
    
            // Verify all returned roles are not archived
            Assert.All(result, r => Assert.False(r.IsArchived));
        }
        
        [Fact]
        public async Task GetAllRoles_FiltersByProject()
        {
            // Act
            var result = (await _roleBusiness.GetAllRoles(pid, null)).ToList();
            
            // Assert
            Assert.Single(result);
            Assert.All(result, r => Assert.Equal(false, r.IsArchived));
            Assert.DoesNotContain(result, r => r.Id == rid1);
            Assert.DoesNotContain(result, r => r.Id == rid2);
            Assert.DoesNotContain(result, r => r.Id == rid3);
            Assert.Contains(result, r => r.Id == rid4);
            Assert.DoesNotContain(result, r => r.Id == rid5);
        }
        
        [Fact]
        public async Task GetAllRoles_FiltersByOrganization()
        {
            // Act
            var result = (await _roleBusiness.GetAllRoles(null, oid)).ToList();
            
            // Assert
            Assert.Single(result);
            Assert.All(result, r => Assert.Equal(false, r.IsArchived));
            Assert.DoesNotContain(result, r => r.Id == rid1);
            Assert.DoesNotContain(result, r => r.Id == rid2);
            Assert.DoesNotContain(result, r => r.Id == rid3);
            Assert.DoesNotContain(result, r => r.Id == rid4);
            Assert.Contains(result, r => r.Id == rid5);
        }
        
        #endregion
        
        #region GetRole Tests

        [Fact]
        public async Task GetRole_Succeeds_WhenExists()
        {
            // Act
            var result = await _roleBusiness.GetRole(rid1);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(rid1, result.Id);
            Assert.Equal("Role 1", result.Name);
            Assert.False(result.IsArchived);
        }
        
        [Fact]
        public async Task GetRole_Succeeds_IfArchived_AndHideArchivedFalse()
        {
            // Act
            var result = await _roleBusiness.GetRole(rid2, false);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(rid2, result.Id);
            Assert.Equal("Role 2", result.Name);
            Assert.True(result.IsArchived);
        }
        
        [Fact]
        public async Task GetRole_Fails_IfArchived_AndHideArchivedTrue()
        {
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.GetRole(rid2, true));
            
            // Assert
            Assert.Contains($"Role with id {rid2} is archived", exception.Message);
        }
        
        [Fact]
        public async Task GetRole_Fails_IfDeletedRole()
        {
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.GetRole(rid3, true));
            
            // Assert
            Assert.Contains($"Role with id {rid3} not found", exception.Message);
        }
        
        #endregion
        
        #region UpdateRole Tests
        
        [Fact]
        public async Task UpdateRole_Success_ReturnsRole()
        {
            // Arrange
            var dto = new UpdateRoleRequestDto
            {
                Name = "Updated Role",
                Description = "Now with a description",
            };
            
            // Act
            var result = await _roleBusiness.UpdateRole(uid, rid1, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(rid1, result.Id);
            Assert.Equal("Updated Role", result.Name);
            Assert.Equal("Now with a description", result.Description);
            
            // Verify it was actually saved to DB
            var savedRole = await Context.Roles.FindAsync(rid1);
            Assert.NotNull(savedRole);
            Assert.Equal("Updated Role", savedRole.Name);
            Assert.Equal("Now with a description", savedRole.Description);
            
            // Ensure that the Role update event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal("update", actualEvent.Operation);
            Assert.Equal("role", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
        }
        
        [Fact]
        public async Task UpdateRole_Success_CreatesEvent()
        {
            // Arrange
            var dto = new UpdateRoleRequestDto
            {
                Name = "Updated Role",
                Description = "Now with a description",
            };
            
            // Act
            var result = await _roleBusiness.UpdateRole(uid, rid1, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Role", result.Name);
            
            // Ensure that the Role update event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal("update", actualEvent.Operation);
            Assert.Equal("role", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
        }
        
        [Fact]
        public async Task UpdateRole_Fails_IfNotFound()
        {
            // Arrange
            var dto = new UpdateRoleRequestDto
            {
                Name = "Updated Role",
                Description = "Now with a description",
            };
            
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.UpdateRole(uid, rid3, dto));
            
            // Assert
            Assert.Contains($"Role with id {rid3} not found", exception.Message);
            
            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        #endregion
        
        #region ArchiveRole Tests
        
        [Fact]
        public async Task ArchiveRole_Succeeds_IfNotArchived()
        {
            // Act
            var result = await _roleBusiness.ArchiveRole(uid, rid1);
            
            // Assert
            Assert.True(result);
            
            // Force EF to sync with database
            Context.ChangeTracker.Clear();
            
            // Verify it was actually saved to DB
            var savedRole = await Context.Roles.FindAsync(rid1);
            Assert.NotNull(savedRole);
            Assert.True(savedRole.IsArchived);
            
            // Ensure that the Role archive event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal("archive", actualEvent.Operation);
            Assert.Equal("role", actualEvent.EntityType);
            Assert.Equal(rid1, actualEvent.EntityId);
        }

        [Fact] 
        public async Task ArchiveRole_RemovesRole_FromProjectMembers()
        {
            // Confirm that user exists as project member with role
            var member = await Context.ProjectMembers.FindAsync(mid);
            Assert.NotNull(member);
            Assert.Equal(pid, member.ProjectId);
            Assert.Equal(uid, member.UserId);
            Assert.Equal(rid4, member.RoleId);
            
            // Act
            var result = await _roleBusiness.ArchiveRole(uid, rid4);
            
            // Assert
            Assert.True(result);
            
            // Force EF to sync with database
            Context.ChangeTracker.Clear();
            
            // Confirm that user no longer holds role
            var updatedMember = await Context.ProjectMembers.FindAsync(mid);
            Assert.NotNull(updatedMember);
            Assert.Equal(pid, updatedMember.ProjectId);
            Assert.Equal(uid, updatedMember.UserId);
            Assert.NotEqual(rid4, updatedMember.RoleId);
            Assert.Null(updatedMember.RoleId);
            
            // Ensure that the Role archive event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal("archive", actualEvent.Operation);
            Assert.Equal("role", actualEvent.EntityType);
            Assert.Equal(rid4, actualEvent.EntityId);
        }
        
        [Fact]
        public async Task ArchiveRole_Fails_IfArchived()
        {
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.ArchiveRole(uid, rid2));
            
            // Assert
            Assert.Contains($"Role with id {rid2} not found or is archived", exception.Message);
            
            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        #endregion
        
        #region UnarchiveRole Tests
        
        [Fact]
        public async Task UnarchiveRole_Succeeds_IfArchived()
        {
            // Act
            var result = await _roleBusiness.UnarchiveRole(uid, rid2);
    
            // Assert
            Assert.True(result);
    
            // Verify it was actually saved to DB
            var savedRole = await Context.Roles.FindAsync(rid2);
            Assert.NotNull(savedRole);
            Assert.False(savedRole.IsArchived);
    
            // Ensure that the Role unarchive event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal("unarchive", actualEvent.Operation);
            Assert.Equal("role", actualEvent.EntityType);
            Assert.Equal(rid2, actualEvent.EntityId);
        }

        [Fact]
        public async Task UnarchiveRole_Fails_IfNotArchived()
        {
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.UnarchiveRole(uid, rid1));
    
            // Assert
            Assert.Contains($"Role with id {rid1} not found or is not archived", exception.Message);
    
            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        #endregion
        
        #region DeleteRole Tests
        
        [Fact]
        public async Task DeleteRole_Succeeds_WhenExists()
        {
            // Act
            var result = await _roleBusiness.DeleteRole(rid1);
    
            // Assert
            Assert.True(result);
    
            // Verify it was actually deleted from DB
            var deletedRole = await Context.Roles.FindAsync(rid1);
            Assert.Null(deletedRole);
    
            // Ensure that the Role delete event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal("delete", actualEvent.Operation);
            Assert.Equal("role", actualEvent.EntityType);
            Assert.Equal(rid1, actualEvent.EntityId);
        }

        [Fact]
        public async Task DeleteRole_Fails_IfNotFound()
        {
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.DeleteRole(rid3));
    
            // Assert
            Assert.Contains($"Role with id {rid3} not found or is archived", exception.Message);
    
            // Ensure that no event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        #endregion
        
        #region GetPermissionsByRole Tests
        
        [Fact]
        public async Task GetPermissionsByRole_Lists_AllPermissionsForRole()
        {
            // Act
            var result = (await _roleBusiness.GetPermissionsByRole(rid1)).ToList();
            
            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Id == permid1);
            Assert.Contains(result, p => p.Id == permid2);
        }

        [Fact]
        public async Task GetPermissionsByRole_DoesNotList_PermissionsNotForRole()
        {
            // Act
            var result = (await _roleBusiness.GetPermissionsByRole(rid1)).ToList();
            
            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Id == permid1);
            Assert.Contains(result, p => p.Id == permid2);
            Assert.DoesNotContain(result, p => p.Id == permid3);
        }

        [Fact]
        public async Task GetPermissionsByRole_Fails_IfRoleNotFound()
        {
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.GetPermissionsByRole(rid3));
            
            // Assert
            Assert.Contains($"Role with id {rid3} not found", exception.Message);
        }

        [Fact]
        public async Task GetPermissionsByRole_ReturnsEmpty_IfNoPermissionsForRole()
        {
            // Act
            var result = await _roleBusiness.GetPermissionsByRole(rid4);
            
            // Assert
            Assert.Empty(result);
        }
        
        #endregion
        
        #region AddPermissionToRole Tests
        
        [Fact]
        public async Task AddPermissionToRole_AddsPermissionToRole()
        {
            // Act
            var result = await _roleBusiness.AddPermissionToRole(rid1, permid3);
    
            // Assert
            Assert.True(result);
    
            // Verify permission was added
            var role = await Context.Roles.Include(r => r.Permissions).FirstAsync(r => r.Id == rid1);
            Assert.Contains(role.Permissions, p => p.Id == permid3);
        }

        [Fact]
        public async Task AddPermissionToRole_Fails_IfRoleNotFound()
        {
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.AddPermissionToRole(rid3, permid3));
            
            // Assert
            Assert.Contains($"Role with id {rid3} not found", exception.Message);
        }

        [Fact]
        public async Task AddPermissionToRole_Fails_IfPermissionNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.AddPermissionToRole(rid1, permid4));
            
            // Assert
            Assert.Contains($"Permission with id {permid4} not found", exception.Message);
        }

        [Fact]
        public async Task AddPermissionToRole_Fails_IfPermissionExistsForRole()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _roleBusiness.AddPermissionToRole(rid1, permid2));
    
            Assert.Contains($"Permission with id {permid2} already exists as part of role {rid1}", exception.Message);
        }
        
        #endregion
        
        #region RemovePermissionFromRole Tests
        
        [Fact]
        public async Task RemovePermissionFromRole_RemovesPermissionFromRole()
        {
            // Act
            var result = await _roleBusiness.RemovePermissionFromRole(rid1, permid1);
    
            // Assert
            Assert.True(result);
    
            // Verify permission was removed
            var updatedRole = await Context.Roles.Include(r => r.Permissions).FirstAsync(r => r.Id == rid1);
            Assert.DoesNotContain(updatedRole.Permissions, p => p.Id == permid1);
        }

        [Fact]
        public async Task RemovePermissionFromRole_Fails_IfRoleNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.RemovePermissionFromRole(rid3, permid3));
            
            // Assert
            Assert.Contains($"Role with id {rid3} not found", exception.Message);
        }

        [Fact]
        public async Task RemovePermissionFromRole_Fails_IfPermissionNotExistsForRole()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.RemovePermissionFromRole(rid1, permid3));
    
            Assert.Contains($"Permission with id {permid3} is not assigned to role {rid1}", exception.Message);
        }
        
        #endregion
        
        #region SetPermissionsForRole Tests
        
        [Fact]
        public async Task SetPermissionsForRole_SetsPermissionsForEmptyRole()
        {
            // Arrange
            var permissionIds = new long[] { permid1, permid2 };
            
            // Act
            var result = await _roleBusiness.SetPermissionsForRole(rid4, permissionIds);
            
            // Assert
            Assert.True(result);
            
            // Verify permissions were set
            var role = await Context.Roles.Include(r => r.Permissions).FirstAsync(r => r.Id == rid1);
            Assert.Equal(2, role.Permissions.Count);
            Assert.Contains(role.Permissions, p => p.Id == permid1);
            Assert.Contains(role.Permissions, p => p.Id == permid2);
        }

        [Fact]
        public async Task SetPermissionsForRole_ResetsPermissionsIfAnyExist()
        {
            // Check existing permissions for role 1 to ensure perms 1 and 2 exist
            var roleBefore = await Context.Roles.Include(r => r.Permissions).FirstAsync(r => r.Id == rid1);
            Assert.Equal(2, roleBefore.Permissions.Count);
            Assert.Contains(roleBefore.Permissions, p => p.Id == permid1);
            Assert.Contains(roleBefore.Permissions, p => p.Id == permid2);
            
            // Arrange
            var permissionIds = new long[] { permid1, permid3 };
            
            // Act
            var result = await _roleBusiness.SetPermissionsForRole(rid1, permissionIds);
            
            // Assert
            Assert.True(result);
            var roleAfter = await Context.Roles.Include(r => r.Permissions).FirstAsync(r => r.Id == rid1);
            Assert.Equal(2, roleAfter.Permissions.Count);
            Assert.Contains(roleAfter.Permissions, p => p.Id == permid1);
            Assert.DoesNotContain(roleAfter.Permissions, p => p.Id == permid2);
            Assert.Contains(roleAfter.Permissions, p => p.Id == permid3);
        }

        [Fact]
        public async Task SetPermissionsForRole_SetsPermissionsBlank_IfNoneSupplied()
        {
            // Check existing permissions for role 1 to ensure perms 1 and 2 exist
            var roleBefore = await Context.Roles.Include(r => r.Permissions).FirstAsync(r => r.Id == rid1);
            Assert.Equal(2, roleBefore.Permissions.Count);
            Assert.Contains(roleBefore.Permissions, p => p.Id == permid1);
            Assert.Contains(roleBefore.Permissions, p => p.Id == permid2);
            
            // Arrange
            var emptyPermissionIds = new long[] { };
            
            // Act
            var result = await _roleBusiness.SetPermissionsForRole(rid1, emptyPermissionIds);
            
            // Assert
            Assert.True(result);
            var roleAfter = await Context.Roles.Include(r => r.Permissions).FirstAsync(r => r.Id == rid1);
            Assert.Empty(roleAfter.Permissions);
        }

        [Fact]
        public async Task SetPermissionsForRole_Fails_IfRoleNotFound()
        {
            // Arrange
            var permissionIds = new long[] { permid1, permid2 };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.SetPermissionsForRole(rid3, permissionIds));
    
            Assert.Contains($"Role with id {rid3} not found", exception.Message);
        }

        [Fact]
        public async Task SetPermissionsForRole_Fails_IfAnyPermissionNotFound()
        {
            // Arrange
            var permissionIds = new long[] { permid4 };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.SetPermissionsForRole(rid1, permissionIds));
    
            Assert.Contains($"Permissions not found: {string.Join(", ", permissionIds)}", exception.Message);
        }
        
        #endregion
        
        #region SetPermissionsByPattern Tests
        
        [Fact]
        public async Task SetPermissionsByPattern_Success_SetsPermissions()
        {
            // Arrange
            var permissionPatterns = new Dictionary<string, string[]>
            {
                { "test", new[] { "read", "write" } },
                { "test2", new[] { "execute" } }
            };

            var testRoleId = rid4;

            // Act
            var result = await _roleBusiness.SetPermissionsByPattern(testRoleId, permissionPatterns);

            // Assert
            Assert.True(result);

            // Verify permissions were set correctly
            var role = await Context.Roles.Include(r => r.Permissions).FirstAsync(r => r.Id == testRoleId);

            // Get resources to query
            var resources = permissionPatterns.Keys.ToList();
    
            // Fetch all permissions for those resources, then filter in memory
            var allPermissionsForResources = await Context.Permissions
                .Where(p => resources.Contains(p.Resource))
                .ToListAsync();
    
            // Filter in memory to get expected permissions
            var expectedPermissions = allPermissionsForResources
                .Where(p => permissionPatterns.ContainsKey(p.Resource) &&
                            permissionPatterns[p.Resource].Contains(p.Action))
                .ToList();

            Assert.Equal(expectedPermissions.Count, role.Permissions.Count);
            
            foreach (var expectedPerm in expectedPermissions)
            {
                Assert.Contains(role.Permissions, p => p.Id == expectedPerm.Id);
            }
        }

        [Fact]
        public async Task SetPermissionsByPattern_Fails_IfRoleNotFound()
        {
            // Arrange
            var permissionPatterns = new Dictionary<string, string[]>
            {
                { "test", new[] { "read" } }
            };
    
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.SetPermissionsByPattern(rid3, permissionPatterns));

            Assert.Contains($"Role with id {rid3} not found", exception.Message);
        }
        
        #endregion
        
        #region LastUpdatedBy Tests

        [Fact]
        public async Task CreateRole_Success_StoresLastUpdatedByUserId()
        {
            // Arrange
            var testRole = new Role
            {
                Name = "Test Role LastUpdatedBy",
                Description = "Test description",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid
            };
            
            // Act
            Context.Roles.Add(testRole);
            await Context.SaveChangesAsync();

            // Assert
            var savedRole = await Context.Roles.FindAsync(testRole.Id);
            Assert.NotNull(savedRole);
            Assert.Equal(uid, savedRole.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateRole_Success_NavigationPropertyLoadsUser()
        {
            // Arrange
            var testRole = new Role
            {
                Name = "Test Role Navigation",
                Description = "Test description 2",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid
            };
            
            Context.Roles.Add(testRole);
            await Context.SaveChangesAsync();

            // Act
            var roleWithUser = await Context.Roles
                .Include(r => r.LastUpdatedByUser)
                .FirstAsync(r => r.Id == testRole.Id);
            
            // Assert
            Assert.NotNull(roleWithUser.LastUpdatedByUser);
            Assert.Equal("Test User", roleWithUser.LastUpdatedByUser.Name);
            Assert.Equal("test@test.com", roleWithUser.LastUpdatedByUser.Email);
            Assert.Equal(uid, roleWithUser.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateRole_Success_WithNullLastUpdatedBy()
        {
            // Arrange
            var testRole = new Role
            {
                Name = "Test Role Null",
                Description = "Test description 3",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            // Act
            Context.Roles.Add(testRole);
            await Context.SaveChangesAsync();

            // Assert
            var savedRole = await Context.Roles.FindAsync(testRole.Id);
            Assert.NotNull(savedRole);
            Assert.Null(savedRole.LastUpdatedBy);
            
            var roleWithUser = await Context.Roles
                .Include(r => r.LastUpdatedByUser)
                .FirstAsync(r => r.Id == testRole.Id);
            
            Assert.Null(roleWithUser.LastUpdatedByUser);
        }

        [Fact]
        public async Task UpdateRole_Success_UpdatesLastUpdatedByUserId()
        {
            // Arrange
            var testRole = new Role
            {
                Name = "Test Role Update",
                Description = "Test description 4",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Roles.Add(testRole);
            await Context.SaveChangesAsync();

            // Act
            testRole.LastUpdatedBy = uid;
            testRole.Name = "Updated Role Name";
            testRole.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            
            Context.Roles.Update(testRole);
            await Context.SaveChangesAsync();

            // Assert
            var updatedRole = await Context.Roles
                .Include(r => r.LastUpdatedByUser)
                .FirstAsync(r => r.Id == testRole.Id);
            
            Assert.Equal(uid, updatedRole.LastUpdatedBy);
            Assert.NotNull(updatedRole.LastUpdatedByUser);
            Assert.Equal("Test User", updatedRole.LastUpdatedByUser.Name);
            Assert.Equal("Updated Role Name", updatedRole.Name);
        }

        #endregion
        
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            
            // create user
            var user = new User 
            { 
                Name = "Test User", 
                Email = "test@test.com",
                Password = "test_password",
                IsArchived = false
            };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            uid = user.Id;
            
            // create test organization
            var organization = new Organization 
            { 
                Name = "Test",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid
            };
            Context.Organizations.Add(organization);
            await Context.SaveChangesAsync();
            oid = organization.Id;

            // create test project
            var project = new Project 
            { 
                Name = "Test",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid,
                OrganizationId = organization.Id
            };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
            
            // Create roles
            var role1 = new Role { Name = "Role 1" };
            var role2 = new Role { Name = "Role 2", IsArchived = true }; // Archive role2
            var role3 = new Role { Name = "Role 3" };
            var role4 = new Role { Name = "Role 4", ProjectId = pid };
            var role5 = new Role { Name = "Role 5", OrganizationId = oid };
            Context.Roles.AddRange(role1, role2, role3, role4, role5);
            await Context.SaveChangesAsync();
            rid1 = role1.Id;
            rid2 = role2.Id;
            rid3 = role3.Id;
            rid4 = role4.Id;
            rid5 = role5.Id;
            
            // Delete role 3
            Context.Roles.Remove(role3);
            await Context.SaveChangesAsync();
            
            // Add user as project member
            var projectMember = new ProjectMember { ProjectId = pid, UserId = uid, RoleId = rid4 };
            Context.ProjectMembers.Add(projectMember);
            await Context.SaveChangesAsync();
            mid = projectMember.Id;
            
            // Create permissions
            var permission1 = new Permission { Name = "Permission 1", Action = "read", Resource = "test" };
            var permission2 = new Permission { Name = "Permission 2", Action = "write", Resource = "test" };
            var permission3 = new Permission { Name = "Permission 3", Action = "execute", Resource = "test2" };
            var permission4 = new Permission { Name = "Permission 4", Action = "glorbulon", Resource = "test" };
            Context.Permissions.AddRange(permission1, permission2, permission3, permission4);
            await Context.SaveChangesAsync();
            permid1 = permission1.Id;
            permid2 = permission2.Id;
            permid3 = permission3.Id;
            permid4 = permission4.Id;
            
            // Delete permission 4
            Context.Permissions.Remove(permission4);
            await Context.SaveChangesAsync();
            
            // Add permissions 1 and 2 to role 1
            var role1perms = await Context.Roles
                .Include(r => r.Permissions)
                .FirstAsync(r => r.Id == rid1);
            role1perms.Permissions.Add(permission1);
            role1perms.Permissions.Add(permission2);
            await Context.SaveChangesAsync();
        }
    }
}
