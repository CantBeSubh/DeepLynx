using DotNetEnv;

using deeplynx.api;

public static class ConnectionStringsProvider
{
    public static string GetPostgresConnectionString(IConfiguration configuration)
    {
        Env.Load();

        var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var postgresDatabaseName = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var postgresServer = Environment.GetEnvironmentVariable("POSTGRES_DBHOST");
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");

        if (!string.IsNullOrEmpty(postgresUser) &&
            !string.IsNullOrEmpty(postgresPassword) &&
            !string.IsNullOrEmpty(postgresDatabaseName) &&
            !string.IsNullOrEmpty(postgresServer) &&
            !string.IsNullOrEmpty(postgresPort))
        {
            NLog.LogManager.GetCurrentClassLogger().Info("Using .env postgres connection credentials");
            return $"User ID={postgresUser};Password={postgresPassword};Database={postgresDatabaseName};Server={postgresServer};Port={postgresPort};";
        }
        else if (!string.IsNullOrEmpty(defaultConnectionString))
        {
            NLog.LogManager.GetCurrentClassLogger().Info(".env postgres connection variables not configured. Falling back to default postgres connection credentials");
            return defaultConnectionString;
        }
        else
        {
            throw new InvalidOperationException("No valid database connection string was found.");
        }
    }
}