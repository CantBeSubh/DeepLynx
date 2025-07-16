using System;
using System.IO;
using System.Threading.Tasks;
using deeplynx.graph;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.Extensions.Configuration;
using Serilog;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class KuzuDatabaseManagerTests : IntegrationTestBase
    {
        private KuzuDatabaseManager _kuzuDatabaseManager;
        private readonly IConfiguration _configuration;
        private const int ProjectId = 1;

        public KuzuDatabaseManagerTests(TestSuiteFixture fixture) : base(fixture)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\test-log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
            _kuzuDatabaseManager = new KuzuDatabaseManager(_configuration);
        }

        public override async Task InitializeAsync()
        {
            if (System.IO.Directory.Exists("../deeplynx.graph/kuzu_db"))
                System.IO.Directory.Delete("../deeplynx.graph/kuzu_db", true);

            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

            Log.Information("Initializing KuzuDatabaseManagerTests...");
            await _kuzuDatabaseManager.ConnectAsync();
            Log.Information("KuzuDatabaseManager initialized and connected.");
        }


        private async Task<bool> ExecuteSqlFromFileAsync(string filePath)
        {
            string? connectionString = _configuration.GetConnectionString("DefaultConnection");

            string sql;
            try
            {
                sql = await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading SQL file: {ex.Message}");
                throw;
            }

            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        var result = await command.ExecuteNonQueryAsync();
                        if (result > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }


        [Fact]
        public async Task InitalSeedDatabaseAsync()
        {
            // Arrange
            bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
            // Act
            Log.Information("Running SeedDatabaseAsync test...");

            // Assert
            Assert.True(result);
            Log.Information("SeedDatabaseAsync test completed.");
        }

        [Fact]
        public async Task SeedDatabaseAsync()
        {
            // Arrange
            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
            bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

            // Act
            Log.Information("Running SeedDatabaseAsync test...");

            // Assert
            Assert.True(result);
            Log.Information("SeedDatabaseAsync test completed.");
        }


        #region ConnectAsync Tests

        [Fact]
        public async Task ConnectAsync_ShouldInitializeDatabase_WhenNotInitialized()
        {
            // Arrange & Act
            Log.Information("Running ConnectAsync_ShouldInitializeDatabase_WhenNotInitialized test...");
            bool connected = await _kuzuDatabaseManager.ConnectAsync();

            // Assert
            Assert.True(connected);
            Log.Information("ConnectAsync test completed. Connected: {Connected}", connected);
        }

        #endregion


        #region InstallPostgresExtensionsAsync Tests

        [Fact]
        public async Task InstallPostgresExtensionsAsync_ShouldInstallExtensions_WhenCalled()
        {
            // Arrange & Act
            Log.Information("Running InstallPostgresExtensionsAsync_ShouldInstallExtensions_WhenCalled test...");
            bool result = await _kuzuDatabaseManager.InstallPostgresExtensionsAsync();

            // Assert
            Assert.True(result);
            Log.Information("InstallPostgresExtensionsAsync test completed. Result: {Result}", result);
        }

        #endregion


        #region ExportDataAsync Tests

        [Fact]
        public async Task ExportDataAsync_ShouldReturnTrue_WhenExportIsSuccessful()
        {
            // Arrange & Act
            Log.Information("Running ExportDataAsync_ShouldReturnTrue_WhenExportIsSuccessful test...");
            bool result = await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

            // Assert
            Assert.True(result);
            Log.Information("ExportDataAsync test completed. Result: {Result}", result);
        }

        [Fact]
        public async Task ExportDataAsync_ShouldReturnTrue_WhenProjectIdAlreadyProcessed()
        {
            //Arrange
            var requestDto = new KuzuDBMQueryRequestDto
            {
                Query = "DETACH test;"
            };
            await _kuzuDatabaseManager.ExportDataAsync(ProjectId);
            await _kuzuDatabaseManager.ExecuteQueryAsync(requestDto);

            // Act
            Log.Information("Running ExportDataAsync_ShouldReturnTrue_WhenProjectIdAlreadyProcessed test...");
            await _kuzuDatabaseManager.ConnectAsync();
            var result = await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

            // Assert
            Assert.True(result);
            Log.Information("ExportDataAsync test for already processed project completed. Result: {Result}", result);
        }

        #endregion


        #region GetNodesWithinDepthByIdAsync Tests

        [Fact]
        public async Task GetNodesWithinDepthByIdAsync_ValidInput_ReturnsExpectedResults()
        {
            // Arrange
            var requestDto = new KuzuDBMNodesWithinDepthRequestDto
            {
                TableName = "Musician",
                Id = 1,
                Depth = 1,
            };
            await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

            // Act
            Log.Information("Running GetNodesWithinDepthByIdAsync_ValidInput_ReturnsExpectedResults test...");
            var result = await _kuzuDatabaseManager.GetNodesWithinDepthByIdAsync(requestDto);
            Console.WriteLine("result: " + result);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("id:", result);
            Log.Information("GetNodesWithinDepthByIdAsync test completed. Result contains 'class_name'.");
        }

        #endregion


        #region ExecuteQuery Tests

        [Fact]
        public async Task ExecuteQuery_ValidQuery_ReturnsExpectedResults()
        {
            // Arrange
            var requestDto = new KuzuDBMQueryRequestDto
            {
                Query = @"MATCH (m:Musician) RETURN m LIMIT 1;"
            };
            await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

            // Act
            Log.Information("Running ExecuteQuery_ValidQuery_ReturnsExpectedResults test...");
            var result = await _kuzuDatabaseManager.ExecuteQueryAsync(requestDto);


            // Assert
            Assert.NotNull(result);
            Assert.Contains("id", result);
            Log.Information("ExecuteQuery test completed. Result contains 'id'.");
        }

        [Fact]
        public async Task ExecuteQuery_InvalidQuery_ShouldThrowException()
        {
            // Arrange
            var requestDto = new KuzuDBMQueryRequestDto
            {
                Query = "INVALID QUERY"
            };
            await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

            // Act & Assert
            Log.Information("Running ExecuteQuery_InvalidQuery_ShouldThrowException test...");
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _kuzuDatabaseManager.ExecuteQueryAsync(requestDto);
            });
            Log.Information("ExecuteQuery test for invalid query completed, exception was thrown as expected.");
        }

        #endregion


        #region CloseAsync Tests

        [Fact]
        public async Task CloseAsync_ShouldCloseConnection_WhenCalled()
        {
            // Arrange & Act
            Log.Information("Running CloseAsync_ShouldCloseConnection_WhenCalled test...");
            bool result = await _kuzuDatabaseManager.CloseAsync();

            // Assert
            Assert.True(result);
            Log.Information("CloseAsync test completed. Result: {Result}", result);
        }

        #endregion
    }
}
