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
    public class OrganizationBusinessTests : IntegrationTestBase
    {
        private EventBusiness _eventBusiness = null!;
        private Mock<ILogger<OrganizationBusiness>> _mockLoggerOrg = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLoggerProject = null!;
        private OrganizationBusiness _organizationBusiness = null!;
        private ProjectBusiness _projectBusiness = null!;
        private UserBusiness _userBusiness = null!;
        // mocked business classes needed for project business
        private Mock<IClassBusiness> _mockClassBusiness = null!;
        private Mock<IDataSourceBusiness> _mockDataSourceBusiness = null!;
        private Mock<IObjectStorageBusiness> _mockObjectStorageBusiness = null!;

        public long oid;    // organization ID
        public long pid;    // project ID
        public long uid;    // user ID
        
        public OrganizationBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            // used in multiple contexts
            _eventBusiness = new EventBusiness(Context);
            
            // project business and dependencies
            _mockLoggerProject = new Mock<ILogger<ProjectBusiness>>();
            _mockClassBusiness = new Mock<IClassBusiness>();
            _mockDataSourceBusiness = new Mock<IDataSourceBusiness>();
            _mockObjectStorageBusiness = new Mock<IObjectStorageBusiness>();
            _projectBusiness = new ProjectBusiness(
                Context, _mockLoggerProject.Object,  _mockClassBusiness.Object, 
                _mockDataSourceBusiness.Object, _mockObjectStorageBusiness.Object,
                _eventBusiness);
            
            // user business
            _userBusiness = new UserBusiness(Context);
            
            // org business and dependencies
            _mockLoggerOrg = new Mock<ILogger<OrganizationBusiness>>();
            _organizationBusiness = new OrganizationBusiness(
                Context, _eventBusiness, _mockLoggerOrg.Object);
        }
        
        [Fact]
        public async Task CreateOrganization_Success_ReturnsOrganization()
        {
            var now = DateTime.UtcNow;
            var dto = new CreateOrganizationRequestDto
            {
                Name = "New Test Organization",
                Description = "New Test Organization Description",
            };
            
            var result = await _organizationBusiness.CreateOrganization(dto);
            
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Description, result.Description);
            
            // verify org was actually created in database
            var createdOrg = await Context.Organizations.FindAsync(result.Id);
            Assert.NotNull(createdOrg);
            Assert.Equal(dto.Name, createdOrg.Name);
        }

        [Fact]
        public async Task CreateOrganization_Success_CreatesEvent()
        {
            
        }
        
        [Fact]
        public async Task CreateOrganization_Fails_IfNoName()
        {
            
        }
        
        [Fact]
        public async Task CreateOrganization_Fails_IfEmptyName()
        {
            
        }

        [Fact]
        public async Task GetAllOrganizations_ExcludesArchived()
        {
            
        }

        [Fact]
        public async Task GetOrganization_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task GetOrganization_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task GetOrganization_Fails_IfDeletedOrg()
        {
            
        }
        
        [Fact]
        public async Task UpdateOrganization_Success_ReturnsOrganization()
        {
            
        }
        
        [Fact]
        public async Task UpdateOrganization_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task ArchiveProject_Succeeds_IfNotArchived()
        {
            
        }
        
        [Fact]
        public async Task ArchiveProject_Fails_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchiveProject_Succeeds_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchiveProject_Fails_IfNotArchived()
        {
            
        }
        
        [Fact]
        public async Task DeleteOrganization_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task DeleteOrganization_Fails_IfNotFOund()
        {
            
        }
        
        [Fact]
        public async Task AddUser_Succeeds_IfOrgAndUserExists()
        {
            
        }
        
        [Fact]
        public async Task AddUser_Fails_IfOrgUserExists()
        {
            
        }
        
        [Fact]
        public async Task UpdateUserAdmin_Succeeds_IfOrgUserExists()
        {
            
        }
        
        [Fact]
        public async Task UpdateUserAdmin_Fails_IfOrgUserNotExists()
        {
            
        }
        
        [Fact]
        public async Task RemoveUser_Succeeds_IfOrgUserExists()
        {
            
        }
        
        [Fact]
        public async Task RemoveUser_Fails_IfOrgUserNotExists()
        {
            
        }
        
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
            Context.Organizations.Add(testOrg);
            await Context.SaveChangesAsync();
            oid = testOrg.Id;

            // create test user
            var testUser = new User
            {
                Name = "Test User",
                Email = "test@test.com",
                IsArchived = false
            };
            Context.Users.Add(testUser);
            await Context.SaveChangesAsync();
            uid = testUser.Id;
            
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
