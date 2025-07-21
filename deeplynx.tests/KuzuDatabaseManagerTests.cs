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
using DotNetEnv;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class KuzuDatabaseManagerTests : IntegrationTestBase
    {
        private KuzuDatabaseManager _kuzuDatabaseManager;
        private readonly IConfiguration _configuration;
        private const int ProjectId = 1;
        private static bool _isConnected = false;

        public KuzuDatabaseManagerTests(TestSuiteFixture fixture) : base(fixture)
        {
            Env.Load("../.env");

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
            if (!_isConnected)
            {
                await Task.Delay(5000);
                await _kuzuDatabaseManager.ConnectAsync();
                _isConnected = true;
            }

            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

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

        [SkippableFact]
        public async Task InitalSeedDatabaseAsync()
        {

            Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled. Set ENABLE_KUZU to true to run these tests.");

            // Arrange
            bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
            // Act

            // Assert
            Assert.True(result);

            if (_isConnected)
            {
                await _kuzuDatabaseManager.CloseAsync();
                _isConnected = false;
            }
        }

        [SkippableFact]
        public async Task SeedDatabaseAsync()
        {
            Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled. Set ENABLE_KUZU to true to run these tests.");

            // Arrange
            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
            await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
            bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

            // Assert
            Assert.True(result);

            if (_isConnected)
            {
                await _kuzuDatabaseManager.CloseAsync();
                _isConnected = false;
                Log.Information("Disconnected from SeedDataBaseAsync test");
            }
        }


        #region ConnectAsync Tests

        [SkippableFact]
        public async Task ConnectAsync_ShouldInitializeDatabase_WhenNotInitialized()
        {
            Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled. Set ENABLE_KUZU to true to run these tests.");

            // Arrange & Act
            bool connected = await _kuzuDatabaseManager.ConnectAsync();

            // Assert
            Assert.True(connected);

            if (_isConnected)
            {
                await _kuzuDatabaseManager.CloseAsync();
                _isConnected = false;
            }
        }

        #endregion


        #region ExportDataAsync Tests

        [SkippableFact]
        public async Task ExportDataAsync_ShouldReturnTrue_WhenExportIsSuccessful()
        {
            Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled. Set ENABLE_KUZU to true to run these tests.");

            // Arrange & Act
            bool result = await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

            // Assert
            Assert.True(result);

            if (_isConnected)
            {
                await _kuzuDatabaseManager.CloseAsync();
                _isConnected = false;
            }
        }

        #endregion


        #region GetNodesWithinDepthByIdAsync Tests

        [SkippableFact]
        public async Task GetNodesWithinDepthByIdAsync_ValidInput_ReturnsExpectedResults()
        {
            Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled. Set ENABLE_KUZU to true to run these tests.");

            // Arrange
            var requestDto = new KuzuDBMNodesWithinDepthRequestDto
            {
                TableName = "Musician",
                Id = 1,
                Depth = 1,
            };
            await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

            // Act
            var result = await _kuzuDatabaseManager.GetNodesWithinDepthByIdAsync(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("id:", result);

            if (_isConnected)
            {
                await _kuzuDatabaseManager.CloseAsync();
                _isConnected = false;
            }
        }

        #endregion


        #region ExecuteQuery Tests

        [SkippableFact]
        public async Task ExecuteQuery_ValidQuery_ReturnsExpectedResults()
        {
            Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled. Set ENABLE_KUZU to true to run these tests.");

            // Arrange
            var requestDto = new KuzuDBMQueryRequestDto
            {
                Query = @"CREATE NODE TABLE Celebrity(name STRING PRIMARY KEY);
                    ALTER TABLE Follows ADD FROM User TO Celebrity;"
            };
            await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

            // Act
            var result = await _kuzuDatabaseManager.ExecuteQueryAsync(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("id", result);

            if (_isConnected)
            {
                await _kuzuDatabaseManager.CloseAsync();
                _isConnected = false;
            }
        }

        #endregion

        #region CloseAsync Tests

        [SkippableFact]
        public async Task CloseAsync_ShouldCloseConnection_WhenCalled()
        {
            Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled. Set ENABLE_KUZU to true to run these tests.");

            // Arrange & Act
            bool result = await _kuzuDatabaseManager.CloseAsync();

            // Assert
            Assert.True(result);

            if (_isConnected)
            {
                await _kuzuDatabaseManager.CloseAsync();
                _isConnected = false;
            }
        }

        #endregion
    }
}
