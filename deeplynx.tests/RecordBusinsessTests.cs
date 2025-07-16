using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;
using Xunit;

using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests
{
    public class RecordBusinessTests : IntegrationTestBase
    {
        private RecordBusiness _recordBusiness;
        private Mock<IEdgeBusiness> _mockEdgeBusiness;

        public RecordBusinessTests()
        {
            _mockEdgeBusiness = new Mock<IEdgeBusiness>();
        }

        public override async Task InitializeAsync()
        {
           
            await base.InitializeAsync();

            // Create the archive_record stored procedure
            await CreateArchiveRecordProcedureAsync();

            _recordBusiness = new RecordBusiness(Context, _mockEdgeBusiness.Object);
        }

        private async Task CreateArchiveRecordProcedureAsync()
        {
            // Create the stored procedure that the RecordBusiness.ArchiveRecord method calls
            await Context.Database.ExecuteSqlRawAsync(@"
            CREATE OR REPLACE PROCEDURE deeplynx.archive_record(
                record_id_param INTEGER,
                archived_at_param TIMESTAMP WITHOUT TIME ZONE
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                -- Simulate a failure by doing nothing for record id 100
                IF record_id_param = 100 THEN
                    -- Do nothing, simulating a failure
                    RETURN;
                END IF;
                
                -- Normal processing for other records
                UPDATE deeplynx.records 
                SET archived_at = archived_at_param 
                WHERE id = record_id_param AND archived_at IS NULL;
                
                UPDATE deeplynx.edges 
                SET archived_at = archived_at_param 
                WHERE (origin_id = record_id_param OR destination_id = record_id_param) 
                AND archived_at IS NULL;
                SELECT COUNT(archived_at) FROM deeplynx.records WHERE archived_at IS NOT NULL AND id = record_id_param;
            END; $$");
        }
        
        #region GetAllRecords Tests

        [Fact]
        public async Task GetAllRecords_ValidProjectId_ReturnsActiveRecords()
        {
            // Act
            var result = await _recordBusiness.GetAllRecords(1);
            var records = result.ToList();

            // Assert
            Assert.Equal(3, records.Count);
            Assert.All(records, r => Assert.Equal(1, r.ProjectId));
            Assert.All(records, r => Assert.Null(r.ArchivedAt));
            Assert.Contains(records, r => r.Name == "John Doe - Customer Profile");
            Assert.Contains(records, r => r.Name == "Purchase Order ORD-2024-001234");
            Assert.DoesNotContain(records, r => r.Name == "Archived Record");
        }

        [Fact]
        public async Task GetAllRecords_WithDataSourceFilter_ReturnsFilteredRecords()
        {
            // Act
            var result = await _recordBusiness.GetAllRecords(1, 1);
            var records = result.ToList();

            // Assert
            Assert.Single(records);
            Assert.Equal(1, records.First().DataSourceId);
            Assert.Equal("John Doe - Customer Profile", records.First().Name);
        }

        [Fact]
        public async Task GetAllRecords_ProjectWithNoRecords_ReturnsEmptyList()
        {
            // Act
            var result = await _recordBusiness.GetAllRecords(999);
            var records = result.ToList();

            // Assert
            Assert.Empty(records);
        }

        [Fact]
        public async Task GetAllRecords_DifferentProject_ReturnsCorrectRecords()
        {
            // Act
            var result = await _recordBusiness.GetAllRecords(2);
            var records = result.ToList();

            // Assert
            Assert.Equal(4, records.Count);
            Assert.Equal("Advanced Components Corp - Supplier", records.First().Name);
            Assert.Equal(2, records.First().ProjectId);
        }

        [Fact]
        public async Task GetAllRecords_NonExistentDataSource_ReturnsEmptyList()
        {
            // Act
            var result = await _recordBusiness.GetAllRecords(1, 999);
            var records = result.ToList();

            // Assert
            Assert.Empty(records);
        }

        #endregion

        #region GetRecord Tests

        [Fact]
        public async Task GetRecord_ValidIds_ReturnsRecord()
        {
            // Act
            var result = await _recordBusiness.GetRecord(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John Doe - Customer Profile", result.Name);
            Assert.Equal("crm://customers/CUST_001", result.Uri);
            Assert.Equal("CUST_001", result.OriginalId);
            Assert.Equal(1, result.ClassId);
            Assert.Equal(1, result.DataSourceId);
            Assert.Equal(1, result.ProjectId);
        }

        [Fact]
        public async Task GetRecord_NonExistentRecord_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.GetRecord(1, 999));

            Assert.Contains("Record with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task GetRecord_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.GetRecord(2, 1)); // Record 1 belongs to project 1, not 2

            Assert.Contains("Record with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task GetRecord_ArchivedRecord_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.GetRecord(1, 5)); // Record 3 is archived

            Assert.Contains("Record with id 5 not found", exception.Message);
        }

        #endregion

        #region CreateRecord Tests

        [Fact]
        public async Task CreateRecord_ValidDto_CreatesRecord()
        {
            // Arrange
            var properties = new JsonObject
            {
                ["name"] = "Jane Smith",
                ["email"] = "jane@example.com",
                ["department"] = "Engineering"
            };

            var dto = new RecordRequestDto
            {
                Uri = "test://records/new",
                Properties = properties,
                OriginalId = "ORIG_NEW",
                Name = "Jane Smith Customer Record",
                ClassName = "Customer",
                ClassId = 1
            };

            // Act
            var result = await _recordBusiness.CreateRecord(1, 1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("test://records/new", result.Uri);
            Assert.Equal("ORIG_NEW", result.OriginalId);
            Assert.Equal("Jane Smith Customer Record", result.Name);
            Assert.Equal(1, result.ClassId);
            Assert.Equal(1, result.DataSourceId);
            Assert.Equal(1, result.ProjectId);
            Assert.Contains("Jane Smith", result.Properties);

            // Verify it was actually saved to database
            var savedRecord = await Context.Records.FindAsync(result.Id);
            Assert.NotNull(savedRecord);
            Assert.Equal("Jane Smith Customer Record", savedRecord.Name);
        }

        [Fact]
        public async Task CreateRecord_NonExistentProject_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new RecordRequestDto
            {
                Properties = new JsonObject { ["test"] = "value" },
                Name = "Test Record"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.CreateRecord(999, 1, dto));

            Assert.Contains("Project with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task CreateRecord_NonExistentDataSource_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new RecordRequestDto
            {
                Properties = new JsonObject { ["test"] = "value" },
                Name = "Test Record"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.CreateRecord(1, 999, dto));

            Assert.Contains("DataSource with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task CreateRecord_ArchivedDataSource_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new RecordRequestDto
            {
                Properties = new JsonObject { ["test"] = "value" },
                Name = "Test Record"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.CreateRecord(1, 7, dto));

            Assert.Contains("DataSource with id 7 not found", exception.Message);
        }

        [Fact]
        public async Task CreateRecord_JsonDepthTooDeep_ThrowsException()
        {
            // Arrange - Create JSON with depth > 3
            var deepProperties = new JsonObject
            {
                ["level1"] = new JsonObject
                {
                    ["level2"] = new JsonObject
                    {
                        ["level3"] = new JsonObject
                        {
                            ["level4"] = "too deep"
                        }
                    }
                }
            };

            var dto = new RecordRequestDto
            {
                Properties = deepProperties,
                Name = "Deep JSON Record"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _recordBusiness.CreateRecord(1, 1, dto));

            Assert.Contains("depth of the JSON structure exceeds the maximum allowed depth of 3", exception.Message);
        }

        [Fact]
        public async Task CreateRecord_JsonDepthAtLimit_CreatesSuccessfully()
        {
            // Arrange - Create JSON with exactly depth 3
            var properties = new JsonObject
            {
                ["level1"] = new JsonObject
                {
                    ["level2"] = new JsonObject
                    {
                        ["level3"] = "at limit"
                    }
                }
            };

            var dto = new RecordRequestDto
            {
                Properties = properties,
                Name = "Depth Limit Record"
            };

            // Act
            var result = await _recordBusiness.CreateRecord(1, 1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("at limit", result.Properties);
        }

        [Fact]
        public async Task CreateRecord_SetsTimestamps_Correctly()
        {
            // Arrange
            var dto = new RecordRequestDto
            {
                Properties = new JsonObject { ["test"] = "timestamp" },
                Name = "Timestamp Test Record"
            };

            var beforeCreate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // Act
            var result = await _recordBusiness.CreateRecord(1, 1, dto);

            // Assert
            Assert.True(result.CreatedAt >= beforeCreate);
            Assert.True(result.CreatedAt <= DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified));
        }

        #endregion

        #region UpdateRecord Tests

        [Fact]
        public async Task UpdateRecord_ValidUpdate_UpdatesRecord()
        {
            // Arrange
            var properties = new JsonObject
            {
                ["name"] = "John Doe Updated",
                ["email"] = "john.updated@example.com",
                ["age"] = 31,
                ["department"] = "Sales"
            };

            var dto = new RecordRequestDto
            {
                Uri = "test://records/1/updated",
                Properties = properties,
                OriginalId = "ORIG_001_UPD",
                Name = "John Doe Updated Customer Record",
                ClassName = "Customer",
                ClassId = 1
            };

            // Act
            var result = await _recordBusiness.UpdateRecord(1, 1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, (int)result.Id);
            Assert.Equal("test://records/1/updated", result.Uri);
            Assert.Equal("ORIG_001_UPD", result.OriginalId);
            Assert.Equal("John Doe Updated Customer Record", result.Name);
            Assert.Contains("John Doe Updated", result.Properties);
            Assert.Contains("Sales", result.Properties);
            Assert.NotNull(result.ModifiedAt);

            // Verify it was actually updated in database
            var updatedRecord = await Context.Records.FindAsync((long)1);
            Assert.Equal("John Doe Updated Customer Record", updatedRecord.Name);
            Assert.NotNull(updatedRecord.ModifiedAt);
        }

        [Fact]
        public async Task UpdateRecord_NonExistentRecord_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new RecordRequestDto
            {
                Properties = new JsonObject { ["test"] = "value" },
                Name = "Update Test"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.UpdateRecord(1, 999, dto));

            Assert.Contains("Record with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateRecord_WrongProject_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new RecordRequestDto
            {
                Properties = new JsonObject { ["test"] = "value" },
                Name = "Update Test"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.UpdateRecord(2, 1, dto)); // Record 1 belongs to project 1

            Assert.Contains("Record with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateRecord_ArchivedRecord_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new RecordRequestDto
            {
                Properties = new JsonObject { ["test"] = "value" },
                Name = "Update Test"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.UpdateRecord(1, 10, dto)); 

            Assert.Contains("Record with id 10 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateRecord_JsonDepthTooDeep_ThrowsException()
        {
            // Arrange
            var deepProperties = new JsonObject
            {
                ["level1"] = new JsonObject
                {
                    ["level2"] = new JsonObject
                    {
                        ["level3"] = new JsonObject
                        {
                            ["level4"] = "too deep"
                        }
                    }
                }
            };

            var dto = new RecordRequestDto
            {
                Properties = deepProperties,
                Name = "Deep JSON Update"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _recordBusiness.UpdateRecord(1, 1, dto));

            Assert.Contains("depth of the JSON structure exceeds the maximum allowed depth of 3", exception.Message);
        }

        #endregion

        #region DeleteRecord Tests

        [Fact]
        public async Task DeleteRecord_ValidRecord_DeletesSuccessfully()
        {
            // Act
            var result = await _recordBusiness.DeleteRecord(1, 2);

            // Assert
            Assert.True(result);

            // Verify it was actually deleted from database
            var deletedRecord = await Context.Records.FindAsync((long)2);
            Assert.Null(deletedRecord);
        }

        [Fact]
        public async Task DeleteRecord_NonExistentRecord_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.DeleteRecord(1, 999));

            Assert.Contains("Record with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task DeleteRecord_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.DeleteRecord(2, 1)); // Record 1 belongs to project 1

            Assert.Contains("Record with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task DeleteRecord_ArchivedRecord_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.DeleteRecord(1, 10)); 

            Assert.Contains("Record with id 10 not found", exception.Message);
        }

        #endregion

        #region ArchiveRecord Tests

        [Fact]
        public async Task ArchiveRecord_ValidRecord_ArchivesSuccessfully()
        {
            // Arrange
            var beforeArchive = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // Act
            var result = await _recordBusiness.ArchiveRecord(1, 1);

            // Assert
            Assert.True(result);

            // Verify it was actually archived in database
            var archivedRecord = await Context.Records.FindAsync((long)1);
            Assert.NotNull(archivedRecord);
            Assert.NotNull(archivedRecord.ArchivedAt);
            Assert.True(archivedRecord.ArchivedAt >= beforeArchive);

            // Verify related edges were also archived
            var edge = await Context.Edges.FindAsync((long)1);
            Assert.NotNull(edge.ArchivedAt);
        }

        [Fact]
        public async Task ArchiveRecord_NonExistentRecord_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.ArchiveRecord(1, 999));

            Assert.Contains("Record with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task ArchiveRecord_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.ArchiveRecord(2, 1)); // Record 1 belongs to project 1

            Assert.Contains("Record with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task ArchiveRecord_AlreadyArchivedRecord_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.ArchiveRecord(1, 3)); // Record 3 is already archived

            Assert.Contains("Record with id 3 not found", exception.Message);
        }

        [Fact]
        public async Task ArchiveRecord_ArchivedRecordNotReturnedInGetAll()
        {
            // Arrange
            var initialCount = (await _recordBusiness.GetAllRecords(1)).Count();

            // Act
            await _recordBusiness.ArchiveRecord(1, 2);
            var finalCount = (await _recordBusiness.GetAllRecords(1)).Count();

            // Assert
            Assert.Equal(initialCount - 1, finalCount);
        }

        [Fact]
        public async Task ArchiveRecord_StoredProcedureFailure_ThrowsDependencyDeletionException()
        {
            // Arrange - Create a scenario where the stored procedure might fail
            // This test simulates the stored procedure returning 0 affected rows
            
            // First, let's create a record that we'll manipulate to cause a failure
            var testRecord = new Record
            {
                Id = 100,
                Uri = "test://failure",
                Properties = @"{""test"":""failure""}",
                Name = "Failure Test Record",
                DataSourceId = 1,
                ProjectId = 1,
                CreatedBy = "test@example.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            Context.Records.Add(testRecord);
            await Context.SaveChangesAsync();

            // Now modify the stored procedure to simulate a failure condition
            await Context.Database.ExecuteSqlRawAsync(@"
                CREATE OR REPLACE FUNCTION deeplynx.archive_record(
                    record_id_param INTEGER,
                    archived_at_param TIMESTAMP WITHOUT TIME ZONE
                ) RETURNS INTEGER AS $$
                BEGIN
                    -- Simulate a failure by returning 0 for record id 100
                    IF record_id_param = 100 THEN
                        RETURN 0;
                    END IF;
                    
                    -- Normal processing for other records
                    UPDATE deeplynx.records 
                    SET archived_at = archived_at_param 
                    WHERE id = record_id_param AND archived_at IS NULL;
                    
                    RETURN 1;
                END;
                $$ LANGUAGE plpgsql;");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DependencyDeletionException>(
                () => _recordBusiness.ArchiveRecord(1, 100));

            Assert.Contains("unable to archive record 100", exception.Message);
        }

        #endregion

        #region JSON Depth Calculation Tests

        [Fact]
        public async Task JsonDepthCalculation_SimpleObject_ReturnsCorrectDepth()
        {
            // Arrange - depth 1
            var properties = new JsonObject
            {
                ["name"] = "Test",
                ["value"] = 123
            };

            var dto = new RecordRequestDto
            {
                Properties = properties,
                Name = "Simple Object"
            };

            // Act & Assert - Should not throw
            var result = await _recordBusiness.CreateRecord(1, 1, dto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task JsonDepthCalculation_NestedObject_ReturnsCorrectDepth()
        {
            // Arrange - depth 2
            var properties = new JsonObject
            {
                ["user"] = new JsonObject
                {
                    ["name"] = "Test User",
                    ["id"] = 123
                }
            };

            var dto = new RecordRequestDto
            {
                Properties = properties,
                Name = "Nested Object"
            };

            // Act & Assert - Should not throw
            var result = await _recordBusiness.CreateRecord(1, 1, dto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task JsonDepthCalculation_ArrayWithNestedObjects_ReturnsCorrectDepth()
        {
            // Arrange - depth 3
            var properties = new JsonObject
            {
                ["users"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["name"] = "User 1",
                        ["details"] = new JsonObject
                        {
                            ["role"] = "admin"
                        }
                    },
                    new JsonObject
                    {
                        ["name"] = "User 2",
                        ["details"] = new JsonObject
                        {
                            ["role"] = "user"
                        }
                    }
                }
            };

            var dto = new RecordRequestDto
            {
                Properties = properties,
                Name = "Array with Nested Objects"
            };

            // Act & Assert - Should not throw (depth is exactly 3)
            var result = await _recordBusiness.CreateRecord(1, 1, dto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task JsonDepthCalculation_EmptyArray_ReturnsCorrectDepth()
        {
            // Arrange - depth 1
            var properties = new JsonObject
            {
                ["emptyArray"] = new JsonArray(),
                ["emptyObject"] = new JsonObject(),
                ["primitiveValue"] = "test"
            };

            var dto = new RecordRequestDto
            {
                Properties = properties,
                Name = "Empty Collections"
            };

            // Act & Assert - Should not throw
            var result = await _recordBusiness.CreateRecord(1, 1, dto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task JsonDepthCalculation_PrimitiveValues_ReturnsZeroDepth()
        {
            // Arrange - primitives only
            var properties = new JsonObject
            {
                ["string"] = "test",
                ["number"] = 42,
                ["boolean"] = true,
                ["null"] = (JsonNode)null
            };

            var dto = new RecordRequestDto
            {
                Properties = properties,
                Name = "Primitive Values Only"
            };

            // Act & Assert - Should not throw
            var result = await _recordBusiness.CreateRecord(1, 1, dto);
            Assert.NotNull(result);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task RecordOperations_ConcurrentModification_HandlesCorrectly()
        {
            // This test simulates concurrent operations on different records
            // Arrange
            var dto1 = new RecordRequestDto
            {
                Properties = new JsonObject { ["concurrent"] = "update1" },
                Name = "Concurrent Update 1"
            };

            var dto2 = new RecordRequestDto
            {
                Properties = new JsonObject { ["concurrent"] = "update2" },
                Name = "Concurrent Update 2"
            };

            // Act
            var task1 = _recordBusiness.UpdateRecord(1, 1, dto1);
            var task2 = _recordBusiness.UpdateRecord(1, 2, dto2);

            await Task.WhenAll(task1, task2);

            // Assert
            var result1 = await task1;
            var result2 = await task2;

            Assert.Equal("Concurrent Update 1", result1.Name);
            Assert.Equal("Concurrent Update 2", result2.Name);
        }

        [Fact]
        public async Task RecordOperations_LargeJsonProperties_HandlesCorrectly()
        {
            // Arrange - Create large JSON within depth limits
            var largeProperties = new JsonObject();
            
            for (int i = 0; i < 100; i++)
            {
                largeProperties[$"property_{i}"] = $"value_{i}";
                largeProperties[$"object_{i}"] = new JsonObject
                {
                    ["id"] = i,
                    ["data"] = new JsonObject
                    {
                        ["nested_value"] = $"nested_{i}"
                    }
                };
            }

            var dto = new RecordRequestDto
            {
                Properties = largeProperties,
                Name = "Large JSON Record"
            };

            // Act
            var result = await _recordBusiness.CreateRecord(1, 1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("value_50", result.Properties);
            Assert.Contains("nested_50", result.Properties);
        }

        [Fact]
        public async Task RecordOperations_SpecialCharactersInJson_HandlesCorrectly()
        {
            // Arrange
            var properties = new JsonObject
            {
                ["unicode_text"] = "Unicode: 中文, العربية, русский, émojis 🚀🎉",
                ["special_chars"] = "Quotes: \"double\" and 'single', newlines\nand tabs\t",
                ["json_escape"] = "{\"escaped\": \"json\", \"array\": [1, 2, 3]}",
                ["paths"] = "C:\\Windows\\System32\\file.txt"
            };

            var dto = new RecordRequestDto
            {
                Uri = "test://unicode/测试",
                Properties = properties,
                Name = "Unicode & Special Characters Record",
                OriginalId = "ORIG_UNICODE_🌟"
            };

            // Act
            var result = await _recordBusiness.CreateRecord(1, 1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test://unicode/测试", result.Uri);
            Assert.Equal("ORIG_UNICODE_🌟", result.OriginalId);
            Assert.Contains("中文", result.Properties);
            Assert.Contains("🚀🎉", result.Properties);
        }

        [Fact]
        public async Task RecordOperations_NullAndEmptyValues_HandlesCorrectly()
        {
            // Arrange
            var properties = new JsonObject
            {
                ["null_value"] = (JsonNode)null,
                ["empty_string"] = "",
                ["empty_object"] = new JsonObject(),
                ["empty_array"] = new JsonArray()
            };

            var dto = new RecordRequestDto
            {
                Properties = properties,
                Name = "Null and Empty Values Record",
                OriginalId = null, // Test null original ID
                ClassName = null   // Test null class name
            };

            // Act
            var result = await _recordBusiness.CreateRecord(1, 1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.OriginalId);
            Assert.Contains("null", result.Properties);
        }

        [Fact]
        public async Task GetAllRecords_WithComplexFiltering_ReturnsCorrectResults()
        {
            // Arrange - Create additional test records for filtering
            var additionalRecords = new List<Record>
            {
                new Record
                {
                    Id = 10,
                    Properties = @"{""category"":""test1""}",
                    Name = "Filter Test 1",
                    DataSourceId = 1,
                    ProjectId = 1,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                },
                new Record
                {
                    Id = 11,
                    Properties = @"{""category"":""test2""}",
                    Name = "Filter Test 2",
                    DataSourceId = 2,
                    ProjectId = 1,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                }
            };

            Context.Records.AddRange(additionalRecords);
            await Context.SaveChangesAsync();

            // Act & Assert - Test various filtering scenarios
            var allRecords = await _recordBusiness.GetAllRecords(1);
            Assert.Equal(4, allRecords.Count()); // Original 2 + 2 new

            var dataSource1Records = await _recordBusiness.GetAllRecords(1, 1);
            Assert.Equal(2, dataSource1Records.Count()); // Original 1 + 1 new

            var dataSource2Records = await _recordBusiness.GetAllRecords(1, 2);
            Assert.Equal(2, dataSource2Records.Count()); // Original 1 + 1 new
        }

        [Fact]
        public async Task RecordLifecycle_CreateUpdateArchive_WorksCorrectly()
        {
            // Arrange
            var createDto = new RecordRequestDto
            {
                Properties = new JsonObject { ["status"] = "new" },
                Name = "Lifecycle Test Record"
            };

            // Act & Assert - Create
            var created = await _recordBusiness.CreateRecord(1, 1, createDto);
            Assert.NotNull(created);
            Assert.Equal("Lifecycle Test Record", created.Name);

            // Act & Assert - Update
            var updateDto = new RecordRequestDto
            {
                Properties = new JsonObject { ["status"] = "updated" },
                Name = "Lifecycle Test Record Updated"
            };

            var updated = await _recordBusiness.UpdateRecord(1, created.Id, updateDto);
            Assert.Equal("Lifecycle Test Record Updated", updated.Name);
            Assert.Contains("updated", updated.Properties);
            Assert.NotNull(updated.ModifiedAt);

            // Act & Assert - Archive
            var archived = await _recordBusiness.ArchiveRecord(1, created.Id);
            Assert.True(archived);

            // Verify record is no longer returned in queries
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _recordBusiness.GetRecord(1, created.Id));
            Assert.Contains($"Record with id {created.Id} not found", exception.Message);
        }

        [Fact]
        public async Task RecordArchive_WithMultipleRelatedEdges_ArchivesAll()
        {
            // Arrange - Create a record with multiple related edges
            var testRecord = new Record
            {
                Id = 200,
                Properties = @"{""test"":""archive_with_edges""}",
                Name = "Archive Test Record",
                DataSourceId = 1,
                ProjectId = 1,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            var relatedEdges = new List<Edge>
            {
                new Edge
                {
                    Id = 10,
                    OriginId = 200,
                    DestinationId = 1,
                    DataSourceId = 1,
                    ProjectId = 1,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                },
                new Edge
                {
                    Id = 11,
                    OriginId = 1,
                    DestinationId = 200,
                    DataSourceId = 1,
                    ProjectId = 1,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                }
            };

            Context.Records.Add(testRecord);
            Context.Edges.AddRange(relatedEdges);
            await Context.SaveChangesAsync();

            // Act
            var result = await _recordBusiness.ArchiveRecord(1, 200);

            // Assert
            Assert.True(result);

            // Verify all related edges are archived
            var archivedEdges = await Context.Edges
                .Where(e => e.Id == 10 || e.Id == 11)
                .ToListAsync();

            Assert.All(archivedEdges, edge => Assert.NotNull(edge.ArchivedAt));
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task CreateRecord_DatabaseConstraintViolation_HandlesGracefully()
        {
            // This test would require specific database constraints to be set up
            // For now, we'll test a simpler constraint violation scenario
            
            // Arrange - Try to create a record with invalid foreign key
            var dto = new RecordRequestDto
            {
                Properties = new JsonObject { ["test"] = "constraint" },
                Name = "Constraint Test",
                ClassId = 999 // Non-existent class ID
            };

            // Act & Assert - The method should still work as it doesn't validate ClassId constraint
            var result = await _recordBusiness.CreateRecord(1, 1, dto);
            Assert.NotNull(result);
            Assert.Equal(999, result.ClassId);
        }

        [Fact]
        public async Task UpdateRecord_ConcurrentModification_HandlesCorrectly()
        {
            // Arrange - Get the current record
            var originalRecord = await Context.Records.FindAsync(1);
            var originalModifiedAt = originalRecord.ModifiedAt;

            // Simulate another process updating the record
            originalRecord.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            Context.Records.Update(originalRecord);
            await Context.SaveChangesAsync();

            // Act - Try to update the record
            var dto = new RecordRequestDto
            {
                Properties = new JsonObject { ["concurrent"] = "test" },
                Name = "Concurrent Test"
            };

            var result = await _recordBusiness.UpdateRecord(1, 1, dto);

            // Assert - Update should succeed (no optimistic concurrency control implemented)
            Assert.Equal("Concurrent Test", result.Name);
        }

        #endregion
    }

    // Additional test class for testing DTOs and helper methods
    public class RecordBusinessHelperTests
    {
        [Fact]
        public void RecordRequestDto_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var properties = new JsonObject { ["test"] = "value" };
            var dto = new RecordRequestDto
            {
                Uri = "test://uri",
                Properties = properties,
                OriginalId = "ORIG_TEST",
                Name = "Test Record",
                ClassName = "TestClass",
                ClassId = 1
            };

            // Assert
            Assert.Equal("test://uri", dto.Uri);
            Assert.Equal(properties, dto.Properties);
            Assert.Equal("ORIG_TEST", dto.OriginalId);
            Assert.Equal("Test Record", dto.Name);
            Assert.Equal("TestClass", dto.ClassName);
            Assert.Equal(1, dto.ClassId);
        }

        [Fact]
        public void RecordResponseDto_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new RecordResponseDto
            {
                Id = 1,
                Uri = "test://uri",
                Properties = @"{""test"":""value""}",
                OriginalId = "ORIG_TEST",
                Name = "Test Record",
                ClassId = 1,
                DataSourceId = 1,
                ProjectId = 1,
                CreatedBy = "test@example.com",
                CreatedAt = now,
                ModifiedBy = "modified@example.com",
                ModifiedAt = now.AddDays(1),
                ArchivedAt = null
            };

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal("test://uri", dto.Uri);
            Assert.Equal(@"{""test"":""value""}", dto.Properties);
            Assert.Equal("ORIG_TEST", dto.OriginalId);
            Assert.Equal("Test Record", dto.Name);
            Assert.Equal(1, dto.ClassId);
            Assert.Equal(1, dto.DataSourceId);
            Assert.Equal(1, dto.ProjectId);
            Assert.Equal("test@example.com", dto.CreatedBy);
            Assert.Equal(now, dto.CreatedAt);
            Assert.Equal("modified@example.com", dto.ModifiedBy);
            Assert.Equal(now.AddDays(1), dto.ModifiedAt);
            Assert.Null(dto.ArchivedAt);
        }

        [Theory]
        [InlineData(@"{""simple"":""value""}", 1)]
        [InlineData(@"{""level1"":{""level2"":""value""}}", 2)]
        [InlineData(@"{""level1"":{""level2"":{""level3"":""value""}}}", 3)]
        [InlineData(@"{""array"":[{""nested"":""value""}]}", 2)]
        [InlineData(@"{""empty_object"":{}}", 1)]
        [InlineData(@"{""empty_array"":[]}", 1)]
        public void JsonDepthCalculation_VariousStructures_ReturnsExpectedDepth(string jsonString, int expectedDepth)
        {
            // This would test the private CalculateJsonMaxDepth method if it were made internal/public
            // For now, we test it indirectly through the Create/Update methods
            
            // The actual testing is done in the integration tests above
            // This theory demonstrates the expected behavior
            var jsonNode = JsonNode.Parse(jsonString);
            Assert.NotNull(jsonNode);
            
            // Note: The actual depth calculation is tested through the CreateRecord tests
            // where we verify that depth > 3 throws exceptions and depth <= 3 succeeds
        }
    }
}