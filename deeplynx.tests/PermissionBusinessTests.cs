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
    public class PermissionBusinessTests : IntegrationTestBase
    {
        private EventBusiness _eventBusiness;
        private PermissionBusiness _permissionBusiness;

        public long oid;    // organization ID
        public long pid;    // project ID
        public long rid;    // role ID
        
        public PermissionBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
            _permissionBusiness = new PermissionBusiness(Context, _eventBusiness, _cacheBusiness);
        }
        
        [Fact]
        public async Task GetAllPermissions_IncludesHardcoded_WhenNoFilters()
        {
            
        }
        
        [Fact]
        public async Task GetAllPermissions_FiltersOnLabelId()
        {
            
        }
        
        [Fact]
        public async Task GetAllPermissions_FiltersOnProjectId()
        {
            
        }
        
        [Fact]
        public async Task GetAllPermissions_FiltersOnOrganizationId()
        {
            
        }
        
        [Fact]
        public async Task GetAllPermissions_IncludesHardcoded_WhenMultipleFilters()
        {
            
        }
        
        [Fact]
        public async Task GetAllPermissions_ExcludesArchived()
        {
            
        }
        
        [Fact]
        public async Task GetAllPermissions_WithHideArchivedFalse_IncludesArchived()
        {
            
        }

        [Fact]
        public async Task GetPermission_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task GetPermission_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task GetPermission_Fails_IfArchivedPermission()
        {
            
        }
        
        [Fact]
        public async Task CreatePermission_Succeeds_WithProjectSupplied()
        {
            
        }
        
        [Fact]
        public async Task CreatePermission_Succeeds_WithOrganizationSupplied()
        {
            
        }

        [Fact]
        public async Task CreatePermission_Success_CreatesEvent()
        {
            
        }
        
        [Fact]
        public async Task CreatePermission_Fails_IfBothProjectAndOrgAreSet()
        {
            
        }
        
        [Fact]
        public async Task CreatePermission_Fails_IfNeitherProjectNorOrgAreSet()
        {
            
        }
        
        [Fact]
        public async Task CreatePermission_Fails_IfNoName()
        {
            
        }
        
        [Fact]
        public async Task CreatePermission_Fails_IfNoAction()
        {
            
        }
        
        [Fact]
        public async Task CreatePermission_Fails_IfNoLabelId()
        {
            
        }
        
        [Fact]
        public async Task CreatePermission_DoesNot_SetResource()
        {
            
        }
        
        [Fact]
        public async Task CreatePermission_DoesNot_SetIsHardcoded()
        {
            
        }
        
        [Fact]
        public async Task UpdatePermission_Success_ReturnsPermission()
        {
            
        }
        
        [Fact]
        public async Task UpdatePermission_Success_CreatesEvent()
        {
            
        }
        
        [Fact]
        public async Task UpdatePermission_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task UpdatePermission_DoesNot_SetResource()
        {
            
        }
        
        [Fact]
        public async Task UpdatePermission_DoesNot_SetIsHardcoded()
        {
            
        }
        
        [Fact]
        public async Task UpdatePermission_Fails_IfHardcoded()
        {
            
        }
        
        [Fact]
        public async Task ArchivePermission_Succeeds_IfNotArchived()
        {
            
        }
        
        [Fact]
        public async Task ArchivePermission_Fails_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task ArchivePermission_Fails_IfHardcoded()
        {
            
        }
        
        [Fact]
        public async Task UnarchivePermission_Succeeds_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchivePermission_Fails_IfNotArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchivePermission_Fails_IfHardcoded()
        {
            
        }
        
        [Fact]
        public async Task DeletePermission_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task DeletePermission_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task DeletePermission_Fails_IfHardcoded()
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
