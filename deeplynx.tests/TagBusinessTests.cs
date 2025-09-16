using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Moq;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class TagBusinessTests : IntegrationTestBase
    {
        private DeeplynxContext _context;
        private EventBusiness _eventBusiness;
        private TagBusiness _tagBusiness;
        public long pid;
        public long pid2;
        public long pid3;
        public long tid;
        public long tid2;
        public long tid3;
        public long tid4;

        public TagBusinessTests(TestSuiteFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _eventBusiness = new EventBusiness(Context);

            _tagBusiness = new TagBusiness( 
                Context,
                _eventBusiness);
        }

        #region GetAllTags Tests

        [Fact]
        public async Task GetAllTags_ValidProjectId_ReturnsActiveTags()
        {
            // Act
            var result = await _tagBusiness.GetAllTags(pid, true);
            var tags = result.ToList();

            // Assert
            Assert.Equal(2, tags.Count);
            Assert.All(tags, t => Assert.Equal(pid, t.ProjectId));
            Assert.All(tags, t => Assert.Equal(false, t.IsArchived));
            Assert.Contains(tags, t => t.Id == tid);
            Assert.Contains(tags, t => t.Id == tid2);
            Assert.DoesNotContain(tags, t => t.Id == tid3);
            Assert.DoesNotContain(tags, t => t.Id == tid4);
        }

        [Fact]
        public async Task GetAllTags_ProjectWithNoTags_ReturnsEmptyList()
        {
            // Act
            var result = await _tagBusiness.GetAllTags(pid3, true);
            var tags = result.ToList();

            // Assert
            Assert.Empty(tags);
        }

        [Fact]
        public async Task GetAllTags_DifferentProject_ReturnsCorrectTags()
        {
            // Act
            var result = await _tagBusiness.GetAllTags(pid, true);
            var tags = result.ToList();

            // Assert
            Assert.Equal(2, tags.Count);
            Assert.All(tags, ds => Assert.Equal(pid, ds.ProjectId));
            Assert.Equal(pid, tags.First().ProjectId);
        }

        #endregion
        
        #region GetTag Tests

        [Fact]
        public async Task GetTag_ValidIds_ReturnsTag()
        {
            // Act
            var result = await _tagBusiness.GetTag(pid, tid, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tid, result.Id);
            Assert.Equal("Analytics", result.Name);
            Assert.Equal("john.smith@company.com", result.LastUpdatedBy);
            Assert.False( result.IsArchived);
            Assert.Equal(pid, result.ProjectId);
        }

        [Fact]
        public async Task GetTag_NonExistentTag_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.GetTag(pid, 999, false));
            
            Assert.Contains("Tag with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task GetTag_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.GetTag(pid, tid4, false)); // Tag 1 belongs to project 1, not 2
            
            Assert.Contains($"Tag with id {tid4} not found", exception.Message);
        }

        [Fact]
        public async Task GetTag_ArchivedTag_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.GetTag(pid, tid3, true)); // Tag 3 of project 1 is archived
            
            Assert.Contains($"Tag with id {tid3} is archived", exception.Message);
        }

        #endregion

        #region CreateTag Tests

        [Fact]
        public async Task CreateTag_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _tagBusiness.CreateTag(pid, null));
            
            // Ensure that the Tag create event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateTag_ValidDto_CreatesTag()
        {
            // Arrange
            var dto = new CreateTagRequestDto
            {
                Name = "Tag One"
            };

            // Act
            var result = await _tagBusiness.CreateTag(pid, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Tag One", result.Name);
            Assert.Equal(pid, result.ProjectId);

            // Verify it was actually saved to database
            var savedTag = await Context.Tags.FindAsync(result.Id);
            Assert.NotNull(savedTag);
            Assert.Equal("Tag One", savedTag.Name);
            
            // Ensure that the Tag create event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = result.ProjectId,
                Operation = "create",
                EntityType = "tag",
                EntityId = result.Id,
            });
        }

        [Fact]
        public async Task CreateTag_SetsCreatedAtAndCreatedBy()
        {
            // Arrange
            var dto = new CreateTagRequestDto
            {
                Name = "Tag Timestamp Test"
            };

            var beforeCreate = DateTime.UtcNow;

            // Act
            var result = await _tagBusiness.CreateTag(pid, dto);

            // Assert
            Assert.True(result.LastUpdatedAt <= DateTime.UtcNow);

            // Ensure that the Tag create event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = result.ProjectId,
                Operation = "create",
                EntityType = "tag",
                EntityId = result.Id,
            });
        }

        [Fact]
        public async Task CreateTag_Success_OnBulkCreate()
        {
            var tags = new List<CreateTagRequestDto>
            {
                new CreateTagRequestDto
                {
                    Name = "Test Tag 1"
                },
                new CreateTagRequestDto
                {
                    Name = "Test Tag 2"
                }
            };
            
            var result = await _tagBusiness.BulkCreateTags(pid, tags);
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Test Tag 1");
            result.Last().Name.Should().Be("Test Tag 2");
            
            // Ensure that create event was logged for each created tag
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(2);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "create",
                EntityType = "tag",
                ProjectId = result[0].ProjectId,
                EntityId = result[0].Id,
            });
            eventList[1].Should().BeEquivalentTo(new
            {
                Operation = "create",
                EntityType = "tag",
                ProjectId = result[1].ProjectId,
                EntityId = result[1].Id,
            });
        }
                
        [Fact]
        public async Task CreateTagRequest_Fails_IfNoName()
        { 
            var dto = new CreateTagRequestDto() { Name = null };
            var result = () => _tagBusiness.CreateTag(pid, dto);
            await result.Should().ThrowAsync<ValidationException>();
            
            // Ensure that no tag create event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        #endregion
        
        #region UpdateTag Tests

        [Fact]
        public async Task UpdateTag_ValidUpdate_UpdatesTag()
        {
            // Arrange
            var dto = new UpdateTagRequestDto
            {
                Name = "Updated Test Tag"
            };

            // Act
            var result = await _tagBusiness.UpdateTag(pid, tid, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tid, result.Id);
            Assert.Equal("Updated Test Tag", result.Name);
            Assert.True(result.LastUpdatedAt <= DateTime.UtcNow);

            // Verify it was actually updated in database
            var updatedTag = await Context.Tags.FindAsync(tid);
            Assert.Equal("Updated Test Tag", updatedTag?.Name);
            Assert.NotNull(updatedTag?.LastUpdatedAt);
             // Ensure that the tag update event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = result.ProjectId,
                Operation = "update",
                EntityType = "tag",
                EntityId = result.Id,
            });
        }

        [Fact]
        public async Task UpdateTag_PartialUpdate_UpdatesTag()
        {
            // Arrange
            var originalTag = new Tag
            {
                Name = "Original Tag",
                ProjectId = pid,
                LastUpdatedBy = "john.smith@company.com",
            };

            Context.Tags.Add(originalTag);
            await Context.SaveChangesAsync();

            var updateDto = new UpdateTagRequestDto
            {
                Name = "Updated Tag"
            };

            // Act
            var result = await _tagBusiness.UpdateTag(pid, originalTag.Id, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(originalTag.Id, result.Id);
            Assert.Equal("Updated Tag", result.Name);
            Assert.Equal(originalTag.LastUpdatedBy, result.LastUpdatedBy);

            // Verify it was actually updated in database
            var updatedTag = await Context.Tags.FindAsync(originalTag.Id);
            Assert.NotNull(updatedTag);
            Assert.Equal("Updated Tag", updatedTag.Name);
            Assert.NotNull(updatedTag.LastUpdatedAt);
            
            // Ensure that the tag update event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = result.ProjectId,
                Operation = "update",
                EntityType = "tag",
                EntityId = result.Id,
            });
        }

        [Fact]
        public async Task UpdateTag_NonExistentTag_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new UpdateTagRequestDto
            {
                Name = "Update Test Tag"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.UpdateTag(pid, 999, dto));

            Assert.Contains("Tag with id 999 not found", exception.Message);
            
            // Ensure that the update tag event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task UpdateTag_WrongProject_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new UpdateTagRequestDto
            {
                Name = "Update Test"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.UpdateTag(pid2, tid, dto)); // Tag 1 belongs to project 2

            Assert.Contains($"Tag with id {tid} not found", exception.Message);
            
            // Ensure that the update tag event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task UpdateTag_ArchivedTag_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new UpdateTagRequestDto
            {
                Name = "Update Test"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.UpdateTag(pid, tid3, dto)); // Tag 3 is archived

            Assert.Contains($"Tag with id {tid3} not found", exception.Message);
            
            // Ensure that the update tag event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        #endregion
        
        #region DeleteTag Tests

        [Fact]
        public async Task DeleteTag_ValidTag_DeletesSuccessfully()
        {
            // Act
            var result = await _tagBusiness.DeleteTag(pid, tid);

            // Assert
            Assert.True(result);

            // Verify it was actually deleted from database
            var deletedTag = await Context.Tags.FindAsync(tid);
            Assert.Null(deletedTag);
        }

        [Fact]
        public async Task DeleteTag_NonExistentTag_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.DeleteTag(pid, 999));

            Assert.Contains("Tag with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task DeleteTag_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _tagBusiness.DeleteTag(pid2, tid)); // Tag 1 belongs to project 1

            Assert.Contains($"Tag with id {tid} not found", exception.Message);
        }

        #endregion
        
         #region ArchiveTag Tests

         [Fact]
         public async Task ArchiveTag_ValidTag_ArchivesSuccessfully()
         {
             // Arrange
             var beforeArchive = DateTime.UtcNow;

             // Act
             var result = await _tagBusiness.ArchiveTag(pid, tid);

             // Assert
             Assert.True(result);

             // Verify it was actually archived in database
             var archivedTag = await Context.Tags.FindAsync(tid);
             Assert.NotNull(archivedTag);
             Assert.True(archivedTag.IsArchived);
             
             // Ensure that the tag delete event was logged
             var eventList = Context.Events.ToList();
             eventList.Count.Should().Be(1);
             eventList[0].Should().BeEquivalentTo(new
             {
                 ProjectId = pid,
                 Operation = "delete",
                 EntityType = "tag",
             });
         }

         [Fact]
         public async Task ArchiveTag_NonExistentTag_ThrowsKeyNotFoundException()
         {
             // Act & Assert
             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                 () => _tagBusiness.ArchiveTag(pid, 999));

             Assert.Contains("Tag with id 999 not found", exception.Message);
             
             // Ensure that no tag delete event was logged
             var eventList = Context.Events.ToList();
             eventList.Count.Should().Be(0);
         }

         [Fact]
         public async Task ArchiveTag_WrongProject_ThrowsKeyNotFoundException()
         {
             // Act & Assert
             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                 () => _tagBusiness.ArchiveTag(pid2, tid));

             Assert.Contains($"Tag with id {tid} not found", exception.Message);
             
             // Ensure that no tag delete event was logged
             var eventList = Context.Events.ToList();
             eventList.Count.Should().Be(0);
         }

         [Fact]
         public async Task ArchiveTag_AlreadyArchivedTag_ThrowsKeyNotFoundException()
         {
             // Act & Assert
             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                 () => _tagBusiness.ArchiveTag(pid, tid3)); // Tag 3 is already archived

             Assert.Contains($"Tag with id {tid3} not found", exception.Message);
             
             // Ensure that no tag delete event was logged
             var eventList = Context.Events.ToList();
             eventList.Count.Should().Be(0);
         }

         [Fact]
         public async Task ArchiveTag_ArchivedTagNotReturnedInGetAll()
         {
             // Arrange
             var initialCount = (await _tagBusiness.GetAllTags(pid, true)).Count();

             // Act
             await _tagBusiness.ArchiveTag(pid, tid);
             var finalCount = (await _tagBusiness.GetAllTags(pid, true)).Count();

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
             var dto1 = new UpdateTagRequestDto
             {
                 Name = "Concurrent Tag Update 1"
             };

             var dto2 = new UpdateTagRequestDto
             {
                 Name = "Concurrent Tag Update 2"
             };

             // Act
             var task1 = await _tagBusiness.UpdateTag(pid, tid, dto1);
             var task2 = await _tagBusiness.UpdateTag(pid, tid2, dto2);

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
             var dto = new CreateTagRequestDto
             {
                 Name = "Test with émojis 🚀 and ñ special chars 中文"
             };

             // Act
             var result = await _tagBusiness.CreateTag(pid, dto);

             // Assert
             Assert.Equal("Test with émojis 🚀 and ñ special chars 中文", result.Name);
         }

          [Fact]
          public void TagRequestDto_AllProperties_CanBeSetAndRetrieved()
          {
              // Arrange & Act
              var dto = new CreateTagRequestDto
              {
                  Name = "Tag One"
              };

              // Assert
              Assert.Equal("Tag One", dto.Name);
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
                  LastUpdatedBy= "Test Suite",
                  ProjectId = 1,
                  LastUpdatedAt = now,
                  IsArchived = false
              };

              // Assert
              Assert.Equal(1, dto.Id);
              Assert.Equal("Tag One", dto.Name);
              Assert.Equal("Test Suite", dto.LastUpdatedBy);
              Assert.Equal(1, dto.ProjectId);
              Assert.Equal("Test Suite", dto.LastUpdatedBy);
              Assert.Equal(now, dto.LastUpdatedAt);
              Assert.False(dto.IsArchived);
          }
          
         #endregion

         #region UnarchiveTag Tests

         [Fact]
         public async Task UnarchiveTag_ValidArchivedTag_UnarchivesSuccessfully()
         {
             var archivedTag = new Tag
             {
                 Name = "Archived Tag",
                 ProjectId = pid,
                 LastUpdatedBy = "Test Suite",
                 LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-5), DateTimeKind.Unspecified),
                 IsArchived = true
             };
             await Context.Tags.AddAsync(archivedTag);
             await Context.SaveChangesAsync();

             var tagId = archivedTag.Id;

             var result = await _tagBusiness.UnarchiveTag(pid, tagId);

             Assert.True(result);
             Context.ChangeTracker.Clear();
             var refreshed = await Context.Tags.FindAsync(tagId);
             Assert.NotNull(refreshed);
             Assert.False(refreshed.IsArchived);
         }

         [Fact]
         public async Task UnarchiveTag_NonExistentTag_ThrowsKeyNotFoundException()
         {
             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                 () => _tagBusiness.UnarchiveTag(pid, 99999));

             Assert.Contains("Tag with id 99999 not found", exception.Message);
         }

         [Fact]
         public async Task UnarchiveTag_WrongProject_ThrowsKeyNotFoundException()
         {
             // tid4 is archived and belongs to pid2
             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                 () => _tagBusiness.UnarchiveTag(pid, tid4)); // Calling with pid (wrong project)

             Assert.Contains($"Tag with id {tid4} not found", exception.Message);
         }

         [Fact]
         public async Task UnarchiveTag_AlreadyActive_ThrowsKeyNotFoundException()
         {
             // tid is already active
             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                 () => _tagBusiness.UnarchiveTag(pid, tid));

             Assert.Contains($"Tag with id {tid} not found", exception.Message);
         }

         #endregion
         
         protected override async Task SeedTestDataAsync()
         {
             await base.SeedTestDataAsync();
             var project = new Project { Name = "Project 1" };
             var project2 = new Project { Name = "Project2" };
             var project3 = new Project { Name = "Project 3" };
             Context.Projects.Add(project);
             Context.Projects.Add(project2);
             Context.Projects.Add(project3);
        
             await Context.SaveChangesAsync();
             pid = project.Id;
             pid2 = project2.Id;
             pid3 = project3.Id;
             
             var tag = new Tag
             {
                 Name = "Analytics", ProjectId = pid, LastUpdatedBy = "john.smith@company.com",
                 LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12), 
                 IsArchived = false
             };
             
             var tag2 = new Tag
             {
                 Name = "Analytics 2", ProjectId = pid, LastUpdatedBy = "john.smith@company.com",
                 LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
                 IsArchived = false
             };
             var tag3 = new Tag
             {
                 Name = "Analytics 3", ProjectId = pid, LastUpdatedBy = "john.smith@company.com",
                 LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
                 IsArchived = true
             };
             var tag4 = new Tag
             {
                 Name = "Analytics 4", ProjectId = pid2, LastUpdatedBy = "john.smith@company.com",
                 LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
                 IsArchived = false
             };
             await Context.Tags.AddAsync(tag);
             await Context.Tags.AddAsync(tag2);
             await Context.Tags.AddAsync(tag3);
             await Context.Tags.AddAsync(tag4);
             await Context.SaveChangesAsync();
             tid =  tag.Id;
             tid2 = tag2.Id;
             tid3 = tag3.Id;
             tid4 = tag4.Id;
             
         }
    }
}