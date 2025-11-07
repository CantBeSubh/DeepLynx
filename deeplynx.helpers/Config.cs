namespace deeplynx.helpers;

public class Config
{
    // DATABASE CONFIGURATION
    public string POSTGRES_USER { get; }
    public string POSTGRES_PASSWORD { get; }
    public string POSTGRES_DB_NAME { get; }
    public string POSTGRES_DB_HOST { get; }
    public string POSTGRES_POST { get; }

    // CACHE CONFIGURATION
    public string CACHE_PROVIDER_TYPE { get; }
    public string REDIS_CONNECTION_STRING { get; }

    // USER MANAGEMENT CONFIGURATION
    public string SUPERUSER_EMAIL { get; }

    // GRAPH DATABASE CONFIGURATION
    public string ENABLE_KUZU { get; }

    // DUCKDB CONFIGURATION
    public string DUCKDB_BASE_PATH { get; }

    // FILE STORAGE CONFIGURATION
    // Available options: filesystem, azure_object, aws_s3
    public string FILE_STORAGE_METHOD { get; }
    public string STORAGE_DIRECTORY { get; }

    // Azure Object Storage (when FILE_STORAGE_METHOD=azure_object)
    public string AZURE_OBJECT_CONNECTION_STRING { get; }

    // AWS S3 Storage (when FILE_STORAGE_METHOD=aws_s3)
    public string AWS_S3_CONNECTION_STRING { get; }

    // EMAIL CONFIGURATION
    public string SMTP_SERVER { get; }
    public string SMTP_PORT { get; }
    public string SMTP_ENABLE_SSL { get; }
    public string FROM_EMAIL { get; }
    public string FROM_NAME { get; }
    public string INVITE_URL { get; }
    public string SUPPORT_EMAIL { get; }

    // OKTA JWT CONFIGURATION
    public string JWT_SECRET_KEY { get; }
    public string JWT_ISSUER { get; }
    public string JWT_AUDIENCE { get; }

    // Local Development
    public string DISABLE_BACKEND_AUTHENTICATION { get; }
    public string ENABLE_NOTIFICATION_SERVICE { get; }

    public Config()
    {
        // DATABASE CONFIGURATION
        POSTGRES_USER = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
        POSTGRES_PASSWORD = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
        POSTGRES_DB_NAME = Environment.GetEnvironmentVariable("POSTGRES_DB_NAME") ?? "deeplynx";
        POSTGRES_DB_HOST = Environment.GetEnvironmentVariable("POSTGRES_DB_HOST") ?? "localhost";
        POSTGRES_POST = Environment.GetEnvironmentVariable("POSTGRES_POST") ?? "5432";

        // CACHE CONFIGURATION
        CACHE_PROVIDER_TYPE = Environment.GetEnvironmentVariable("CACHE_PROVIDER_TYPE") ?? "memory";
        REDIS_CONNECTION_STRING = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? string.Empty;

        // USER MANAGEMENT CONFIGURATION
        SUPERUSER_EMAIL = Environment.GetEnvironmentVariable("SUPERUSER_EMAIL") ?? "admin@admin.com";

        // GRAPH DATABASE CONFIGURATION
        ENABLE_KUZU = Environment.GetEnvironmentVariable("ENABLE_KUZU") ?? "false";

        // DUCKDB CONFIGURATION
        DUCKDB_BASE_PATH = Environment.GetEnvironmentVariable("DUCKDB_BASE_PATH") ?? "/data/duckdb";

        // FILE STORAGE CONFIGURATION
        // Available options: filesystem, azure_object, aws_s3
        FILE_STORAGE_METHOD = Environment.GetEnvironmentVariable("FILE_STORAGE_METHOD") ?? "filesystem";
        STORAGE_DIRECTORY = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? string.Empty;
        // Azure Object Storage (when FILE_STORAGE_METHOD=azure_object)
        AZURE_OBJECT_CONNECTION_STRING =
            Environment.GetEnvironmentVariable("AZURE_OBJECT_CONNECTION_STRING") ?? string.Empty;
        // AWS S3 Storage (when FILE_STORAGE_METHOD=aws_s3)
        AWS_S3_CONNECTION_STRING = Environment.GetEnvironmentVariable("AWS_S3_CONNECTION_STRING") ?? string.Empty;

        // EMAIL CONFIGURATION
        SMTP_SERVER = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "mailhost.inl.gov";
        SMTP_PORT = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "25";
        SMTP_ENABLE_SSL = Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "false";
        FROM_EMAIL = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? "no-reply@inl.gov";
        FROM_NAME = Environment.GetEnvironmentVariable("FROM_NAME") ?? "DeepLynx Nexus";
        INVITE_URL = Environment.GetEnvironmentVariable("INVITE_URL") ?? "http://localhost:3000";
        SUPPORT_EMAIL = Environment.GetEnvironmentVariable("SUPPORT_EMAIL") ?? "jaren.brownlee@inl.gov";

        // OKTA JWT CONFIGURATION
        JWT_SECRET_KEY = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? string.Empty;
        JWT_ISSUER = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? string.Empty;
        JWT_AUDIENCE = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? string.Empty;

        // Local Development
        DISABLE_BACKEND_AUTHENTICATION =
            Environment.GetEnvironmentVariable("DISABLE_BACKEND_AUTHENTICATION") ?? "false";
        ENABLE_NOTIFICATION_SERVICE = Environment.GetEnvironmentVariable("ENABLE_NOTIFICATION_SERVICE") ?? "false";
    }
}