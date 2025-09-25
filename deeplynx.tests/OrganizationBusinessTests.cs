using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class OrganizationBusinessTests : IntegrationTestBase
    {
        private EventBusiness _eventBusiness = null!;
        private Mock<ILogger<OrganizationBusiness>> _mockLoggerOrg = null!;
        private OrganizationBusiness _organizationBusiness = null!;

        public long oid;    // organization ID
        public long oid2;   // second organization ID
        public long uid;    // user IDs
        public long uid2;   
        
        public OrganizationBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            // used in multiple contexts
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
            
            // org business and dependencies
            _mockLoggerOrg = new Mock<ILogger<OrganizationBusiness>>();
            _organizationBusiness = new OrganizationBusiness(
                Context, _eventBusiness, _mockLoggerOrg.Object);
        }
        
        #region GetAllOrganizations Tests
        
        [Fact]
        public async Task GetAllOrganizations_ExcludesArchived()
        {
            // Act
            var result = await _organizationBusiness.GetAllOrganizations(true);
            var organizations = result.ToList();
            
            // Assert
            Assert.All(organizations, o => Assert.False(o.IsArchived));
            Assert.Contains(organizations, o => o.Id == oid);
            Assert.DoesNotContain(organizations, o => o.Id == oid2); // archived organization
        }
        
        [Fact]
        public async Task GetAllOrganizations_WithHideArchivedFalse_IncludesArchived()
        {
            // Act
            var result = await _organizationBusiness.GetAllOrganizations(false);
            var organizations = result.ToList();
            
            // Assert
            Assert.Contains(organizations, o => o.IsArchived);
            Assert.Contains(organizations, o => o.Id == oid);
            Assert.Contains(organizations, o => o.Id == oid2); // archived organization
        }
        
        #endregion
        
        #region GetOrganization Tests
        
        [Fact]
        public async Task GetOrganization_Succeeds_WhenExists()
        {
            // Act
            var result = await _organizationBusiness.GetOrganization(oid);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(oid, result.Id);
            Assert.Equal("Test Organization", result.Name);
            Assert.Equal("Test org for unit tests", result.Description);
            Assert.False(result.IsArchived);
        }
        
        [Fact]
        public async Task GetOrganization_Fails_IfNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.GetOrganization(99999));
            
            Assert.Contains("Organization with id 99999 does not exist", exception.Message);
        }
        
        [Fact]
        public async Task GetOrganization_Fails_IfArchivedOrg()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.GetOrganization(oid2)); // archived organization
            
            Assert.Contains($"Organization with id {oid2} is archived", exception.Message);
        }
        
        #endregion
        
        #region CreateOrganization Tests
        
        [Fact]
        public async Task CreateOrganization_Success_ReturnsOrganization()
        {
            // Arrange
            var dto = new CreateOrganizationRequestDto
            {
                Name = "New Test Organization",
                Description = "New Test Organization Description",
            };
            
            // Act
            var result = await _organizationBusiness.CreateOrganization(dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Description, result.Description);
            Assert.False(result.IsArchived);
            
            // verify org was actually created in database
            var createdOrg = await Context.Organizations.FindAsync(result.Id);
            Assert.NotNull(createdOrg);
            Assert.Equal(dto.Name, createdOrg.Name);
        }

        [Fact]
        public async Task CreateOrganization_Success_CreatesEvent()
        {
            // Arrange
            var dto = new CreateOrganizationRequestDto
            {
                Name = "Event Test Organization",
                Description = "A test organization for event logging",
            };
            
            // Act
            var result = await _organizationBusiness.CreateOrganization(dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Event Test Organization", result.Name);
            
            // Ensure that the Organization create event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "create",
                EntityType = "organization",
                EntityId = result.Id,
            });
        }
        
        [Fact]
        public async Task CreateOrganization_Fails_IfNoName()
        {
            // Arrange
            var dto = new CreateOrganizationRequestDto
            {
                Description = "Organization without name"
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _organizationBusiness.CreateOrganization(dto));
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task CreateOrganization_Fails_IfEmptyName()
        {
            // Arrange
            var dto = new CreateOrganizationRequestDto
            {
                Name = "",
                Description = "Organization with empty name"
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _organizationBusiness.CreateOrganization(dto));
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region UpdateOrganization Tests
        
        [Fact]
        public async Task UpdateOrganization_Success_ReturnsOrganization()
        {
            // Arrange
            var dto = new UpdateOrganizationRequestDto
            {
                Name = "Updated Organization",
                Description = "Updated description"
            };
            
            // Act
            var result = await _organizationBusiness.UpdateOrganization(oid, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(oid, result.Id);
            Assert.Equal("Updated Organization", result.Name);
            Assert.Equal("Updated description", result.Description);
            
            // Verify it was actually saved to DB
            var savedOrg = await Context.Organizations.FindAsync(oid);
            Assert.NotNull(savedOrg);
            Assert.Equal("Updated Organization", savedOrg.Name);
            Assert.Equal("Updated description", savedOrg.Description);
        }
        
        [Fact]
        public async Task UpdateOrganization_Success_CreatesEvent()
        {
            // Arrange
            var dto = new UpdateOrganizationRequestDto
            {
                Name = "Event Updated Organization"
            };
            
            // Act
            var result = await _organizationBusiness.UpdateOrganization(oid, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Event Updated Organization", result.Name);
            
            // Ensure that the Organization update event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "update",
                EntityType = "organization",
                EntityId = result.Id,
            });
        }
        
        [Fact]
        public async Task UpdateOrganization_Fails_IfNotFound()
        {
            // Arrange
            var dto = new UpdateOrganizationRequestDto
            {
                Name = "Updated Organization"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.UpdateOrganization(99999, dto));
            
            Assert.Contains("Organization with id 99999 does not exist", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task UpdateOrganization_Fails_IfArchived()
        {
            // Arrange
            var dto = new UpdateOrganizationRequestDto
            {
                Name = "Updated Archived Organization"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.UpdateOrganization(oid2, dto)); // archived organization
            
            Assert.Contains($"Organization with id {oid2} does not exist", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region ArchiveOrganization Tests
        
        [Fact]
        public async Task ArchiveOrganization_Succeeds_IfNotArchived()
        {
            // Act
            var result = await _organizationBusiness.ArchiveOrganization(oid);
            
            // Assert
            Assert.True(result);
            
            // Verify it was actually saved to DB
            var savedOrg = await Context.Organizations.FindAsync(oid);
            Assert.NotNull(savedOrg);
            Assert.True(savedOrg.IsArchived);
            
            // Ensure that the Organization archive event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "archive",
                EntityType = "organization",
                EntityId = oid,
            });
        }
        
        [Fact]
        public async Task ArchiveOrganization_Fails_IfArchived()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.ArchiveOrganization(oid2)); // already archived
            
            Assert.Contains($"Organization with id {oid2} not found", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task ArchiveOrganization_Fails_IfNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.ArchiveOrganization(99999));
            
            Assert.Contains("Organization with id 99999 not found", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region UnarchiveOrganization Tests
        
        [Fact]
        public async Task UnarchiveOrganization_Succeeds_IfArchived()
        {
            // Act
            var result = await _organizationBusiness.UnarchiveOrganization(oid2);
            
            // Assert
            Assert.True(result);
            
            // Verify it was actually saved to DB
            var savedOrg = await Context.Organizations.FindAsync(oid2);
            Assert.NotNull(savedOrg);
            Assert.False(savedOrg.IsArchived);
            
            // Ensure that the Organization unarchive event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "unarchive",
                EntityType = "organization",
                EntityId = oid2,
            });
        }
        
        [Fact]
        public async Task UnarchiveOrganization_Fails_IfNotArchived()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.UnarchiveOrganization(oid)); // not archived
            
            Assert.Contains($"Organization with id {oid} not found", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task UnarchiveOrganization_Fails_IfNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.UnarchiveOrganization(99999));
            
            Assert.Contains("Organization with id 99999 not found", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region DeleteOrganization Tests
        
        [Fact]
        public async Task DeleteOrganization_Succeeds_WhenExists()
        {
            // Act
            var result = await _organizationBusiness.DeleteOrganization(oid);
            
            // Assert
            Assert.True(result);
            
            // Verify it was actually deleted from DB
            var deletedOrg = await Context.Organizations.FindAsync(oid);
            Assert.Null(deletedOrg);
        }
        
        [Fact]
        public async Task DeleteOrganization_Fails_IfNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.DeleteOrganization(99999));
            
            Assert.Contains("Organization with id 99999 not found", exception.Message);
        }
        
        [Fact]
        public async Task DeleteOrganization_Fails_IfArchived()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.DeleteOrganization(oid2)); // archived organization
            
            Assert.Contains($"Organization with id {oid2} not found", exception.Message);
        }
        
        #endregion
        
        #region AddUser Tests
        
        [Fact]
        public async Task AddUser_Succeeds_IfOrgAndUserExists()
        {
            // Act
            var result = await _organizationBusiness.AddUserToOrganization(oid, uid2, false);
            
            // Assert
            Assert.True(result);
            
            // Verify it was actually saved to DB
            var orgUser = await Context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.OrganizationId == oid && ou.UserId == uid2);
            Assert.NotNull(orgUser);
            Assert.False(orgUser.IsOrgAdmin);
        }
        
        [Fact]
        public async Task AddUser_Fails_IfOrgUserExists()
        {
            // Act - try to add user that's already in the org
            var result = await _organizationBusiness.AddUserToOrganization(oid, uid, false);
            
            // Assert
            Assert.False(result); // should return false when user already exists
        }
        
        [Fact]
        public async Task AddUser_Fails_IfUserNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.AddUserToOrganization(oid, 99999, false));
            
            Assert.Contains("User with id 99999 not found", exception.Message);
        }
        
        [Fact]
        public async Task AddUser_Fails_IfOrgNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.AddUserToOrganization(99999, uid, false));
            
            Assert.Contains("Organization with id 99999 not found", exception.Message);
        }
        
        #endregion
        
        #region UpdateUserAdmin Tests
        
        [Fact]
        public async Task UpdateUserAdmin_Succeeds_IfOrgUserExists()
        {
            // Act - set user as admin
            var result = await _organizationBusiness.SetOrganizationAdminStatus(oid, uid, true);
            
            // Assert
            Assert.True(result);
            
            // Verify it was actually saved to DB
            var orgUser = await Context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.OrganizationId == oid && ou.UserId == uid);
            Assert.NotNull(orgUser);
            Assert.True(orgUser.IsOrgAdmin);
        }
        
        [Fact]
        public async Task UpdateUserAdmin_Fails_IfOrgUserNotExists()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.SetOrganizationAdminStatus(oid, uid2, true));
            
            Assert.Contains($"User with id {uid2} not found in Org with id {oid}", exception.Message);
        }
        
        #endregion
        
        #region RemoveUser Tests
        
        [Fact]
        public async Task RemoveUser_Succeeds_IfOrgUserExists()
        {
            // Act
            var result = await _organizationBusiness.RemoveUserFromOrganization(oid, uid);
            
            // Assert
            Assert.True(result);
            
            // Verify it was actually removed from DB
            var orgUser = await Context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.OrganizationId == oid && ou.UserId == uid);
            Assert.Null(orgUser);
        }
        
        [Fact]
        public async Task RemoveUser_Fails_IfOrgUserNotExists()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _organizationBusiness.RemoveUserFromOrganization(oid, uid2));
            
            Assert.Contains($"User with id {uid2} not found in Org with id {oid}", exception.Message);
        }
        
        #endregion
        
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            // create test organization
            var testOrg = new Organization
            {
                Name = "Test Organization",
                Description = "Test org for unit tests",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = false
            };
            var archivedOrg = new Organization
            {
                Name = "Archived Organization",
                Description = "Archived org for tests",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = true
            };
            Context.Organizations.AddRange(testOrg, archivedOrg);
            await Context.SaveChangesAsync();
            oid = testOrg.Id;
            oid2 = archivedOrg.Id;

            // create test users
            var testUser = new User
            {
                Name = "Test User",
                Email = "test@test.com",
            };
            var newUser = new User
            {
                Name = "New User",
                Email = "newuser@test.com",
            };
            Context.Users.AddRange(testUser, newUser);
            await Context.SaveChangesAsync();
            uid = testUser.Id;
            uid2 = newUser.Id;
            
            // create test organization user
            var testOrgUser = new OrganizationUser
            {
                OrganizationId = oid,
                UserId = uid,
                IsOrgAdmin = false
            };
            Context.OrganizationUsers.Add(testOrgUser);
            await Context.SaveChangesAsync();
        }
    }
}