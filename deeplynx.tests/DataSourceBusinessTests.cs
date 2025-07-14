using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Moq;

namespace deeplynx.tests
{
    public class DataSourceBusinessTests : IntegrationTestBase
    {
        private DeeplynxContext _context;
        private DataSourceBusiness _dataSourceBusiness;
        private Mock<IEdgeBusiness> _mockEdgeBusiness;
        private Mock<IRecordBusiness> _mockRecordBusiness;

        public DataSourceBusinessTests()
        {
            
            _mockEdgeBusiness = new Mock<IEdgeBusiness>();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _dataSourceBusiness = new DataSourceBusiness(
                Context,
                _mockEdgeBusiness.Object,
                _mockRecordBusiness.Object);
        }
        
        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        #region GetAllDataSources Tests

        [Fact]
        public async Task GetAllDataSources_ValidProjectId_ReturnsActiveDataSources()
        {
            // Act
            var result = await _dataSourceBusiness.GetAllDataSources(1, false);
            var dataSources = result.ToList();

            // Assert
            Assert.Equal(2, dataSources.Count);
            Assert.All(dataSources, ds => Assert.Equal(1, ds.ProjectId));
            Assert.All(dataSources, ds => Assert.Null(ds.ArchivedAt));
            Assert.Contains(dataSources, ds => ds.Name == "Customer CRM Database");
            Assert.Contains(dataSources, ds => ds.Name == "E-commerce Transaction API");
            Assert.DoesNotContain(dataSources, ds => ds.Name == "Archived Data Source");
        }

        [Fact]
        public async Task GetAllDataSources_ProjectWithNoDataSources_ReturnsEmptyList()
        {
            // Act
            var result = await _dataSourceBusiness.GetAllDataSources(999, false);
            var dataSources = result.ToList();

            // Assert
            Assert.Empty(dataSources);
        }

        [Fact]
        public async Task GetAllDataSources_DifferentProject_ReturnsCorrectDataSources()
        {
            // Act
            var result = await _dataSourceBusiness.GetAllDataSources(2, false);
            var dataSources = result.ToList();

            // Assert
            Assert.Equal(2, dataSources.Count);
            Assert.Equal("Enterprise Resource Planning System", dataSources.First().Name);
            Assert.Equal(2, dataSources.First().ProjectId);
        }

        [Fact]
        public async Task GetAllDataSources_ConfigParsing_ReturnsValidJsonObject()
        {
            // Act
            var result = await _dataSourceBusiness.GetAllDataSources(1, false);
            var dataSource = result.First(ds => ds.Name == "Customer CRM Database");

            // Assert
            Assert.NotNull(dataSource.Config);
            Assert.Equal("sqlserver", dataSource.Config["driver"]?.ToString());
            Assert.Equal("crm-prod.company.com", dataSource.Config["host"]?.ToString());
            Assert.Equal(1433, dataSource.Config["port"]?.GetValue<int>());
        }

        [Fact]
        public async Task GetAllDataSources_NullConfig_ReturnsEmptyJsonObject()
        {
            // Arrange
            var dataSourceWithNullConfig = new DataSource
            {
                Id = 100,
                Name = "Null Config Test",
                Description = "Primary customer relationship management database",
                Abbreviation = "CRM_DB",
                Type = "SQL Server",
                BaseUri = "Server=crm-prod.company.com;Database=CustomerData;",
                Config = null,
                ProjectId = 1,
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
                ModifiedBy = "db.admin@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-45),
                ArchivedAt = null
            };
            await Context.DataSources.AddAsync(dataSourceWithNullConfig);
            await Context.SaveChangesAsync();

            // Act
            var result = await _dataSourceBusiness.GetAllDataSources(1, false);
            var dataSource = result.First(ds => ds.Name == "Null Config Test");

            // Assert
            Assert.NotNull(dataSource.Config);
            Assert.Empty(dataSource.Config);
        }

        #endregion

        #region GetDataSource Tests

        [Fact]
        public async Task GetDataSource_ValidIds_ReturnsDataSource()
        {
            // Act
            var result = await _dataSourceBusiness.GetDataSource(1, 1, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Customer CRM Database", result.Name);
            Assert.Equal("Primary customer relationship management database", result.Description);
            Assert.Equal("CRM_DB", result.Abbreviation);
            Assert.Equal("SQL Server", result.Type);
            Assert.Equal("Server=crm-prod.company.com;Database=CustomerData;", result.BaseUri);
            Assert.Equal(1, result.ProjectId);
            Assert.NotNull(result.Config);
        }

        [Fact]
        public async Task GetDataSource_NonExistentDataSource_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.GetDataSource(1, 999, false));
            
            Assert.Contains("Data Source with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task GetDataSource_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.GetDataSource(2, 1, false)); // DataSource 1 belongs to project 1, not 2
            
            Assert.Contains("Data Source with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task GetDataSource_ArchivedDataSource_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.GetDataSource(1, 3, true)); // DataSource 3 is archived
            
            Assert.Contains("Data Source with id 3 is archived", exception.Message);
        }

        [Fact]
        public async Task GetDataSource_ValidDataSource_ParsesConfigCorrectly()
        {
            // Act
            var result = await _dataSourceBusiness.GetDataSource(1, 2, false);

            // Assert
            Assert.NotNull(result.Config);
            Assert.Equal("v2", result.Config["api_version"]?.ToString());
            Assert.Equal(30, result.Config["timeout"]?.GetValue<int>());
        }

        #endregion

        #region CreateDataSource Tests

        [Fact]
        public async Task CreateDataSource_ValidDto_CreatesDataSource()
        {
            // Arrange
            var config = new JsonObject
            {
                ["driver"] = "postgresql",
                ["host"] = "localhost",
                ["port"] = 5432
            };

            var dto = new DataSourceRequestDto
            {
                Name = "New Test Data Source",
                Description = "A newly created test data source",
                Abbreviation = "NEW_TEST",
                Type = "PostgreSQL",
                BaseUri = "Server=localhost;Database=NewTest;",
                Config = config
            };

            // Act
            var result = await _dataSourceBusiness.CreateDataSource(1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("New Test Data Source", result.Name);
            Assert.Equal("A newly created test data source", result.Description);
            Assert.Equal("NEW_TEST", result.Abbreviation);
            Assert.Equal("PostgreSQL", result.Type);
            Assert.Equal("Server=localhost;Database=NewTest;", result.BaseUri);
            Assert.Equal(1, result.ProjectId);
            Assert.NotNull(result.Config);
            Assert.Equal("postgresql", result.Config["driver"]?.ToString());

            // Verify it was actually saved to database
            var savedDataSource = await Context.DataSources.FindAsync(result.Id);
            Assert.NotNull(savedDataSource);
            Assert.Equal("New Test Data Source", savedDataSource.Name);
        }

        [Fact]
        public async Task CreateDataSource_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _dataSourceBusiness.CreateDataSource(1, null));
        }

        [Fact]
        public async Task CreateDataSource_NullConfig_CreatesWithEmptyConfig()
        {
            // Arrange
            var dto = new DataSourceRequestDto
            {
                Name = "No Config Data Source",
                Description = "Data source without config",
                Type = "File System"
            };

            // Act
            var result = await _dataSourceBusiness.CreateDataSource(1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Config);
            Assert.Empty(result.Config);
        }

        [Fact]
        public async Task CreateDataSource_SetsCreatedAtAndCreatedBy()
        {
            // Arrange
            var dto = new DataSourceRequestDto
            {
                Name = "Timestamp Test",
                Type = "Test"
            };

            var beforeCreate = DateTime.UtcNow;

            // Act
            var result = await _dataSourceBusiness.CreateDataSource(1, dto);

            // Assert
            Assert.True(result.CreatedAt >= beforeCreate);
            Assert.True(result.CreatedAt <= DateTime.UtcNow);
            // CreatedBy is null in current implementation (TODO: JWT implementation)
            Assert.Null(result.CreatedBy);
        }

        #endregion

        #region UpdateDataSource Tests

        [Fact]
        public async Task UpdateDataSource_ValidUpdate_UpdatesDataSource()
        {
            // Arrange
            var config = new JsonObject
            {
                ["driver"] = "mysql",
                ["host"] = "updated.com",
                ["port"] = 3306
            };

            var dto = new DataSourceRequestDto
            {
                Name = "Updated Test Data Source",
                Description = "Updated description",
                Abbreviation = "UPD_TEST",
                Type = "MySQL",
                BaseUri = "Server=updated.com;Database=UpdatedDB;",
                Config = config
            };

            // Act
            var result = await _dataSourceBusiness.UpdateDataSource(1, 1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Updated Test Data Source", result.Name);
            Assert.Equal("Updated description", result.Description);
            Assert.Equal("UPD_TEST", result.Abbreviation);
            Assert.Equal("MySQL", result.Type);
            Assert.Equal("Server=updated.com;Database=UpdatedDB;", result.BaseUri);
            Assert.NotNull(result.ModifiedAt);
            Assert.Equal("mysql", result.Config["driver"]?.ToString());

            // Verify it was actually updated in database
            var updatedDataSource = await Context.DataSources.FindAsync((long)1);
            Assert.Equal("Updated Test Data Source", updatedDataSource.Name);
            Assert.NotNull(updatedDataSource.ModifiedAt);
        }

        [Fact]
        public async Task UpdateDataSource_NonExistentDataSource_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new DataSourceRequestDto
            {
                Name = "Update Test",
                Type = "Test"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.UpdateDataSource(1, 999, dto));
            
            Assert.Contains("Data Source with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateDataSource_WrongProject_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new DataSourceRequestDto
            {
                Name = "Update Test",
                Type = "Test"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.UpdateDataSource(2, 1, dto)); // DataSource 1 belongs to project 1
            
            Assert.Contains("Data Source with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateDataSource_ArchivedDataSource_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new DataSourceRequestDto
            {
                Name = "Update Test",
                Type = "Test"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.UpdateDataSource(1, 3, dto)); // DataSource 3 is archived
            
            Assert.Contains("Data Source with id 3 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateDataSource_NullConfig_UpdatesWithEmptyConfig()
        {
            // Arrange
            var dto = new DataSourceRequestDto
            {
                Name = "No Config Update",
                Type = "Test",
                Config = null
            };

            // Act
            var result = await _dataSourceBusiness.UpdateDataSource(1, 2, dto);

            // Assert
            Assert.NotNull(result.Config);
            Assert.Empty(result.Config);
        }

        #endregion

        #region DeleteDataSource Tests

        [Fact]
        public async Task DeleteDataSource_ValidDataSource_DeletesSuccessfully()
        {
            // Act
            var result = await _dataSourceBusiness.DeleteDataSource(1, 2);

            // Assert
            Assert.True(result);

            // Verify it was actually deleted from database
            var deletedDataSource = await Context.DataSources.FindAsync((long)2);
            Assert.Null(deletedDataSource);
        }

        [Fact]
        public async Task DeleteDataSource_NonExistentDataSource_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.DeleteDataSource(1, 999));
            
            Assert.Contains("Data Source with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task DeleteDataSource_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.DeleteDataSource(2, 1)); // DataSource 1 belongs to project 1
            
            Assert.Contains("Data Source with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task DeleteDataSource_ArchivedDataSource_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.DeleteDataSource(1, 3)); // DataSource 3 is archived
            
            Assert.Contains("Data Source with id 3 not found", exception.Message);
        }

        #endregion

        #region ArchiveDataSource Tests

        [Fact]
        public async Task ArchiveDataSource_ValidDataSource_ArchivesSuccessfully()
        {
            // Arrange
            var beforeArchive = DateTime.UtcNow;

            // Act
            var result = await _dataSourceBusiness.ArchiveDataSource(1, 1);

            // Assert
            Assert.True(result);

            // Verify it was actually archived in database
            var archivedDataSource = await Context.DataSources.FindAsync((long)1);
            Assert.NotNull(archivedDataSource);
            Assert.NotNull(archivedDataSource.ArchivedAt);
            Assert.True(archivedDataSource.ArchivedAt >= beforeArchive);
            Assert.True(archivedDataSource.ArchivedAt <= DateTime.UtcNow);
        }

        [Fact]
        public async Task ArchiveDataSource_NonExistentDataSource_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.ArchiveDataSource(1, 999));
            
            Assert.Contains("Data Source with id 999 not found", exception.Message);
        }

        [Fact]
        public async Task ArchiveDataSource_WrongProject_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.ArchiveDataSource(2, 1)); 
            
            Assert.Contains("Data Source with id 1 not found", exception.Message);
        }

        [Fact]
        public async Task ArchiveDataSource_AlreadyArchivedDataSource_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _dataSourceBusiness.ArchiveDataSource(1, 3)); // DataSource 3 is already archived
            
            Assert.Contains("Data Source with id 3 not found", exception.Message);
        }

        [Fact]
        public async Task ArchiveDataSource_ArchivedDataSourceNotReturnedInGetAll()
        {
            // Arrange
            var initialCount = (await _dataSourceBusiness.GetAllDataSources(1, false)).Count();

            // Act
            await _dataSourceBusiness.ArchiveDataSource(1, 1);
            var finalCount = (await _dataSourceBusiness.GetAllDataSources(1, true)).Count();

            // Assert
            Assert.Equal(initialCount - 1, finalCount);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task DataSourceOperations_ConcurrentModification_HandlesCorrectly()
        {
            // This test simulates concurrent operations on the same data source
            // In a real scenario, you might want to test with actual concurrent tasks

            // Arrange
            var dto1 = new DataSourceRequestDto
            {
                Name = "Concurrent Update 1",
                Type = "Test1"
            };

            var dto2 = new DataSourceRequestDto
            {
                Name = "Concurrent Update 2", 
                Type = "Test2"
            };

            // Act
            var task1 = await _dataSourceBusiness.UpdateDataSource(1, 1, dto1);
            var task2 = await _dataSourceBusiness.UpdateDataSource(1, 2, dto2);
            
            // Assert
            var result1 = task1;
            var result2 = task2;

            Assert.Equal("Concurrent Update 1", result1.Name);
            Assert.Equal("Concurrent Update 2", result2.Name);
            
        }

        [Fact]
        public async Task DataSourceOperations_LargeConfigJson_HandlesCorrectly()
        {
            // Arrange
            var largeConfig = new JsonObject();
            for (int i = 0; i < 100; i++)
            {
                largeConfig[$"property_{i}"] = $"value_{i}";
                largeConfig[$"nested_{i}"] = new JsonObject
                {
                    ["sub_property"] = i,
                    ["sub_array"] = new JsonArray { i, i + 1, i + 2 }
                };
            }

            var dto = new DataSourceRequestDto
            {
                Name = "Large Config Test",
                Type = "Test",
                Config = largeConfig
            };

            // Act
            var result = await _dataSourceBusiness.CreateDataSource(1, dto);

            // Assert
            Assert.NotNull(result.Config);
            Assert.Equal("value_50", result.Config["property_50"]?.ToString());
            Assert.Equal(50, result.Config["nested_50"]?["sub_property"]?.GetValue<int>());
        }

        [Fact]
        public async Task DataSourceOperations_SpecialCharactersInFields_HandlesCorrectly()
        {
            // Arrange
            var dto = new DataSourceRequestDto
            {
                Name = "Test with émojis 🚀 and ñ special chars",
                Description = "Description with quotes \"test\" and 'single quotes'",
                Abbreviation = "SPEC_CHAR",
                Type = "Test & Special",
                BaseUri = "https://test.com/path?param=value&other=123",
                Config = new JsonObject
                {
                    ["special"] = "Value with quotes \"test\" and newlines\nand tabs\t",
                    ["unicode"] = "Unicode: 中文, العربية, русский"
                }
            };

            // Act
            var result = await _dataSourceBusiness.CreateDataSource(1, dto);

            // Assert
            Assert.Equal("Test with émojis 🚀 and ñ special chars", result.Name);
            Assert.Contains("quotes \"test\"", result.Description);
            Assert.Equal("Test & Special", result.Type);
            Assert.Contains("Unicode: 中文", result.Config["unicode"]?.ToString());
        }

        #endregion
        
        [Fact]
        public void DataSourceRequestDto_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var config = new JsonObject { ["test"] = "value" };
            var dto = new DataSourceRequestDto
            {
                Name = "Test Name",
                Description = "Test Description",
                Abbreviation = "TEST",
                Type = "Test Type",
                BaseUri = "http://test.com",
                Config = config
            };

            // Assert
            Assert.Equal("Test Name", dto.Name);
            Assert.Equal("Test Description", dto.Description);
            Assert.Equal("TEST", dto.Abbreviation);
            Assert.Equal("Test Type", dto.Type);
            Assert.Equal("http://test.com", dto.BaseUri);
            Assert.Equal(config, dto.Config);
        }

        [Fact]
        public void DataSourceResponseDto_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var config = new JsonObject { ["test"] = "value" };
            var now = DateTime.UtcNow;
            
            var dto = new DataSourceResponseDto
            {
                Id = 1,
                Name = "Test Name",
                Description = "Test Description", 
                Abbreviation = "TEST",
                Type = "Test Type",
                BaseUri = "http://test.com",
                Config = config,
                ProjectId = 1,
                CreatedBy = "test@example.com",
                CreatedAt = now,
                ModifiedBy = "modified@example.com",
                ModifiedAt = now.AddDays(1),
                ArchivedAt = null
            };

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal("Test Name", dto.Name);
            Assert.Equal("Test Description", dto.Description);
            Assert.Equal("TEST", dto.Abbreviation);
            Assert.Equal("Test Type", dto.Type);
            Assert.Equal("http://test.com", dto.BaseUri);
            Assert.Equal(config, dto.Config);
            Assert.Equal(1, dto.ProjectId);
            Assert.Equal("test@example.com", dto.CreatedBy);
            Assert.Equal(now, dto.CreatedAt);
            Assert.Equal("modified@example.com", dto.ModifiedBy);
            Assert.Equal(now.AddDays(1), dto.ModifiedAt);
            Assert.Null(dto.ArchivedAt);
        }
    }
}