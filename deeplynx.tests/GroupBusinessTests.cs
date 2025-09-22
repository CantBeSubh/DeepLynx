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
    public class GroupBusinessTests : IntegrationTestBase
    {
        private EventBusiness _eventBusiness;
        private GroupBusiness _groupBusiness;

        public long oid;    // organization ID
        public long uid;    // user ID
        public long gid;    // group ID
        
        public GroupBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
            _groupBusiness = new GroupBusiness(Context, _eventBusiness);
        }
        
        [Fact]
        public async Task CreateGroup_Success_ReturnsGroup()
        {
            
        }

        [Fact]
        public async Task CreateGroup_Success_CreatesEvent()
        {
            
        }
        
        [Fact]
        public async Task CreateGroup_Fails_IfNoName()
        {
            
        }
        
        [Fact]
        public async Task CreateGroup_Fails_IfEmptyName()
        {
            
        }

        [Fact]
        public async Task GetAllGroups_ExcludesArchived()
        {
            
        }

        [Fact]
        public async Task GetGroup_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task GetGroup_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task GetGroup_Fails_IfDeletedGroup()
        {
            
        }
        
        [Fact]
        public async Task UpdateGroup_Success_ReturnsGroup()
        {
            
        }
        
        [Fact]
        public async Task UpdateGroup_Success_CreatesEvent()
        {
            
        }
        
        [Fact]
        public async Task UpdateGroup_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task ArchiveGroup_Succeeds_IfNotArchived()
        {
            
        }
        
        [Fact]
        public async Task ArchiveGroup_Fails_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchiveGroup_Succeeds_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchiveGroup_Fails_IfNotArchived()
        {
            
        }
        
        [Fact]
        public async Task DeleteGroup_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task DeleteGroup_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task AddUser_Succeeds_IfGroupAndUserExists()
        {
            
        }
        
        [Fact]
        public async Task AddUser_Fails_IfGroupUserExists()
        {
            
        }
        
        [Fact]
        public async Task RemoveUser_Succeeds_IfGroupUserExists()
        {
            
        }
        
        [Fact]
        public async Task RemoveUser_Fails_IfGroupUserNotExists()
        {
            
        }
        
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            // create test organization
            
            // create test user
            
            // create test group
            await Context.SaveChangesAsync();
        }
    }
}
