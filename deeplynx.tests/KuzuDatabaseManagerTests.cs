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
using Testcontainers.PostgreSql;
using deeplynx.helpers;

namespace deeplynx.tests {
    [Collection("Test Suite Collection")]
    public class KuzuDatabaseManagerTests : IntegrationTestBase
    {
        private readonly PostgreSqlContainer _container;
        private readonly TestSuiteFixture _fixture;
        private KuzuDatabaseManager? _kuzuDatabaseManager;
        private readonly IConfiguration uration;
        public string ConnectionString { get; private set; }
        private const int ProjectId = 1;
        private bool _isConnected = false;
        private static readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);

        public KuzuDatabaseManagerTests(TestSuiteFixture fixture) : base(fixture)
        {
            _fixture = fixture;

            _container = new PostgreSqlBuilder().WithImage("postgres:15-alpine").Build();

            ConnectionString = _fixture.PostgresConnectionString;
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\test-log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            uration = builder.Build();
        }

        public override async Task InitializeAsync()
        {
            // await _container.StartAsync();
            var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));

            var kuzuDbFilePath = Path.Combine(projectRoot, "deeplynx.tests/bin/Debug/deeplynx.graph");

            if (System.IO.Directory.Exists(kuzuDbFilePath))
                System.IO.Directory.Delete(kuzuDbFilePath, true);

            _ = bool.TryParse(Config.Instance.ENABLE_KUZU, out var enableKuzu);

            if (enableKuzu)
            {
                _kuzuDatabaseManager = new KuzuDatabaseManager(uration, ConnectionString ?? "NULL", "g72g72");
            }

            if (enableKuzu && _kuzuDatabaseManager != null)
            {
                Log.Information("Attempting to acquire initialize semaphore lock");
                await _connectionSemaphore.WaitAsync();
                Log.Information("Initialize lock acquired");



                try
                {
                    if (!_isConnected)
                    {
                        _ = await _kuzuDatabaseManager.ConnectAsync();
                        _isConnected = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"An error occurred during initialization: {ex.Message}");
                }
                finally
                {
                    _ = _connectionSemaphore.Release();
                    Log.Information("Initialize lock released");
                }
            }
        }

        public async Task DisposesAsync()
        {
            if (_kuzuDatabaseManager != null && _isConnected)
            {
                _ = await _kuzuDatabaseManager.CloseAsync();
                _isConnected = false;
            }
            await base.DisposeAsync();
        }

        private async Task<bool> ExecuteSqlFromFileAsync(string filePath)
        {
            ConnectionString = _fixture.PostgresConnectionString;

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

            using (var connection = new NpgsqlConnection(ConnectionString))
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
                Skip.If(!bool.TryParse(Config.Instance.ENABLE_KUZU, out var enableKuzu) || !enableKuzu, "Kuzu tests are disabled until the random crash bug is fixed. Please refer to the README.md local setup instructions to setup Kuzu.");

                Log.Information("Starting InitalSeedDatabaseAsync test...");

                if (_kuzuDatabaseManager != null)
                {
                    // Arrange
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");

                    // Assert
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    Assert.True(result);
                    Log.Information("Test InitalSeedDatabaseAsync passed.");
                }
                
            }
            catch (Exception ex)
            {
                Log.Error($"Test InitalSeedDatabaseAsync failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    _ = await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
                await DisposesAsync();
            }
        }

        [SkippableFact]
        public async Task SeedDatabaseAsync()
        {
            try
            {
                Skip.If(!bool.TryParse(Config.Instance.ENABLE_KUZU, out var enableKuzu));

                Log.Information("Starting SeedDatabaseAsync test...");

                if (_kuzuDatabaseManager != null && enableKuzu)
                {
                   // Arrange
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    bool result = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                    // Assert
                    Assert.True(result);
                    Log.Information("Test SeedDatabaseAsync passed."); 
                }
                
            }
            catch (Exception ex)
            {
                Log.Error($"Test SeedDatabaseAsync failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    _ = await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
                await DisposesAsync();
            }
        }

        #region ConnectAsync Tests

        [SkippableFact]
        public async Task ConnectAsync_ShouldInitializeDatabase_WhenNotInitialized()
        {
            try
            {
                Skip.If(!bool.TryParse(Config.Instance.ENABLE_KUZU, out var enableKuzu));

                Log.Information("Starting ConnectAsync test...");

                if (_kuzuDatabaseManager != null && enableKuzu)
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
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    _ = await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
                await DisposesAsync();
            }
        }

        #endregion

        #region ExportDataAsync Tests

        [SkippableFact]
        public async Task ExportDataAsync_ShouldReturnFalse_WhenExportIsSuccessful()
        {
            try
            {
                Skip.If(!bool.TryParse(Config.Instance.ENABLE_KUZU, out var enableKuzu));

                Log.Information("Starting ExportDataAsync test...");

                if (_kuzuDatabaseManager != null)
                {
                    // Arrange & Act
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

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
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    _ = await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
                await DisposesAsync();
            }
        }

        #endregion


        #region GetNodesWithinDepthByIdAsync Tests

        [SkippableFact]
        public async Task GetNodesWithinDepthByIdAsync_ValidInput_ReturnsExpectedResults()
        {
            try
            {
                Skip.If(!bool.TryParse(Config.Instance.ENABLE_KUZU, out var enableKuzu));

                Log.Information("Starting GetNodesWithin test...");

                // Arrange
                var requestDto = new KuzuDBMNodesWithinDepthRequestDto
                {
                    TableName = "Song",
                    Id = 3,
                    Depth = 2,
                };

                if (_kuzuDatabaseManager != null)
                {
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                    _ = await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

                    // Act
                    var (result, formattedString) = await _kuzuDatabaseManager.GetNodesWithinDepthByIdAsync(requestDto);

                    // Assert
                    Assert.NotNull(formattedString);
                    Assert.Contains("id:", formattedString);
                    Log.Information("Test GetNodesWithin passed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Test GetNodesWithinDepthByIdAsync failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    _ = await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
                await DisposesAsync();
            }
        }

        #endregion


        #region ExecuteQuery Tests

        [SkippableFact]
        public async Task ExecuteQuery_ValidQuery_ReturnsExpectedResults()
        {
            try
            {
                Skip.If(!bool.TryParse(Config.Instance.ENABLE_KUZU, out var enableKuzu));

                Log.Information("Starting ExecuteQuery test...");

                // Arrange
                var requestDto = new KuzuDBMQueryRequestDto
                {
                    Query = @"MATCH (m:Musician) RETURN m LIMIT 1;"
                };

                if (_kuzuDatabaseManager != null)
                {
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

                    _ = await _kuzuDatabaseManager.ExportDataAsync(ProjectId);

                    // Act
                    var (formattedString, result) = await _kuzuDatabaseManager.ExecuteQueryAsync(requestDto);

                    // Assert
                    Assert.NotNull(formattedString);
                    Assert.Contains("id", formattedString);
                    Log.Information("Test ExecuteQuery passed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Test ExecuteQuery_ValidQuery failed: {ex.Message}");
                throw;
            }
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    _ = await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
                await DisposesAsync();
            }
        }

        #endregion


        #region CloseAsync Tests

        [SkippableFact]
        public async Task CloseAsync_ShouldCloseConnection_WhenCalled()
        {
            try
            {
                Skip.If(!bool.TryParse(Config.Instance.ENABLE_KUZU, out var enableKuzu));

                Log.Information("Starting CloseAsync test...");

                if (_kuzuDatabaseManager != null)
                {
                    // Arrange & Act
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/clear_database.sql");
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/initial_test_data_inserts.sql");
                    _ = await ExecuteSqlFromFileAsync("../../../../deeplynx.graph/SeedData/test_data_sql_inserts.sql");

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
            finally
            {
                if (_kuzuDatabaseManager != null && _isConnected)
                {
                    _ = await _kuzuDatabaseManager.CloseAsync();
                    _isConnected = false;
                }
                await DisposesAsync();
            }
        }

        #endregion
    }
}

