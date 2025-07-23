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

            var envFilePath = Path.Combine(projectRoot, ".env_sample");

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
            bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu);

            if (enableKuzu && _kuzuDatabaseManager != null)
            {
                Console.WriteLine("Attempting to acquire initialize semaphore lock");
                await _connectionSemaphore.WaitAsync();
                Console.WriteLine("Initialize lock acquired");

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
                    Console.WriteLine($"An error occurred during initialization: {ex.Message}");
                }
                finally
                {
                    _connectionSemaphore.Release();
                    Console.WriteLine("Initialize lock released");
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
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database operation failed: {ex.Message}");
                    return false;
                }
            }
        }

        [SkippableFact]
        public async Task InitalSeedDatabaseAsync()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled until the random crash bug is fixed. Please refer to the README.md local setup instructions to setup Kuzu.");

                // Arrange
                bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");

                // Assert
                await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                Assert.True(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test InitalSeedDatabaseAsync failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
            }
        }

        [SkippableFact]
        public async Task SeedDatabaseAsync()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                // Arrange
                await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                // Assert
                Assert.True(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test SeedDatabaseAsync failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
            }
        }

        #region ConnectAsync Tests

        [SkippableFact]
        public async Task ConnectAsync_ShouldInitializeDatabase_WhenNotInitialized()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                if (_kuzuDatabaseManager != null)
                {
                    // Arrange & Act
                    bool connected = await _kuzuDatabaseManager.ConnectAsync();

                    // Assert
                    Assert.True(connected);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test ConnectAsync_ShouldInitializeDatabase failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
            }
        }

        #endregion

        #region ExportDataAsync Tests

        [SkippableFact]
        public async Task ExportDataAsync_ShouldReturnTrue_WhenExportIsSuccessful()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                if (_kuzuDatabaseManager != null)
                {
                    // Arrange & Act
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                    bool result = await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

                    // Assert
                    Assert.True(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test ExportDataAsync_ShouldReturnTrue failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
            }
        }

        #endregion


        #region GetNodesWithinDepthByIdAsync Tests

        [SkippableFact]
        public async Task GetNodesWithinDepthByIdAsync_ValidInput_ReturnsExpectedResults()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                // Arrange
                var requestDto = new KuzuDBMNodesWithinDepthRequestDto
                {
                    TableName = "Musician",
                    Id = 1,
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test GetNodesWithinDepthByIdAsync failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
            }
        }

        #endregion


        #region ExecuteQuery Tests

        [SkippableFact]
        public async Task ExecuteQuery_ValidQuery_ReturnsExpectedResults()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test ExecuteQuery_ValidQuery failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
            }
        }

        #endregion


        #region CloseAsync Tests

        [SkippableFact]
        public async Task CloseAsync_ShouldCloseConnection_WhenCalled()
        {
            try
            {
                Skip.If(!bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_KUZU"), out var enableKuzu));

                if (_kuzuDatabaseManager != null)
                {
                    // Arrange & Act
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                    bool result = await _kuzuDatabaseManager.CloseAsync();

                    // Assert
                    Assert.True(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test CloseAsync_ShouldCloseConnection failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
            }
        }

        #endregion
    }
}
