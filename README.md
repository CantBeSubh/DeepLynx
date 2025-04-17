# DeepLynx Nexus
## Deployment

### Install
Install and launch [PostgreSQL](https://www.postgresql.org/)

### Load the database 
dotnet ef database update -c DeeplynxContext


## Development

### Create Migration 
If database changes are made. Create a datbase migration in the format ```[triple_sequential_num]_[topic]_[mm/dd/yy]```
```
dotnet ef migrations add 003_Example-120125 -c DeeplynxContext
```
