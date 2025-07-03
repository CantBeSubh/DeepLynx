// using deeplynx.business;
// using deeplynx.datalayer.Models;
// using deeplynx.tests;
// using Testcontainers.PostgreSql;
// using Microsoft.EntityFrameworkCore;
// using Xunit;
//
// public class IntegrationTestBase : IAsyncLifetime
// {
//     
//     private readonly PostgreSqlContainer _container; 
//     protected DeeplynxContext Context { get; private set; }
//
//
//     protected IntegrationTestBase()
//     {
//         _container = new PostgreSqlBuilder()
//             .WithImage("postgres:15-alpine")
//             .Build();
//     }
//
//     public async virtual Task InitializeAsync()
//     {
//         await _container.StartAsync();
//
//         // Create DbContext with container connection string
//         var options = new DbContextOptionsBuilder<DeeplynxContext>()
//             .UseNpgsql(_container.GetConnectionString())
//             .Options;
//
//         Context = new DeeplynxContext(options);
//
//         // Ensure database is created
//         await Context.Database.EnsureCreatedAsync();
//         await CreateProceduresAsync();
//         await SeedData.SeedDatabase(Context);
//     }
//
//
//     public async Task DisposeAsync()
//      {
//          await Context.DisposeAsync();
//          await _container.DisposeAsync();
//      }
//
//      /// <summary>
//      /// Clean database between tests
//      /// </summary>
//      protected async Task CleanDatabaseAsync()
//      {
//          var projects = await Context.Projects.ToListAsync();
//          Context.Projects.RemoveRange(projects);
//          var datasources = await Context.DataSources.ToListAsync();
//          Context.DataSources.RemoveRange(datasources);
//          var classes = await Context.Classes.ToListAsync();
//          Context.Classes.RemoveRange(classes);
//          var records = await Context.Records.ToListAsync();
//          Context.Records.RemoveRange(records);
//          await Context.SaveChangesAsync();
//      }
//
//      private async Task CreateProceduresAsync()
//      {
//          var createProcedureScript = @"
//         SET search_path TO deeplynx;
//
// CREATE OR REPLACE PROCEDURE archive_project(arc_project_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
// LANGUAGE plpgsql AS $$
// BEGIN
// 	UPDATE deeplynx.projects SET archived_at = arc_time WHERE id = arc_project_id;
// 	UPDATE deeplynx.data_sources SET archived_at = arc_time WHERE project_id = arc_project_id;
// 	UPDATE deeplynx.records SET archived_at = arc_time WHERE project_id = arc_project_id;
// 	UPDATE deeplynx.edges SET archived_at = arc_time WHERE project_id = arc_project_id;
// 	UPDATE deeplynx.classes SET archived_at = arc_time WHERE project_id = arc_project_id;
// 	UPDATE deeplynx.relationships SET archived_at = arc_time WHERE project_id = arc_project_id;
// 	UPDATE deeplynx.edge_mappings SET archived_at = arc_time WHERE project_id = arc_project_id;
// 	UPDATE deeplynx.record_mappings SET class_id = 2 WHERE project_id = 1;
// 	UPDATE deeplynx.tags SET archived_at = arc_time WHERE project_id = arc_project_id;
// END;
// $$;
//
// -- not including tags -> record mappings since we want that to be many to many
// -- not including data sources -> records/edges because an archived data source won't necessarily archive a record/edge
//
// CREATE OR REPLACE PROCEDURE archive_record(arc_record_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
// LANGUAGE plpgsql AS $$
// BEGIN
// 	UPDATE deeplynx.records SET archived_at = arc_time WHERE id = arc_record_id;
// 	UPDATE deeplynx.edges SET archived_at = arc_time WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
// END;
// $$;
//
// -- not including classes -> record because an archived class won't necessarily archive a record
// CREATE OR REPLACE PROCEDURE archive_class(arc_class_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
// LANGUAGE plpgsql AS $$
// BEGIN
// 	UPDATE deeplynx.classes SET archived_at = arc_time WHERE id = arc_class_id;
// 	UPDATE deeplynx.relationships SET archived_at = arc_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
// 	UPDATE deeplynx.edge_mappings SET archived_at = arc_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
// 	UPDATE deeplynx.record_mappings SET archived_at = arc_time WHERE class_id = arc_class_id;
// END;
// $$;
//
// -- not including relationships -> edge because an archived relationship won't necessarily archive an edge
// CREATE OR REPLACE PROCEDURE deeplynx.archive_relationship(arc_rel_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
// LANGUAGE plpgsql AS $$
// BEGIN
// 	UPDATE deeplynx.relationships SET archived_at = arc_time WHERE id = arc_rel_id;
// 	UPDATE deeplynx.edge_mappings SET archived_at = arc_time WHERE relationship_id = arc_rel_id;
// END;
// $$;
//     ";
//          
//          await Context.Database.ExecuteSqlRawAsync(createProcedureScript);
//      }
// }

using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.tests;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container; 
    protected DeeplynxContext Context { get; private set; }

    protected IntegrationTestBase()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();
    }

    public async virtual Task InitializeAsync()
    {
        await _container.StartAsync();

        // Create DbContext with container connection string
        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        Context = new DeeplynxContext(options);

        // Apply migrations to ensure database is created and up-to-date
        await Context.Database.MigrateAsync();
        await SeedData.SeedDatabase(Context);
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Clean database between tests
    /// </summary>
    protected async Task CleanDatabaseAsync()
    {
        var projects = await Context.Projects.ToListAsync();
        Context.Projects.RemoveRange(projects);
        var datasources = await Context.DataSources.ToListAsync();
        Context.DataSources.RemoveRange(datasources);
        var classes = await Context.Classes.ToListAsync();
        Context.Classes.RemoveRange(classes);
        var records = await Context.Records.ToListAsync();
        Context.Records.RemoveRange(records);
        await Context.SaveChangesAsync();
    }
}
