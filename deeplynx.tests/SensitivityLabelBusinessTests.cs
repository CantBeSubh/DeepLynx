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
    public class SensitivityLabelBusinessTests : IntegrationTestBase
    {
        private EventBusiness _eventBusiness;
        private SensitivityLabelBusiness _labelBusiness;
        
        public SensitivityLabelBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
            _labelBusiness = new SensitivityLabelBusiness(Context, _cacheBusiness, _eventBusiness);
        }
        
        [Fact]
        public async Task CreateSensitivityLabel_Success_ReturnsLabel()
        {
            
        }

        [Fact]
        public async Task CreateSensitivityLabel_Success_CreatesEvent()
        {
            
        }
        
        [Fact]
        public async Task CreateSensitivityLabel_Fails_IfNoName()
        {
            
        }
        
        [Fact]
        public async Task CreateSensitivityLabel_Fails_IfEmptyName()
        {
            
        }

        [Fact]
        public async Task GetAllSensitivityLabels_ExcludesArchived()
        {
            
        }

        [Fact]
        public async Task GetSensitivityLabel_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task GetSensitivityLabel_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task GetSensitivityLabel_Fails_IfDeletedLabel()
        {
            
        }
        
        [Fact]
        public async Task UpdateSensitivityLabel_Success_ReturnsLabel()
        {
            
        }
        
        [Fact]
        public async Task UpdateSensitivityLabel_Success_CreatesEvent()
        {
            
        }
        
        [Fact]
        public async Task UpdateSensitivityLabel_Fails_IfNotFound()
        {
            
        }
        
        [Fact]
        public async Task ArchiveSensitivityLabel_Succeeds_IfNotArchived()
        {
            
        }
        
        [Fact]
        public async Task ArchiveSensitivityLabel_Fails_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchiveSensitivityLabel_Succeeds_IfArchived()
        {
            
        }
        
        [Fact]
        public async Task UnarchiveSensitivityLabel_Fails_IfNotArchived()
        {
            
        }
        
        [Fact]
        public async Task DeleteSensitivityLabel_Succeeds_WhenExists()
        {
            
        }
        
        [Fact]
        public async Task DeleteSensitivityLabel_Fails_IfNotFound()
        {
            
        }
        
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            // create test stuff
            await Context.SaveChangesAsync();
        }
    }
}
