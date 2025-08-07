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
using System.Diagnostics;
using Renci.SshNet;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class KuzuDatabaseManagerTests : IntegrationTestBase
    {
        private KuzuDatabaseManager? _kuzuDatabaseManager;
        private readonly IConfiguration _configuration;
        private const int ProjectId = 1;
        private bool _isConnected = false;
        private static readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);

        public KuzuDatabaseManagerTests(TestSuiteFixture fixture) : base(fixture)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));

            var envFilePath = Path.Combine(projectRoot, ".env");

            Env.Load(envFilePath);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\test-log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
            bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu);

            if (enableKuzu)
            {
                _kuzuDatabaseManager = new KuzuDatabaseManager(_configuration);
            }
            else
            {
                _kuzuDatabaseManager = null;
            }
        }

        public override async Task InitializeAsync()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));

            var kuzuDbFilePath = Path.Combine(projectRoot, "deeplynx.tests/bin/Debug/deeplynx.graph");

            if (System.IO.Directory.Exists(kuzuDbFilePath))
                System.IO.Directory.Delete(kuzuDbFilePath, true);

            bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu);

            if (enableKuzu && _kuzuDatabaseManager != null)
            {
                Log.Information("Attempting to acquire initialize semaphore lock");
                await _connectionSemaphore.WaitAsync();
                Log.Information("Initialize lock acquired");

                try
                {
                    if (!_isConnected)
                    {
                        await _kuzuDatabaseManager.ConnectAsync();
                        _isConnected = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"An error occurred during initialization: {ex.Message}");
                }
                finally
                {
                    _connectionSemaphore.Release();
                    Log.Information("Initialize lock released");
                }
            }
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
                Log.Information($"Error reading SQL file: {ex.Message}");
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
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Database operation failed: {ex.Message}");
                    return false;
                }
            }
        }

        [SkippableFact]
        public async Task InitalSeedDatabaseAsync()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));

            var kuzuDbFilePath = Path.Combine(projectRoot, "deeplynx.tests/bin/Debug/deeplynx.graph");

            Console.WriteLine("KuzuDB Path: " + kuzuDbFilePath);

            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled until the random crash bug is fixed. Please refer to the README.md local setup instructions to setup Kuzu.");

                Log.Information("Starting InitalSeedDatabaseAsync test...");

                // Arrange
                await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");

                // Assert
                await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                Assert.True(result);
                Log.Information("Test InitalSeedDatabaseAsync passed.");
            }
            catch (Exception ex)
            {
                Log.Error($"Test InitalSeedDatabaseAsync failed: {ex.Message}");
                throw;
            }
            // finally
            // {
            //     if (_kuzuDatabaseManager != null && _isConnected)
            //     {
            //         await _kuzuDatabaseManager.CloseAsync();
            //         _isConnected = false;
            //     }
            // }
        }

        [SkippableFact]
        public async Task SeedDatabaseAsync()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));
                
                Log.Information("Starting SeedDatabaseAsync test...");

                // Arrange
                await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                // Assert
                Assert.True(result);
                Log.Information("Test SeedDatabaseAsync passed.");
            }
            catch (Exception ex)
            {
                Log.Error($"Test SeedDatabaseAsync failed: {ex.Message}");
                throw;
            }
            // finally
            // {
            //     if (_kuzuDatabaseManager != null && _isConnected)
            //     {
            //         await _kuzuDatabaseManager.CloseAsync();
            //         _isConnected = false;
            //     }
            // }
        }

        #region ConnectAsync Tests

        [SkippableFact]
        public async Task ConnectAsync_ShouldInitializeDatabase_WhenNotInitialized()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                Log.Information("Starting ConnectAsync test...");

                if (_kuzuDatabaseManager != null)
                {
                    // Arrange & Act
                    bool connected = await _kuzuDatabaseManager.ConnectAsync();

                    // Assert
                    Assert.True(connected);
                    Log.Information("Test ConnectAsync passed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Test ConnectAsync_ShouldInitializeDatabase failed: {ex.Message}");
                throw;
            }
            // finally
            // {
            //     if (_kuzuDatabaseManager != null && _isConnected)
            //     {
            //         await _kuzuDatabaseManager.CloseAsync();
            //         _isConnected = false;
            //     }
            // }
        }

        #endregion

        #region ExportDataAsync Tests

        [SkippableFact]
        public async Task ExportDataAsync_ShouldReturnFalse_WhenExportIsSuccessful()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                Log.Information("Starting ExportDataAsync test...");

                if (_kuzuDatabaseManager != null)
                {
                    // Arrange & Act
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                    bool result = await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

                    // Assert
                    Assert.False(result);
                    Log.Information("Test ExportDataAsync passed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Test ExportDataAsync_ShouldReturnFalse failed: {ex.Message}");
                throw;
            }
            // finally
            // {
            //     if (_kuzuDatabaseManager != null && _isConnected)
            //     {
            //         await _kuzuDatabaseManager.CloseAsync();
            //         _isConnected = false;
            //     }
            // }
        }

        #endregion


        #region GetNodesWithinDepthByIdAsync Tests

        [SkippableFact]
        public async Task GetNodesWithinDepthByIdAsync_ValidInput_ReturnsExpectedResults()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                Log.Information("Starting GetNodesWithin test...");

                // Arrange
                var requestDto = new KuzuDBMNodesWithinDepthRequestDto
                {
                    TableName = "Musician",
                    Id = 2,
                    Depth = 1,
                };

                if (_kuzuDatabaseManager != null)
                {
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                    await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

                    // Act
                    var result = await _kuzuDatabaseManager.GetNodesWithinDepthByIdAsync(requestDto);

                    // Assert
                    Assert.NotNull(result);
                    Assert.Contains("id:", result);
                    Log.Information("Test GetNodesWithin passed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Test GetNodesWithinDepthByIdAsync failed: {ex.Message}");
                throw;
            }
            // finally
            // {
            //     if (_kuzuDatabaseManager != null && _isConnected)
            //     {
            //         await _kuzuDatabaseManager.CloseAsync();
            //         _isConnected = false;
            //     }
            // }
        }

        #endregion


        #region ExecuteQuery Tests

        [SkippableFact]
        public async Task ExecuteQuery_ValidQuery_ReturnsExpectedResults()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                Log.Information("Starting ExecuteQuery test...");

                // Arrange
                var requestDto = new KuzuDBMQueryRequestDto
                {
                    Query = @"MATCH (m:Musician) RETURN m LIMIT 1;"
                };

                if (_kuzuDatabaseManager != null)
                {
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                    await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

                    // Act
                    var result = await _kuzuDatabaseManager.ExecuteQueryAsync(requestDto);

                    // Assert
                    Assert.NotNull(result);
                    Assert.Contains("id", result);
                    Log.Information("Test ExecuteQuery passed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Test ExecuteQuery_ValidQuery failed: {ex.Message}");
                throw;
            }
            // finally
            // {
            //     if (_kuzuDatabaseManager != null && _isConnected)
            //     {
            //         await _kuzuDatabaseManager.CloseAsync();
            //         _isConnected = false;
            //     }
            // }
        }

        #endregion


        #region CloseAsync Tests

        [SkippableFact]
        public async Task CloseAsync_ShouldCloseConnection_WhenCalled()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                Log.Information("Starting CloseAsync test...");

                if (_kuzuDatabaseManager != null)
                {
                    // Arrange & Act
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                    bool result = await _kuzuDatabaseManager.CloseAsync();

                    // Assert
                    Assert.True(result);
                    Log.Information("Test CloseAsync passed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Test CloseAsync_ShouldCloseConnection failed: {ex.Message}");
                throw;
            }
            // finally
            // {
            //     if (_kuzuDatabaseManager != null && _isConnected)
            //     {
            //         await _kuzuDatabaseManager.CloseAsync();
            //         _isConnected = false;
            //     }
            // }
        }

        #endregion
    }
}