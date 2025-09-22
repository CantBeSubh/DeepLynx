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
        public long rid;    // role ID
        
        public RoleBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
            _roleBusiness = new RoleBusiness(Context, _cacheBusiness, _eventBusiness);
        }
        
        [Fact]
        public async Task CreateRole_Success_ReturnsRole()
        {
            
        }

        [Fact]
        public async Task CreateRole_Success_CreatesEvent()
        {
            
        }
        
        [Fact]
        public async Task CreateRole_Fails_IfNoName()
        {
            
        }
        
        [Fact]
        public async Task CreateRole_Fails_IfEmptyName()
        {
            
        }

        [Fact]
        public async Task GetAllRoles_ExcludesArchived()
        {
            
        }

        [Fact]
        public async Task GetRole_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task GetRole_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task GetRole_Fails_IfDeletedRole()
        {
            
        }
        
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
        
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            // create test organization
            
            // create test project
            
            // create test roles
            await Context.SaveChangesAsync();
        }
    }
}
