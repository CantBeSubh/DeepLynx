using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Record = deeplynx.datalayer.Models.Record;
using System.Text.Json.Nodes;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class RoleBusinessTests : IntegrationTestBase
    {
        private EventBusiness _eventBusiness;
        private RoleBusiness _roleBusiness;

        public long oid;    // organization ID
        public long pid;    // project ID
        public long rid1;   // role IDs
        public long rid2;
        public long rid3;
        public long rid4;
        public long rid5;
        
        public RoleBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
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
        
        [Fact]
        public async Task UpdateRole_Success_ReturnsRole()
        {
            
        }
        
        [Fact]
        public async Task UpdateRole_Success_CreatesEvent()
        {
            
        }
        
        [Fact]
        public async Task UpdateRole_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task ArchiveRole_Succeeds_IfNotArchived()
        {
            
        }

        [Fact] public async Task ArchiveRole_RemovesRole_FromProjectMembers()
        {
            
        }
        
        [Fact]
        public async Task ArchiveRole_Fails_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchiveRole_Succeeds_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchiveRole_Fails_IfNotArchived()
        {
            
        }
        
        [Fact]
        public async Task DeleteRole_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task DeleteRole_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task GetPermissionsByRole_Lists_AllPermissionsForRole()
        {
            
        }
        
        [Fact]
        public async Task GetPermissionsByRole_DoesNotList_PermissionsNotForRole()
        {
            
        }
        
        [Fact]
        public async Task GetPermissionsByRole_Fails_IfRoleNotFound()
        {
            
        }
        
        [Fact]
        public async Task GetPermissionsByRole_ReturnsEmpty_IfNoPermissionsForRole()
        {
            
        }
        
        [Fact]
        public async Task AddPermissionToRole_AddsPermissionToRole()
        {
            
        }
        
        [Fact]
        public async Task AddPermissionToRole_Fails_IfRoleNotFound()
        {
            
        }
        
        [Fact]
        public async Task AddPermissionToRole_Fails_IfPermissionNotFound()
        {
            
        }
        
        [Fact]
        public async Task AddPermissionToRole_Fails_IfPermissionExistsForRole()
        {
            
        }
        
        [Fact]
        public async Task RemovePermissionFromRole_RemovesPermissionFromRole()
        {
            
        }
        
        [Fact]
        public async Task RemovePermissionFromRole_Fails_IfRoleNotFound()
        {
            
        }
        
        [Fact]
        public async Task RemovePermissionFromRole_Fails_IfPermissionNotExistsForRole()
        {
            
        }
        
        [Fact]
        public async Task SetPermissionsForRole_SetsPermissionsForEmptyRole()
        {
            
        }
        
        [Fact]
        public async Task SetPermissionsForRole_ResetsPermissionsIfAnyExist()
        {
            
        }
        
        [Fact]
        public async Task SetPermissionsForRole_SetsPermissionsBlank_IfNoneSupplied()
        {
            
        }
        
        [Fact]
        public async Task SetPermissionsForRole_Fails_IfRoleNotFound()
        {
            
        }
        
        [Fact]
        public async Task SetPermissionsForRole_Fails_IfAnyPermissionNotFound()
        {
            
        }
        
        protected override async Task SeedTestDataAsync()
        {
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
            Context.Roles.Add(role1);
            Context.Roles.Add(role2);
            Context.Roles.Add(role3);
            Context.Roles.Add(role4);
            Context.Roles.Add(role5);
            await Context.SaveChangesAsync();
            rid1 = role1.Id;
            rid2 = role2.Id;
            rid3 = role3.Id;
            rid4 = role4.Id;
            rid5 = role5.Id;
            
            // delete role 3
            Context.Roles.Remove(role3);
            await Context.SaveChangesAsync();
        }
        
        private async Task CleanupTestData()
        {
            // Remove all roles (or just test-specific ones)
            var existingRoles = await Context.Roles.ToListAsync();
            Context.Roles.RemoveRange(existingRoles);
            await Context.SaveChangesAsync();
        }
    }
}
