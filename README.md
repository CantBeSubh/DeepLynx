# DeepLynx Nexus
## Deployment
### Prerequisites
1. PostgreSQL: Ensure PostgreSQL is installed and running on your system. Download [PostgreSQL](https://www.postgresql.org/)

2. .NET SDK: Ensure .NET SDK version 9.0 is installed on your system. Download [.NET 9.0](https://dotnet.microsoft.com/en-us/).

### Install
1. PostgreSQL Setup:
    * Install and launch PostgreSQL.
    * Create a PostgreSQL server. 
    * Add credentials (Username/Password) for the newly created PostgreSQL server to the connection string in appsettings.json. 

2. .NET SDK Setup:

* Install the .NET SDK version 9.0.
3. Entity Framework CLI:

    * Install the .NET Entity Framework CLI tool globally:
```
dotnet tool install --global dotnet-ef
```
## Load the Database
1. Navigate to the datalayer folder in your project directory.

2. Run the following command to apply the latest migrations and update the database:

```
dotnet ef database update -c DeeplynxContext
```
## Development
### Create Migration
If you make changes to the database schema, create a new database migration with a descriptive name. For example, to add a migration for updating the users table, run:
```
dotnet ef migrations add UpdateUsersExample -c DeeplynxContext
```
### Additional Notes
* Ensure that your PostgreSQL server is running before attempting to update the database or create migrations.
