using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Moq;

namespace deeplynx.tests
{
    public class TagBusinessTests : IntegrationTestBase
    {
        private DeeplynxContext _context;
        private TagBusiness _tagBusiness;
        private Mock<IRecordMappingBusiness> _mockRecordMappingBusiness;

        public TagBusinessTests()
        {
            _mockRecordMappingBusiness = new Mock<IRecordMappingBusiness>();
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _tagBusiness = new TagBusiness( Context, _mockRecordMappingBusiness.Object);
        }
        
        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        #region GetAllTags Tests

        [Fact]
        public async Task GetAllTags_ValidProjectId_ReturnsActiveTags()
        {
            // Act
            var result = await _tagBusiness.GetAllTags(1);
            var tags = result.ToList();

            // Assert
            Assert.Equal(4, tags.Count);
            Assert.All(tags, ds => Assert.Equal(1, ds.ProjectId));
            Assert.All(tags, ds => Assert.Null(ds.ArchivedAt));
            Assert.Contains(tags, ds => ds.Name == "Analytics");
            Assert.Contains(tags, ds => ds.Name == "Marketing");
            Assert.Contains(tags, ds => ds.Name == "Customer Data");
            Assert.Contains(tags, ds => ds.Name == "Business Intelligence");
            Assert.DoesNotContain(tags, ds => ds.Name == "Logistics");
            Assert.DoesNotContain(tags, ds => ds.Name == "Optimization");
            Assert.DoesNotContain(tags, ds => ds.Name == "Real-time Monitoring");
            Assert.DoesNotContain(tags, ds => ds.Name == "Predictive Analytics");
            Assert.DoesNotContain(tags, ds => ds.Name == "Inventory Management");
            Assert.DoesNotContain(tags, ds => ds.Name == "Migration");
            Assert.DoesNotContain(tags, ds => ds.Name == "Legacy Systems");
            Assert.DoesNotContain(tags, ds => ds.Name == "Data Transformation");
        }

        [Fact]
        public async Task GetAllTags_ProjectWithNoTags_ReturnsEmptyList()
        {
            // Act
            var result = await _tagBusiness.GetAllTags(999);
            var tags = result.ToList();

            // Assert
            Assert.Empty(tags);
        }

        [Fact]
        public async Task GetAllTags_DifferentProject_ReturnsCorrectTags()
        {
            // Act
            var result = await _tagBusiness.GetAllTags(2);
            var tags = result.ToList();

            // Assert
            Assert.Equal(5, tags.Count);
            Assert.Equal("Logistics", tags.First().Name);
            Assert.Equal(2, tags.First().ProjectId);
        }

        #endregion
        
        #region GetTag Tests

        [Fact]
        public async Task GetTag_ValidIds_ReturnsTag()
        {
            // Act
            var result = await _tagBusiness.GetTagById(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Analytics", result.Name);
            Assert.Equal("john.smith@company.com", result.CreatedBy);
            Assert.Equal("sarah.johnson@company.com", result.ModifiedBy);
            Assert.Null( result.ArchivedAt);
            Assert.Equal(1, result.ProjectId);
        }

        [Fact]
        public async Task GetTag_NonExistentTag_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.GetTagById(1, 999));
            
            Assert.Contains("Tag with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task GetTag_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.GetTagById(2, 1)); // Tag 1 belongs to project 1, not 2
            
            Assert.Contains("Tag with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task GetTag_ArchivedTag_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.GetTagById(3, 10)); // Tag 10 of project 3 is archived
            
            Assert.Contains("Tag with id 10 not found", exception.Message);
        }

        #endregion

        #region CreateTag Tests

        [Fact]
        public async Task CreateTag_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _tagBusiness.CreateTag(1, null));
        }
        
                    
        [Fact]
        public async Task CreateTagRequest_Fails_IfNoName()
        { 
            var missingNameDto = new TagRequestDto() { Name = null, CreatedBy = "Test Suite" };

            var exception =
                await Assert.ThrowsAsync<ValidationException>(() => _tagBusiness.CreateTag(1, missingNameDto));
            
            Assert.Contains("Name is required and cannot be empty or whitespace.", exception.Message);
        }
    
        #endregion
      
    }
}