# DeepLynx Nexus
## Prerequisites
1. Postgres download:
    * Download [PostgreSQL](https://www.postgresql.org/) natively, OR
    * Download [Docker](https://docs.docker.com/engine/install/)

2. .NET SDK: Ensure .NET SDK version 9.0 is installed on your system. Download [.NET 9.0](https://dotnet.microsoft.com/en-us/).

## Docker Setup
1. Environment variables:
    * Create a .env file in the root directory
    * Copy the contents of .env_sample to .env
    * Make any necessary changes

2. Docker:
This application can be run from docker using the following docker commands:
```
docker compose -f docker-compose.yaml build
docker compose -f docker-compose.yaml up
```
Docker users can skip the steps in [Load the Database](#load-the-database), as `database/migration.sql` is automatically applied during container creation. 

## Local Setup
1. PostgreSQL Setup:
    * Native Install:
        * Install and launch PostgreSQL.
        * Create a PostgreSQL server.
    * Docker Install:
        * Run the following command: `docker run --name DeepLynx -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=deeplynx -d -p 5432:5432 postgres`
    * Add credentials (Username/Password) for the newly created PostgreSQL server to the connection string in appsettings.json. For example: 
        * `"User ID=postgres;Database=deeplynx;Password=postgres;Server=localhost;Port=5432;"`

2. .NET SDK Setup:
    * Install the .NET SDK version 9.0.

3. Entity Framework CLI:

    * Install the .NET Entity Framework CLI tool globally:
        * `dotnet tool install --global dotnet-ef`

## Development

### Load the Database
1. Navigate to the root folder in your project directory.

2. Run the following command to apply the latest migrations and update the database:

```
dotnet ef database update -c DeeplynxContext --verbose --project deeplynx.datalayer --startup-project deeplynx.api
```

If the above command fails with a `Could not exeucte` or similar message, `dotnet ef` may need to be added to the PATH.  
Please update your path to include the .NET tools directory, similar to: `export PATH="$PATH:/Users/_username_/.dotnet/tools"`

### Create Migration
If you make changes to the datalayer, create a new database migration with a descriptive name. For example, to add a migration for updating the users table, run:
```
dotnet ef migrations add UpdateUsersExample -c DeeplynxContext --verbose --project deeplynx.datalayer --startup-project deeplynx.api
```

Additionally, please update the `migration.sql` file used for applying migrations to Docker containers using the following command from the project root:
```
dotnet ef migrations script -o database/migration.sql --project deeplynx.datalayer --startup-project deeplynx.api --idempotent
```
This creates an idempotent migration script that will only apply missing migrations to the database.  

See [CONTRIBUTING](./CONTRIBUTING.md) for more details.

### Additional Notes
* Ensure that your PostgreSQL server is running before attempting to update the database or create migrations.
