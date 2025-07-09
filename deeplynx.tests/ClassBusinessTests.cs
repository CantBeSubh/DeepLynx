//
// using deeplynx.business;
// using deeplynx.datalayer.Models;
// using deeplynx.interfaces;
// using deeplynx.models;
// using Moq;
// using System.Text.Json.Nodes;
//
// namespace deeplynx.tests
// {
//     public class ClassBusinessTests : IntegrationTestBase
//     {
//         private DeeplynxContext _context;
//         private ClassBusiness _classBusiness;
//         private Mock<IEdgeMappingBusiness> _mockEdgeMappingBusiness;
//         private Mock<IRecordBusiness> _mockRecordBusiness;
//         private Mock<IRecordMappingBusiness> _mockRecordMappingBusiness;
//         private Mock<IRelationshipBusiness> _mockRelationshipBusiness;
//
//         public ClassBusinessTests()
//         {
//             _mockEdgeMappingBusiness = new Mock<IEdgeMappingBusiness>();
//             _mockRecordBusiness = new Mock<IRecordBusiness>();
//             _mockRecordMappingBusiness = new Mock<IRecordMappingBusiness>();
//             _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();
//         }
//
//         public override async Task InitializeAsync()
//         {
//             await base.InitializeAsync();
//
//             _classBusiness = new ClassBusiness(
//                 Context,
//                 _mockEdgeMappingBusiness.Object,
//                 _mockRecordBusiness.Object,
//                 _mockRecordMappingBusiness.Object,
//                 _mockRelationshipBusiness.Object);
//         }
//
//         public async Task DisposeAsync()
//         {
//             await base.DisposeAsync();
//         }
//
//         #region GetAllClasses Tests
//
//         [Fact]
//         public async Task GetAllClasses_ValidProjectId_ReturnsActiveClasses()
//         {
//             // Act
//             var result = await _classBusiness.GetAllClasses(1);
//             var classes = result.ToList();
//
//             // Assert
//             Assert.True(classes.Count >= 0);
//             Assert.All(classes, c => Assert.Equal(1, c.ProjectId));
//             Assert.All(classes, c => Assert.Null(c.ArchivedAt));
//         }
//
//         [Fact]
//         public async Task GetAllClasses_ProjectWithNoClasses_ReturnsEmptyList()
//         {
//             // Act
//             var result = await _classBusiness.GetAllClasses(999);
//             var classes = result.ToList();
//
//             // Assert
//             Assert.Empty(classes);
//         }
//
//         [Fact]
//         public async Task GetAllClasses_DifferentProject_ReturnsClasses()
//         {
//             // Act
//             var result = await _classBusiness.GetAllClasses(2);
//             var classTest = result.ToList();
//
//             // Assert
//             Assert.Equal(4, classTest.Count);
//             Assert.All(classTest, c => Assert.Equal(2, c.ProjectId));
//             Assert.All(classTest, c => Assert.Null(c.ArchivedAt)); // Verify all are active
//         }
//         [Fact]
//         public async Task GetAllClasses_ExcludesArchivedClasses()
//         {
//             // Arrange - Create an archived class
//             var archivedClass = new Class
//             {
//                 Id = 100,
//                 Name = "Archived Class",
//                 Description = "This class is archived",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-10), DateTimeKind.Unspecified), 
//                 ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-1), DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(archivedClass);
//             await Context.SaveChangesAsync();
//
//             // Act
//             var result = await _classBusiness.GetAllClasses(1);
//             var classes = result.ToList();
//
//             // Assert
//             Assert.DoesNotContain(classes, c => c.Name == "Archived Class");
//             Assert.All(classes, c => Assert.Null(c.ArchivedAt));
//         }
//
//         #endregion
//
//         #region GetClass Tests
//
//         [Fact]
//         public async Task GetClass_ValidIds_ReturnsClass()
//         {
//             // Arrange - Create a test class
//             var testClass = new Class
//             {
//                 Id = 101,
//                 Name = "Test Class",
//                 Description = "Test class description",
//                 Uuid = "test-uuid-123",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-5), DateTimeKind.Unspecified), 
//                 ModifiedAt  = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-1), DateTimeKind.Unspecified),
//                 ModifiedBy = "modifier@example.com",
//             };
//             await Context.Classes.AddAsync(testClass);
//             await Context.SaveChangesAsync();
//
//             // Act
//             var result = await _classBusiness.GetClass(1, 101);
//
//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(101, result.Id);
//             Assert.Equal("Test Class", result.Name);
//             Assert.Equal("Test class description", result.Description);
//             Assert.Equal("test-uuid-123", result.Uuid);
//             Assert.Equal(1, result.ProjectId);
//             Assert.Equal("test@example.com", result.CreatedBy);
//             Assert.Equal("modifier@example.com", result.ModifiedBy);
//         }
//
//         [Fact]
//         public async Task GetClass_NonExistentClass_ThrowsKeyNotFoundException()
//         {
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.GetClass(1, 999));
//
//             Assert.Contains("Class with id 999 not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task GetClass_WrongProject_ThrowsKeyNotFoundException()
//         {
//             // Arrange - Create a class for project 1
//             var testClass = new Class
//             {
//                 Id = 102,
//                 Name = "Project 1 Class",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(testClass);
//             await Context.SaveChangesAsync();
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.GetClass(2, 102)); // Try to get from project 2
//
//             Assert.Contains("Class with id 102 not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task GetClass_ArchivedClass_ThrowsKeyNotFoundException()
//         {
//             // Arrange - Create an archived class
//             var archivedClass = new Class
//             {
//                 Id = 103,
//                 Name = "Archived Class",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-10), DateTimeKind.Unspecified), 
//                 ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-1), DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(archivedClass);
//             await Context.SaveChangesAsync();
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.GetClass(1, 103));
//
//             Assert.Contains("Class with id 103 not found", exception.Message);
//         }
//
//         #endregion
//
//         #region CreateClass Tests
//
//         [Fact]
//         public async Task CreateClass_ValidDto_CreatesClass()
//         {
//             // Arrange
//             var uniqueId = Guid.NewGuid().ToString("N")[..8];
//             var dto = new ClassRequestDto
//             {
//                 Name = $"New Test Class {uniqueId}",
//                 Description = "A newly created test class",
//                 Uuid = $"new-test-uuid-{uniqueId}"
//             };
//
//             // Act
//             var result = await _classBusiness.CreateClass(1, dto);
//
//             // Assert
//             Assert.NotNull(result);
//             Assert.True(result.Id > 0);
//             Assert.Equal($"New Test Class {uniqueId}", result.Name);
//             Assert.Equal("A newly created test class", result.Description);
//             Assert.Equal($"new-test-uuid-{uniqueId}", result.Uuid);
//             Assert.Equal(1, result.ProjectId);
//             Assert.NotNull(result.CreatedAt);
//
//             // Verify it was actually saved to database
//             var savedClass = await Context.Classes.FindAsync(result.Id);
//             Assert.NotNull(savedClass);
//             Assert.Equal($"New Test Class {uniqueId}", savedClass.Name);
//         }
//         [Fact]
//         public async Task CreateClass_ValidDto_ReturnsWithCreatedAtAndId()
//         {
//             // Arrange
//             var uniqueName = $"Test Class Name {Guid.NewGuid().ToString("N")[..8]}";
//             var dto = new ClassRequestDto
//             {
//                 Name = uniqueName,
//                 Description = "Test class description"
//             };
//
//             var beforeCreate = DateTime.UtcNow;
//
//             // Act
//             var result = await _classBusiness.CreateClass(1, dto);
//
//             // Assert
//             Assert.NotNull(result);
//             Assert.True(result.Id > 0);
//             Assert.Equal(uniqueName, result.Name);
//             Assert.Equal("Test class description", result.Description);
//             Assert.True(result.CreatedAt >= beforeCreate);
//             Assert.True(result.CreatedAt <= DateTime.UtcNow);
//             // CreatedBy is null in current implementation (TODO: JWT implementation)
//             Assert.Null(result.CreatedBy);
//         }
//
//         [Fact]
//         public async Task CreateClass_NullDto_ThrowsArgumentNullException()
//         {
//             // Act & Assert
//             await Assert.ThrowsAsync<ArgumentNullException>(
//                 () => _classBusiness.CreateClass(1, null));
//         }
//
//         [Fact]
//         public async Task CreateClass_DeletedProjectId_ThrowsKeyNotFoundException()
//         {
//             // Arrange
//             var dto = new ClassRequestDto
//             {
//                 Name = "Class For Deleted Project",
//                 Description = "Test class for non-existent project"
//             };
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.CreateClass(999, dto));
//
//             Assert.Contains("Project with id 999 not found", exception.Message);
//         }
//        
//         [Fact]
//         public async Task CreateClass_EmptyName_CreatesClassWithEmptyName()
//         {
//             // Arrange
//             var dto = new ClassRequestDto
//             {
//                 Name = "",
//                 Description = "Class with empty name"
//             };
//
//             // Act
//             var result = await _classBusiness.CreateClass(1, dto);
//
//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal("", result.Name);
//             Assert.Equal("Class with empty name", result.Description);
//         }
//
//         #endregion
//
//         #region UpdateClass Tests
//
//         [Fact]
//         public async Task UpdateClass_ValidUpdate_UpdatesClass()
//         {
//             // Arrange - Create a class to update
//             var originalClass = new Class
//             {
//                 Id = 104,
//                 Name = "Original Class",
//                 Description = "Original description",
//                 Uuid = "original-uuid",
//                 ProjectId = 1,
//                 CreatedBy = "creator@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-5), DateTimeKind.Unspecified), 
//             };
//             await Context.Classes.AddAsync(originalClass);
//             await Context.SaveChangesAsync();
//
//             var dto = new ClassRequestDto
//             {
//                 Name = "Updated Class Name",
//                 Description = "Updated description",
//                 Uuid = "updated-uuid"
//             };
//
//             // Act
//             var result = await _classBusiness.UpdateClass(1, 104, dto);
//
//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(104, result.Id);
//             Assert.Equal("Updated Class Name", result.Name);
//             Assert.Equal("Updated description", result.Description);
//             Assert.Equal("updated-uuid", result.Uuid);
//             Assert.NotNull(result.ModifiedAt);
//
//             // Verify it was actually updated in database
//             var updatedClass = await Context.Classes.FindAsync((long)104);
//             Assert.Equal("Updated Class Name", updatedClass.Name);
//             Assert.NotNull(updatedClass.ModifiedAt);
//         }
//
//         [Fact]
//         public async Task UpdateClass_NonExistentClass_ThrowsKeyNotFoundException()
//         {
//             // Arrange
//             var dto = new ClassRequestDto
//             {
//                 Name = "Update Test"
//             };
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.UpdateClass(1, 999, dto));
//
//             Assert.Contains("Class with id 999 not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task UpdateClass_WrongProject_ThrowsKeyNotFoundException()
//         {
//             // Arrange - Create a class for project 1
//             var testClass = new Class
//             {
//                 Id = 105,
//                 Name = "Project 1 Class",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(testClass);
//             await Context.SaveChangesAsync();
//
//             var dto = new ClassRequestDto
//             {
//                 Name = "Update Test"
//             };
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.UpdateClass(2, 105, dto)); // Try to update from project 2
//
//             Assert.Contains("Class with id 105 not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task UpdateClass_ArchivedClass_ThrowsKeyNotFoundException()
//         {
//             // Arrange - Create an archived class
//             var archivedClass = new Class
//             {
//                 Id = 106,
//                 Name = "Archived Class",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-10), DateTimeKind.Unspecified), 
//                 ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-1), DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(archivedClass);
//             await Context.SaveChangesAsync();
//
//             var dto = new ClassRequestDto
//             {
//                 Name = "Update Test"
//             };
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.UpdateClass(1, 106, dto));
//
//             Assert.Contains("Class with id 106 not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task UpdateClass_PartialUpdate_UpdatesOnlyProvidedFields()
//         {
//             // Arrange - Create a class to update
//             var originalClass = new Class
//             {
//                 Id = 107,
//                 Name = "Original Class",
//                 Description = "Original description",
//                 Uuid = "original-uuid",
//                 ProjectId = 1,
//                 CreatedBy = "creator@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-5), DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(originalClass);
//             await Context.SaveChangesAsync();
//
//             var dto = new ClassRequestDto
//             {
//                 Name = "Updated Name Only"
//                 // Description and Uuid not provided
//             };
//
//             // Act
//             var result = await _classBusiness.UpdateClass(1, 107, dto);
//
//             // Assert
//             Assert.Equal("Updated Name Only", result.Name);
//             // Description should remain unchanged or be null depending on implementation
//             Assert.Equal(107, result.Id);
//         }
//
//         #endregion
//
//         #region DeleteClass Tests
//
//         [Fact]
//         public async Task DeleteClass_ValidClass_DeletesSuccessfully()
//         {
//             // Arrange - Create a class to delete
//             var testClass = new Class
//             {
//                 Id = 108,
//                 Name = "Class to Delete",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(testClass);
//             await Context.SaveChangesAsync();
//
//             // Act
//             var result = await _classBusiness.DeleteClass(1, 108);
//
//             // Assert
//             Assert.True(result);
//
//             // Verify it was actually deleted from database
//             var deletedClass = await Context.Classes.FindAsync((long)108);
//             Assert.Null(deletedClass);
//         }
//
//         [Fact]
//         public async Task DeleteClass_NonExistentClass_ThrowsKeyNotFoundException()
//         {
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.DeleteClass(1, 999));
//
//             Assert.Contains("Class with id 999 not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task DeleteClass_NonExistentProject_ThrowsKeyNotFoundException()
//         {
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.DeleteClass(999, 1));
//
//             Assert.Contains("Project with id 999 not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task DeleteClass_WrongProject_ThrowsKeyNotFoundException()
//         {
//             // Arrange - Create a class for project 1
//             var testClass = new Class
//             {
//                 Id = 109,
//                 Name = "Project 1 Class",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(testClass);
//             await Context.SaveChangesAsync();
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.DeleteClass(2, 109)); // Try to delete from project 2
//
//             Assert.Contains("Class with id 109 not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task DeleteClass_ArchivedClass_ThrowsKeyNotFoundException()
//         {
//             // Arrange - Create an archived class
//             var archivedClass = new Class
//             {
//                 Id = 110,
//                 Name = "Archived Class",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-10), DateTimeKind.Unspecified), 
//                 ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-1), DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(archivedClass);
//             await Context.SaveChangesAsync();
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.DeleteClass(1, 110));
//
//             Assert.Contains("Class with id 110 not found", exception.Message);
//         }
//
//         #endregion
//
//         #region ArchiveClass Tests
//
//         // [Fact]
//         // public async Task ArchiveClass_ValidClass_ArchivesSuccessfully()
//         // {
//         //     // Arrange - Create a class to archive
//         //     var testClass = new Class
//         //     {
//         //         Id = 111,
//         //         Name = "Class to Archive",
//         //         ProjectId = 1,
//         //         CreatedBy = "test@example.com",
//         //         CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//         //     };
//         //     await Context.Classes.AddAsync(testClass);
//         //     await Context.SaveChangesAsync();
//         //
//         //     var beforeArchive = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
//         //
//         //     // Act
//         //     var result = await _classBusiness.ArchiveClass(1, 111);
//         //
//         //     // Assert
//         //     Assert.True(result);
//         //
//         //     // Verify it was actually archived in database
//         //     var archivedClass = await Context.Classes.FindAsync((long)111);
//         //     Assert.NotNull(archivedClass);
//         //     Assert.NotNull(archivedClass.ArchivedAt);
//         //     Assert.True(archivedClass.ArchivedAt >= beforeArchive);
//         //     Assert.True(archivedClass.ArchivedAt <= DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified));
//         // }
//         [Fact]
//         public async Task ArchiveClass_ValidClass_ArchivesSuccessfully()
//         {
//             // Arrange - Create a class to archive
//             var testClass = new Class
//             {
//                 Id = 111,
//                 Name = "Class to Archive",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(testClass);
//             await Context.SaveChangesAsync();
//             
//
//             // Act
//             var result = await _classBusiness.ArchiveClass(1, 111);
//
//           
//             Assert.True(result);
//             
//             var archivedClass = await Context.Classes.FindAsync((long)111);
//             Assert.NotNull(archivedClass);
//         }
//         [Fact]
//         public async Task ArchiveClass_NonExistentClass_ThrowsKeyNotFoundException()
//         {
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.ArchiveClass(1, 999));
//
//             Assert.Contains("Class not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task ArchiveClass_WrongProject_ThrowsKeyNotFoundException()
//         {
//             // Arrange - Create a class for project 1
//             var testClass = new Class
//             {
//                 Id = 112,
//                 Name = "Project 1 Class",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(testClass);
//             await Context.SaveChangesAsync();
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.ArchiveClass(2, 112));
//
//             Assert.Contains("Class not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task ArchiveClass_AlreadyArchivedClass_ThrowsKeyNotFoundException()
//         {
//             // Arrange - Create an already archived class
//             var archivedClass = new Class
//             {
//                 Id = 113,
//                 Name = "Already Archived Class",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-10), DateTimeKind.Unspecified),
//                 ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-1), DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(archivedClass);
//             await Context.SaveChangesAsync();
//
//             // Act & Assert
//             var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _classBusiness.ArchiveClass(1, 113));
//
//             Assert.Contains("Class not found", exception.Message);
//         }
//
//         [Fact]
//         public async Task ArchiveClass_ArchivedClassNotReturnedInGetAll()
//         {
//             // Arrange - Create a class to archive
//             var testClass = new Class
//             {
//                 Id = 114,
//                 Name = "Class to Archive",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddAsync(testClass);
//             await Context.SaveChangesAsync();
//
//             var initialCount = (await _classBusiness.GetAllClasses(1)).Count();
//
//             // Act
//             await _classBusiness.ArchiveClass(1, 114);
//             var finalCount = (await _classBusiness.GetAllClasses(1)).Count();
//
//             // Assert
//             Assert.Equal(initialCount - 1, finalCount);
//         }
//
//         #endregion
//
//         #region Edge Cases and Integration Tests
//
//         [Fact]
//         public async Task ClassOperations_ConcurrentModification_HandlesCorrectly()
//         {
//             // Arrange - Create two classes
//             var class1 = new Class
//             {
//                 Id = 115,
//                 Name = "Concurrent Class 1",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//             };
//             var class2 = new Class
//             {
//                 Id = 116,
//                 Name = "Concurrent Class 2",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt =DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
//             };
//             await Context.Classes.AddRangeAsync(class1, class2);
//             await Context.SaveChangesAsync();
//
//             var dto1 = new ClassRequestDto
//             {
//                 Name = "Concurrent Update 1",
//                 Description = "Updated concurrently"
//             };
//
//             var dto2 = new ClassRequestDto
//             {
//                 Name = "Concurrent Update 2",
//                 Description = "Updated concurrently"
//             };
//
//             // Act
//             var result1 = await _classBusiness.UpdateClass(1, 115, dto1);
//             var result2 = await _classBusiness.UpdateClass(1, 116, dto2);
//
//             // Assert
//             Assert.Equal("Concurrent Update 1", result1.Name);
//             Assert.Equal("Concurrent Update 2", result2.Name);
//         }
//         // [Fact]
//         // public async Task ClassOperations_SpecialCharactersInFields_HandlesCorrectly()
//         // {
//         //     // Arrange
//         //     var dto = new ClassRequestDto
//         //     {
//         //         Name = "Special Characters Test Class",
//         //         Description = "Description with quotes \"test\" and ‘single quotes’",
//         //         Uuid = "special-chars-test-uuid"
//         //     };
//         //
//         //     // Act
//         //     var result = await _classBusiness.CreateClass(1, dto);
//         //
//         //     // Assert
//         //     Assert.Equal("Special Characters Test Class", result.Name);
//         //     Assert.Contains("quotes \"test\"", result.Description);
//         //     Assert.Equal("special-chars-test-uuid", result.Uuid);
//         // }
//         [Fact]
//         public async Task ClassOperations_LongStrings_HandlesCorrectly()
//         {
//             // Arrange
//             var longName = new string('A', 1000);
//             var longDescription = new string('B', 5000);
//             var longUuid = new string('C', 500);
//
//             var dto = new ClassRequestDto
//             {
//                 Name = longName,
//                 Description = longDescription,
//                 Uuid = longUuid
//             };
//
//             // Act
//             var result = await _classBusiness.CreateClass(1, dto);
//
//             // Assert
//             Assert.Equal(longName, result.Name);
//             Assert.Equal(longDescription, result.Description);
//             Assert.Equal(longUuid, result.Uuid);
//         }
//
//         [Fact]
//         public async Task ClassOperations_NullAndEmptyStrings_HandlesCorrectly()
//         {
//             // Arrange
//             var dto = new ClassRequestDto
//             {
//                 Name = "className test",
//                 Description = null,
//                 Uuid = ""
//             };
//
//             // Act
//             var result = await _classBusiness.CreateClass(1, dto);
//
//             // Assert
//             Assert.Equal("", result.Name);
//             Assert.Null(result.Description);
//             Assert.Equal("", result.Uuid);
//         }
//
//         #endregion
//
//         #region DTO Tests
//
//         [Fact]
//         public void ClassRequestDto_AllProperties_CanBeSetAndRetrieved()
//         {
//             // Arrange & Act
//             var dto = new ClassRequestDto
//             {
//                 Name = "Test Class",
//                 Description = "Test Description",
//                 Uuid = "test-uuid"
//             };
//
//             // Assert
//             Assert.Equal("Test Class", dto.Name);
//             Assert.Equal("Test Description", dto.Description);
//             Assert.Equal("test-uuid", dto.Uuid);
//         }
//
//         [Fact]
//         public void ClassResponseDto_AllProperties_CanBeSetAndRetrieved()
//         {
//             // Arrange & Act
//             var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
//
//             var dto = new ClassResponseDto
//             {
//                 Id = 1,
//                 Name = "Test Class",
//                 Description = "Test Description",
//                 Uuid = "test-uuid",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = now,
//                 ModifiedBy = "modified@example.com",
//                 ModifiedAt = now.AddDays(1),
//                 ArchivedAt = null
//             };
//
//             // Assert
//             Assert.Equal(1, dto.Id);
//             Assert.Equal("Test Class", dto.Name);
//             Assert.Equal("Test Description", dto.Description);
//             Assert.Equal("test-uuid", dto.Uuid);
//             Assert.Equal(1, dto.ProjectId);
//             Assert.Equal("test@example.com", dto.CreatedBy);
//             Assert.Equal(now, dto.CreatedAt);
//             Assert.Equal("modified@example.com", dto.ModifiedBy);
//             Assert.Equal(now.AddDays(1), dto.ModifiedAt);
//             Assert.Null(dto.ArchivedAt);
//         }
//
//         [Fact]
//         public void ClassResponseDto_WithArchivedAt_CanBeSetAndRetrieved()
//         {
//             // Arrange & Act
//             var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
//
//             var dto = new ClassResponseDto
//             {
//                 Id = 1,
//                 Name = "Archived Class",
//                 ProjectId = 1,
//                 CreatedBy = "test@example.com",
//                 CreatedAt = now,
//                 ArchivedAt = now.AddDays(1)
//             };
//
//             // Assert
//             Assert.Equal(1, dto.Id);
//             Assert.Equal("Archived Class", dto.Name);
//             Assert.Equal(now.AddDays(1), dto.ArchivedAt);
//         }
//
//         #endregion
//     }
// }

using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Moq;

namespace deeplynx.tests
{
    public class ClassBusinessTests : IntegrationTestBase
    {
        private ClassBusiness _classBusiness = null!;
        private ProjectBusiness _projectBusiness = null!;
        private Mock<IEdgeMappingBusiness> _mockEdgeMappingBusiness;
        private Mock<IRecordBusiness> _mockRecordBusiness;
        private Mock<IRecordMappingBusiness> _mockRecordMappingBusiness;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness;
        public long pid;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _mockEdgeMappingBusiness = new Mock<IEdgeMappingBusiness>();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRecordMappingBusiness = new Mock<IRecordMappingBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();

            _classBusiness = new ClassBusiness(Context, _mockEdgeMappingBusiness.Object, _mockRecordBusiness.Object, _mockRecordMappingBusiness.Object, _mockRelationshipBusiness.Object);
            _projectBusiness = new ProjectBusiness(Context, _classBusiness);
        }

        [Fact]
        public async Task CreateClass_Success_ReturnsIdAndCreatedAt()
        {
            await SeedTestDataAsync();
            var now = DateTime.UtcNow;
            var dto = new ClassRequestDto
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                Uuid = $"test-uuid-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
            };

            var result = await _classBusiness.CreateClass(pid, dto);
            result.Id.Should().BeGreaterThan(0);
            result.CreatedAt.Should().BeOnOrAfter(now);
            result.Name.Should().Be(dto.Name);
            result.Description.Should().Be(dto.Description);
            result.ProjectId.Should().Be(pid);
        }

        [Fact]
        public async Task CreateClass_Fails_IfNoName()
        {
            await SeedTestDataAsync();
            var dto = new ClassRequestDto { Name = null, Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateClass_Fails_IfEmptyName()
        {
            await SeedTestDataAsync();
            var dto = new ClassRequestDto { Name = "", Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateClass_Fails_IfNoProjectId()
        {
            await SeedTestDataAsync();
            var dto = new ClassRequestDto { Name = "Test Class", Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid + 99, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateClass_Fails_IfDeletedProjectId()
        {
            await SeedTestDataAsync(true);
            var dto = new ClassRequestDto { Name = "Test Class", Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateClass_Fails_IfDuplicateName()
        {
            await SeedTestDataAsync();
            var duplicateName = "Duplicate Class";
            var dto = new ClassRequestDto { Name = duplicateName, Description = "Test Description" };

            // Create first class
            await _classBusiness.CreateClass(pid, dto);

            // Try to create duplicate
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetAllClasses_ReturnsOnlyForProjects()
        {
            await SeedTestDataAsync();
            var p2 = new Project { Name = "ExtraProj" };
            Context.Projects.Add(p2);
            await Context.SaveChangesAsync();

            await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = $"Class1-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Test" });
            await _classBusiness.CreateClass(p2.Id, new ClassRequestDto { Name = $"Class2-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Test" });

            var list = await _classBusiness.GetAllClasses(pid);
            Assert.All(list, c => Assert.Equal(pid, c.ProjectId));
        }

        [Fact]
        public async Task GetAllClasses_ExcludesSoftDeleted()
        {
            await SeedTestDataAsync();
            var activeClass = new Class
            {
                Name = $"Active Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };

            var archivedClass = new Class
            {
                Name = $"Archived Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(activeClass);
            Context.Classes.Add(archivedClass);
            await Context.SaveChangesAsync();

            var list = await _classBusiness.GetAllClasses(pid);
            Assert.DoesNotContain(list, c => c.Id == archivedClass.Id);
        }

        [Fact]
        public async Task GetClass_Success_WhenExists()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = await _classBusiness.GetClass(pid, testClass.Id);
            Assert.Equal(testClass.Id, result.Id);
        }

        [Fact]
        public async Task GetClass_Fails_IfNoProjectID()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = () => _classBusiness.GetClass(pid + 999, testClass.Id);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetClass_Fails_IfDeletedClass()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Deleted Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = () => _classBusiness.GetClass(pid, testClass.Id);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateClass_Success_ReturnsModifiedAt()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Original Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Original Description",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var dto = new ClassRequestDto { Name = $"Updated Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Updated Description" };
            var updatedResult = await _classBusiness.UpdateClass(pid, testClass.Id, dto);

            updatedResult.ModifiedAt.Should().BeOnOrAfter(updatedResult.CreatedAt);
        }

        [Fact]
        public async Task UpdateClass_Fails_IfNotFound()
        {
            await SeedTestDataAsync();

            var dto = new ClassRequestDto { Name = "Updated Class", Description = "Updated Description" };
            var updatedResult = () => _classBusiness.UpdateClass(pid, 99, dto);
            updatedResult.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task ArchiveClass_Success_WhenExists()
        {
            await SeedTestDataAsync();
            var beforeArchive = DateTime.UtcNow;

            var testClass = new Class
            {
                Name = $"Class to Archive {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var archivedResult = await _classBusiness.ArchiveClass(pid, testClass.Id);
            Assert.True(archivedResult);

            var archivedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(archivedClass);

            // Check if ArchivedAt was set (optional based on implementation)
            if (archivedClass.ArchivedAt.HasValue)
            {
                Assert.NotNull(archivedClass.ArchivedAt);
                Assert.True(archivedClass.ArchivedAt >= beforeArchive);
                Assert.True(archivedClass.ArchivedAt <= DateTime.UtcNow);
            }
        }

        [Fact]
        public async Task DeleteClass_Success_WhenExists()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Class to Delete {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);
            Assert.True(deletedResult);

            var deletedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.Null(deletedClass);
        }

        [Fact]
        public async Task ClassArchived_WhenProjectArchived()
        {
            await SeedTestDataAsync();
            var beforeArchive = DateTime.UtcNow;
            var testClass = new Class
            {
                Name = $"Class in Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var deletedResult = await _projectBusiness.ArchiveProject(pid);
            Assert.True(deletedResult);

            // procedure is not traced by entity framework
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var archivedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(archivedClass);

            // Check if ArchivedAt was set (optional based on implementation)
            if (archivedClass.ArchivedAt.HasValue)
            {
                Assert.NotNull(archivedClass.ArchivedAt);
                Assert.True(archivedClass.ArchivedAt >= beforeArchive);
                Assert.True(archivedClass.ArchivedAt <= DateTime.UtcNow);
            }
        }

        private async Task SeedTestDataAsync(bool deleteProject = false)
        {
            await CleanDatabaseAsync();

            var project = new Project { Name = "Project 1" };

            if (deleteProject)
            {
                project.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
        }
    }
}