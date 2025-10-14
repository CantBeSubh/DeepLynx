using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
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
            var result = await _roleBusiness.CreateRole(dto, pid, null);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Project Role", result.Name);
            Assert.Equal(pid, result.ProjectId);
            
            // Verify it was actually saved to DB
            var savedRole = await Context.Roles.FindAsync(result.Id);
            Assert.NotNull(savedRole);
            Assert.Equal("Project Role", savedRole.Name);
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
            var result = await _roleBusiness.CreateRole(dto, null, oid);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Org Role", result.Name);
            Assert.Equal(oid, result.OrganizationId);
            
            // Verify it was actually saved to DB
            var savedRole = await Context.Roles.FindAsync(result.Id);
            Assert.NotNull(savedRole);
            Assert.Equal("Org Role", savedRole.Name);
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
            var result = await _roleBusiness.CreateRole(dto, pid, null);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Event Role", result.Name);
            
            // Ensure that the Role create event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "role",
                EntityId = result.Id,
            });
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
                () => _roleBusiness.CreateRole(dto, pid, null));
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task CreateRole_Fails_IfNullDto()
        {
            // Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _roleBusiness.CreateRole(null, pid, null));
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
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
            await Assert.ThrowsAsync<ArgumentException>(
                () => _roleBusiness.CreateRole(dto, pid, oid));
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
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
            await Assert.ThrowsAsync<ArgumentException>(
                () => _roleBusiness.CreateRole(dto, null, null));
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        # region BulkCreateRoles Tests
        
        [Fact]
        public async Task CreateRoles_Success_OnBulkCreate()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var bulkDto = new List<CreateRoleRequestDto>
            {
                new CreateRoleRequestDto
                {
                    Name = $"Test Role 1",
                    Description = "Test Description 1"
                },
                new CreateRoleRequestDto
                {
                    Name = $"Test Role 2",
                    Description = "Test Description 2"
                }
            };

            // Act
            var result = await _roleBusiness.BulkCreateRoles(pid, bulkDto);
    
            // Assert
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Test Role 1");
            result.First().Description.Should().Be("Test Description 1");
            result.Last().Name.Should().Be("Test Role 2");
            result.Last().Description.Should().Be("Test Description 2");
    
            // Ensure the create event is logged for each role create
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(2);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "role",
                EntityId = result[0].Id,
            });
            eventList[1].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "role",
                EntityId = result[1].Id,
            });
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
    
            var created = await _roleBusiness.BulkCreateRoles(pid, new List<CreateRoleRequestDto> { existingRole });
            var existingRoleId = created.First().Id;
    
            // Clear events from initial creation
            Context.Events.RemoveRange(Context.Events);
            await Context.SaveChangesAsync();
    
            var bulkDto = new List<CreateRoleRequestDto>
            {
                new CreateRoleRequestDto
                {
                    Name = "Existing Role",
                    Description = "Updated Description"
                }
            };

            // Act
            var result = await _roleBusiness.BulkCreateRoles(pid, bulkDto);
    
            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(existingRoleId); // Same ID as existing role
            result.First().Name.Should().Be("Existing Role");
            result.First().Description.Should().Be("Updated Description");
    
            // Ensure the create event is logged even for upsert
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "role",
                EntityId = existingRoleId,
            });
        }
        
        # endregion
        
        #region GetAllRole Tests
        
        [Fact]
        public async Task GetAllRoles_ExcludesArchived()
        {
            // Arrange: reset test data to avoid race conditions
            await CleanupTestData();
            await SeedTestDataAsync();
            
            // Act
            var result = await _roleBusiness.GetAllRoles(null, null);
            var roles = result.ToList();
            
            // Assert
            Assert.Equal(3, roles.Count);
            Assert.All(roles, r => Assert.Equal(false, r.IsArchived));
            Assert.Contains(roles, r => r.Id == rid1);
            Assert.DoesNotContain(roles, r => r.Id == rid2);
            Assert.DoesNotContain(roles, r => r.Id == rid3);
            Assert.Contains(roles, r => r.Id == rid4);
            Assert.Contains(roles, r => r.Id == rid5);
        }
        
        [Fact]
        public async Task GetAllRoles_FiltersByProject()
        {
            // Act
            var result = await _roleBusiness.GetAllRoles(pid, null);
            var roles = result.ToList();
            
            // Assert
            Assert.Equal(1, roles.Count);
            Assert.All(roles, r => Assert.Equal(false, r.IsArchived));
            Assert.DoesNotContain(roles, r => r.Id == rid1);
            Assert.DoesNotContain(roles, r => r.Id == rid2);
            Assert.DoesNotContain(roles, r => r.Id == rid3);
            Assert.Contains(roles, r => r.Id == rid4);
            Assert.DoesNotContain(roles, r => r.Id == rid5);
        }
        
        [Fact]
        public async Task GetAllRoles_FiltersByOrganization()
        {
            // Act
            var result = await _roleBusiness.GetAllRoles(null, oid);
            var roles = result.ToList();
            
            // Assert
            Assert.Equal(1, roles.Count);
            Assert.All(roles, r => Assert.Equal(false, r.IsArchived));
            Assert.DoesNotContain(roles, r => r.Id == rid1);
            Assert.DoesNotContain(roles, r => r.Id == rid2);
            Assert.DoesNotContain(roles, r => r.Id == rid3);
            Assert.DoesNotContain(roles, r => r.Id == rid4);
            Assert.Contains(roles, r => r.Id == rid5);
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
            var result = await _roleBusiness.UpdateRole(rid1, dto);
            
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
            var result = await _roleBusiness.UpdateRole(rid1, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Role", result.Name);
            
            // Ensure that the Role update event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "update",
                EntityType = "role",
                EntityId = result.Id,
            });
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
                () => _roleBusiness.UpdateRole(rid3, dto));
            
            // Assert
            Assert.Contains($"Role with id {rid3} not found", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region ArchiveRole Tests
        
        [Fact]
        public async Task ArchiveRole_Succeeds_IfNotArchived()
        {
            // Act
            var result = await _roleBusiness.ArchiveRole(rid1);
            
            // Assert
            Assert.True(result);
            
            // Force EF to sync with database
            Context.ChangeTracker.Clear();
            
            // Verify it was actually saved to DB
            var savedRole = await Context.Roles.FindAsync(rid1);
            Assert.NotNull(savedRole);
            Assert.True(savedRole.IsArchived);
            
            // Ensure that the Role archive event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "archive",
                EntityType = "role",
                EntityId = rid1,
            });
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
            var result = await _roleBusiness.ArchiveRole(rid4);
            
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
        }
        
        [Fact]
        public async Task ArchiveRole_Fails_IfArchived()
        {
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.ArchiveRole(rid2));
            
            // Assert
            Assert.Contains($"Role with id {rid2} not found or is archived", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region UnarchiveRole Tests
        
        [Fact]
        public async Task UnarchiveRole_Succeeds_IfArchived()
        {
            // Act
            var result = await _roleBusiness.UnarchiveRole(rid2);
    
            // Assert
            Assert.True(result);
    
            // Verify it was actually saved to DB
            var savedRole = await Context.Roles.FindAsync(rid2);
            Assert.NotNull(savedRole);
            Assert.False(savedRole.IsArchived);
    
            // Ensure that the Role unarchive event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "unarchive",
                EntityType = "role",
                EntityId = rid2,
            });
        }

        [Fact]
        public async Task UnarchiveRole_Fails_IfNotArchived()
        {
            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _roleBusiness.UnarchiveRole(rid1));
    
            // Assert
            Assert.Contains($"Role with id {rid1} not found or is not archived", exception.Message);
    
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
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
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "delete",
                EntityType = "role",
                EntityId = rid1,
            });
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
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region GetPermissionsByRole Tests
        
        [Fact]
        public async Task GetPermissionsByRole_Lists_AllPermissionsForRole()
        {
            // Act
            var result = await _roleBusiness.GetPermissionsByRole(rid1);
            var permissions = result.ToList();
            
            // Assert
            Assert.Equal(2, permissions.Count);
            Assert.Contains(permissions, p => p.Id == permid1);
            Assert.Contains(permissions, p => p.Id == permid2);
        }

        [Fact]
        public async Task GetPermissionsByRole_DoesNotList_PermissionsNotForRole()
        {
            // Act
            var result = await _roleBusiness.GetPermissionsByRole(rid1);
            var permissions = result.ToList();
            
            // Assert
            Assert.Equal(2, permissions.Count);
            Assert.Contains(permissions, p => p.Id == permid1);
            Assert.Contains(permissions, p => p.Id == permid2);
            Assert.DoesNotContain(permissions, p => p.Id == permid3);
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
            var permissions = result.ToList();
            
            // Assert
            Assert.Empty(permissions);
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
        
        # region SetPermissionsByPattern Tests
        
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

            role.Permissions.Count.Should().Be(expectedPermissions.Count);

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
        
        # endregion
        
        protected override async Task SeedTestDataAsync()
        {
            await CleanupTestData();
            
            await base.SeedTestDataAsync();
            // create test organization
            var organization = new Organization { Name = "Test" };
            Context.Organizations.Add(organization);
            await Context.SaveChangesAsync();
            oid = organization.Id;
            
            // create test project
            var project = new Project { Name = "Test" };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
            
            // create test roles
            var role1 = new Role { Name = "Role 1" };
            var role2 = new Role { Name = "Role 2", IsArchived = true }; // archive role 2
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
            
            // delete role 3
            Context.Roles.Remove(role3);
            await Context.SaveChangesAsync();
            
            // create user
            var user = new User { Name = "Test User", Email = "test@test.com" };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            uid = user.Id;
            
            // add user as project member
            var projectMember = new ProjectMember { ProjectId = pid, UserId = uid, RoleId = rid4 };
            Context.ProjectMembers.Add(projectMember);
            await Context.SaveChangesAsync();
            mid = projectMember.Id;
            
            // create permissions
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
            
            // delete permission 4
            Context.Permissions.Remove(permission4);
            await Context.SaveChangesAsync();
            
            // add permissions 1 and 2 to role 1
            var role1perms = await Context.Roles
                .Include(r => r.Permissions)
                .FirstAsync(r => r.Id == rid1);
            role1perms.Permissions.Add(permission1);
            role1perms.Permissions.Add(permission2);
            await Context.SaveChangesAsync();
        }
        
        private async Task CleanupTestData()
        {
            // Remove all project members (or just test-specific ones)
            var existingProjectMembers = await Context.ProjectMembers.ToListAsync();
            Context.ProjectMembers.RemoveRange(existingProjectMembers);
            await Context.SaveChangesAsync();
            
            // Remove all permissions (or just test-specific ones)
            var existingPerms = await Context.Permissions.ToListAsync();
            Context.Permissions.RemoveRange(existingPerms);
            await Context.SaveChangesAsync();
            
            // Remove all roles (or just test-specific ones)
            var existingRoles = await Context.Roles.ToListAsync();
            Context.Roles.RemoveRange(existingRoles);
            await Context.SaveChangesAsync();
        }
    }
}
