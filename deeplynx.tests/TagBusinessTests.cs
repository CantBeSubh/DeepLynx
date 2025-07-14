using System.ComponentModel.DataAnnotations;
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
        private readonly Mock<IRecordMappingBusiness> _mockRecordMappingBusiness;

        public TagBusinessTests()
        {
            _mockRecordMappingBusiness = new Mock<IRecordMappingBusiness>();
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _tagBusiness = new TagBusiness( 
                Context, 
                _mockRecordMappingBusiness.Object);
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
            var result = await _tagBusiness.GetAllTags(1, false);
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
            var result = await _tagBusiness.GetAllTags(999, false);
            var tags = result.ToList();

            // Assert
            Assert.Empty(tags);
        }

        [Fact]
        public async Task GetAllTags_DifferentProject_ReturnsCorrectTags()
        {
            // Act
            var result = await _tagBusiness.GetAllTags(2, false);
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
            var result = await _tagBusiness.GetTagById(1, 1, false);

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
                () => _tagBusiness.GetTagById(1, 999, false));
            
            Assert.Contains("Tag with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task GetTag_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.GetTagById(2, 1, false)); // Tag 1 belongs to project 1, not 2
            
            Assert.Contains("Tag with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task GetTag_ArchivedTag_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.GetTagById(3, 10, true)); // Tag 10 of project 3 is archived
            
            Assert.Contains("Tag with id 10 is archived", exception.Message);
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
        public async Task CreateTag_ValidDto_CreatesTag()
        {
            // Arrange
            var dto = new TagRequestDto
            {
                Name = "Tag One",
                CreatedBy = "Test Suite"
            };

            // Act
            var result = await _tagBusiness.CreateTag(1, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Tag One", result.Name);
            Assert.Equal("Test Suite", result.CreatedBy);
            Assert.Equal(1, result.ProjectId);

            // Verify it was actually saved to database
            var savedTag = await Context.Tags.FindAsync(result.Id);
            Assert.NotNull(savedTag);
            Assert.Equal("Tag One", savedTag.Name);
        }

        [Fact]
        public async Task CreateTag_SetsCreatedAtAndCreatedBy()
        {
            // Arrange
            var dto = new TagRequestDto
            {
                Name = "Tag Timestamp Test",
                CreatedBy = "Test Suite"
            };

            var beforeCreate = DateTime.UtcNow;

            // Act
            var result = await _tagBusiness.CreateTag(1, dto);

            // Assert
            Assert.True(result.CreatedAt >= beforeCreate);
            Assert.True(result.CreatedAt <= DateTime.UtcNow);
            // CreatedBy can be null in current implementation (TODO: JWT implementation)
            Assert.Equal(result.CreatedBy, dto.CreatedBy);
        }
                
        [Fact]
        public async Task CreateTagRequest_Fails_IfNoName()
        { 
            var missingNameDto = new TagRequestDto() { Name = null, CreatedBy = "Test Suite" };

            var exception =
                await Assert.ThrowsAsync<ValidationException>(() => _tagBusiness.CreateTag(1, missingNameDto));
            
            Assert.Contains("Name is required and cannot be empty or whitespace", exception.Message);
        }
        
        /* TODO: revisit after JSON web token implementation
        [Fact]
        public async Task CreateTagRequest_Fails_IfNoCreatedBy()
        {
            var missingCreatedByDto = new TagRequestDto() { Name = "Tag One", CreatedBy = null };
            var exception =
                await Assert.ThrowsAsync<ValidationException>(() => _tagBusiness.CreateTag(1, missingCreatedByDto));
            
            Assert.Contains("CreatedBy is required and cannot be empty or whitespace", exception.Message);
        }*/

        #endregion
        
        #region UpdateTag Tests

        [Fact]
        public async Task UpdateTag_ValidUpdate_UpdatesTag()
        {
            // Arrange
            var dto = new TagRequestDto
            {
                Name = "Updated Test Tag",
                CreatedBy = "Test Suite",
            };

            // Act
            var result = await _tagBusiness.UpdateTag(1, 1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Updated Test Tag", result.Name);
            Assert.Equal("john.smith@company.com", result.CreatedBy);
            Assert.NotNull(result.ModifiedAt);

            // Verify it was actually updated in database
            var updatedTag = await Context.Tags.FindAsync((long)1);
            Assert.Equal("Updated Test Tag", updatedTag.Name);
            Assert.NotNull(updatedTag.ModifiedAt);
        }

        [Fact]
        public async Task UpdateTag_NonExistentTag_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new TagRequestDto
            {
                Name = "Update Test Tag",
                CreatedBy = "Test Suite"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.UpdateTag(1, 999, dto));

            Assert.Contains("Tag with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateTag_WrongProject_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new TagRequestDto
            {
                Name = "Update Test",
                CreatedBy = "Test Suite"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.UpdateTag(2, 1, dto)); // Tag 1 belongs to project 2

            Assert.Contains("Tag with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateTag_ArchivedTag_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new TagRequestDto
            {
                Name = "Update Test",
                CreatedBy = "Test Suite"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.UpdateTag(3, 10, dto)); // Tag 10 is archived

            Assert.Contains("Tag with id 10 not found", exception.Message);
        }

        #endregion
        
        #region DeleteTag Tests

        [Fact]
        public async Task DeleteTag_ValidTag_DeletesSuccessfully()
        {
            // Act
            var result = await _tagBusiness.DeleteTag(1, 2);

            // Assert
            Assert.True(result);

            // Verify it was actually deleted from database
            var deletedTag = await Context.Tags.FindAsync((long)2);
            Assert.Null(deletedTag);
        }

        [Fact]
        public async Task DeleteTag_NonExistentTag_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.DeleteTag(1, 999));

            Assert.Contains("Tag with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task DeleteTag_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.DeleteTag(2, 1)); // Tag 1 belongs to project 1

            Assert.Contains("Tag with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task DeleteTag_ArchivedTag_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.DeleteTag(3, 11)); // Tag 11 is archived

            Assert.Contains("Tag with id 11 not found", exception.Message);
        }

        #endregion
        
         #region ArchiveTag Tests

         [Fact]
         public async Task ArchiveTag_ValidTag_ArchivesSuccessfully()
         {
             // Arrange
             var beforeArchive = DateTime.UtcNow;

             // Act
             var result = await _tagBusiness.ArchiveTag(1, 1);

             // Assert
             Assert.True(result);

             // Verify it was actually archived in database
             var archivedTag = await Context.Tags.FindAsync((long)1);
             Assert.NotNull(archivedTag);
             Assert.NotNull(archivedTag.ArchivedAt);
             Assert.True(archivedTag.ArchivedAt >= beforeArchive);
             Assert.True(archivedTag.ArchivedAt <= DateTime.UtcNow);
         }

         [Fact]
         public async Task ArchiveTag_NonExistentTag_ThrowsKeyNotFoundException()
         {
             // Act & Assert
             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                 () => _tagBusiness.ArchiveTag(1, 999));

             Assert.Contains("Tag with id 999 not found", exception.Message);
         }

         [Fact]
         public async Task ArchiveTag_WrongProject_ThrowsKeyNotFoundException()
         {
             // Act & Assert
             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                 () => _tagBusiness.ArchiveTag(2, 1));

             Assert.Contains("Tag with id 1 not found", exception.Message);
         }

         [Fact]
         public async Task ArchiveTag_AlreadyArchivedTag_ThrowsKeyNotFoundException()
         {
             // Act & Assert
             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                 () => _tagBusiness.ArchiveTag(3, 12)); // Tag 12 is already archived

             Assert.Contains("Tag with id 12 not found", exception.Message);
         }

         [Fact]
         public async Task ArchiveTag_ArchivedTagNotReturnedInGetAll()
         {
             // Arrange
             var initialCount = (await _tagBusiness.GetAllTags(1, true)).Count();

             // Act
             await _tagBusiness.ArchiveTag(1, 1);
             var finalCount = (await _tagBusiness.GetAllTags(1, true)).Count();

             // Assert
             Assert.Equal(initialCount - 1, finalCount);
         }

         #endregion
         
         #region Edge Cases and Integration Tests

         [Fact]
         public async Task TagOperations_ConcurrentModification_HandlesCorrectly()
         {
             // This test simulates concurrent operations on the same data source
             // In a real scenario, you might want to test with actual concurrent tasks

             // Arrange
             var dto1 = new TagRequestDto
             {
                 Name = "Concurrent Tag Update 1",
                 CreatedBy = "Test Suite"
             };

             var dto2 = new TagRequestDto
             {
                 Name = "Concurrent Tag Update 2",
                 CreatedBy = "Test Suite"
             };

             // Act
             var task1 = await _tagBusiness.UpdateTag(1, 1, dto1);
             var task2 = await _tagBusiness.UpdateTag(1, 2, dto2);

             // Assert
             var result1 = task1;
             var result2 = task2;

             Assert.Equal("Concurrent Tag Update 1", result1.Name);
             Assert.Equal("Concurrent Tag Update 2", result2.Name);

         }

         [Fact]
         public async Task TagOperations_SpecialCharactersInFields_HandlesCorrectly()
         {
             // Arrange
             var dto = new TagRequestDto
             {
                 Name = "Test with émojis 🚀 and ñ special chars 中文",
                 CreatedBy = "Name with quotes \"test\" and 'single quotes'",
             };

             // Act
             var result = await _tagBusiness.CreateTag(1, dto);

             // Assert
             Assert.Equal("Test with émojis 🚀 and ñ special chars 中文", result.Name);
             Assert.Contains("quotes \"test\"", result.CreatedBy);
         }

          [Fact]
          public void TagRequestDto_AllProperties_CanBeSetAndRetrieved()
          {
              // Arrange & Act
              var dto = new TagRequestDto
              {
                  Name = "Tag One",
                  CreatedBy = "Test Suite",
              };

              // Assert
              Assert.Equal("Tag One", dto.Name);
              Assert.Equal("Test Suite", dto.CreatedBy);
          }

          [Fact]
          public void TagResponseDto_AllProperties_CanBeSetAndRetrieved()
          {
              // Arrange & Act
              var now = DateTime.UtcNow;

              var dto = new TagResponseDto
              {
                  Id = 1,
                  Name = "Tag One",
                  CreatedBy = "Test Suite",
                  ProjectId = 1,
                  CreatedAt = now,
                  ModifiedBy = "modified@example.com",
                  ModifiedAt = now.AddDays(1),
                  ArchivedAt = null
              };

              // Assert
              Assert.Equal(1, dto.Id);
              Assert.Equal("Tag One", dto.Name);
              Assert.Equal("Test Suite", dto.CreatedBy);
              Assert.Equal(1, dto.ProjectId);
              Assert.Equal("Test Suite", dto.CreatedBy);
              Assert.Equal(now, dto.CreatedAt);
              Assert.Equal("modified@example.com", dto.ModifiedBy);
              Assert.Equal(now.AddDays(1), dto.ModifiedAt);
              Assert.Null(dto.ArchivedAt);
          }
          
         #endregion
    }
}