using DotNetEnv;
using Serilog;

/// <summary>
///  Provide a singular provider class to configure external deeplynx connections
/// </summary>
public static class ConnectionStringsProvider
{
    /// <summary>
    /// Return a postgresql database connection string using the DotNetEnv package.
    /// </summary>
    /// <param name="configuration">Get the default project configuration to use appsettings string as fallback string.</param>
    /// <returns>A string holding the valid postgresql connection string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when valid postgres connection string cannot be created.</exception>
    public static string GetPostgresConnectionString(IConfiguration configuration)
    {
        Env.Load("../.env");

        var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var postgresDatabaseName = Environment.GetEnvironmentVariable("POSTGRES_DB_NAME");
        var postgresServer = Environment.GetEnvironmentVariable("POSTGRES_DBHOST");
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");

        if (!string.IsNullOrEmpty(postgresUser) &&
            !string.IsNullOrEmpty(postgresPassword) &&
            !string.IsNullOrEmpty(postgresDatabaseName) &&
            !string.IsNullOrEmpty(postgresServer) &&
            !string.IsNullOrEmpty(postgresPort))
        {
            Log.Information("Using .env postgres connection credentials.");
            return $"User ID={postgresUser};Password={postgresPassword};Database={postgresDatabaseName};Server={postgresServer};Port={postgresPort};";
        }
        else if (!string.IsNullOrEmpty(defaultConnectionString))
        {
            Log.Information(".env postgres connection variables not configured. Falling back to default postgres connection credentials.");
            return defaultConnectionString;
        }
        else
        {
            throw new InvalidOperationException("No valid database connection string was found.");
        }
    }
}