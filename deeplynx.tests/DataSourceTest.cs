using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using deeplynx.models;

namespace deeplynx.tests;

public class DataSourceTest : IAsyncLifetime
{
    private DeeplynxContext _context;
    private DataSourceBusiness _business;
    private EdgeBusiness _edgeBusiness;
    private RecordBusiness _recordBusiness;
    private readonly PostgreSqlContainer _postgresContainer;
    
    public DataSourceTest()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _context = new DeeplynxContext(options);
        await _context.Database.MigrateAsync();

        _business = new DataSourceBusiness(_context, _edgeBusiness, _recordBusiness);
    }
    
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
    
    private void SeedDataSource()
    {
        var project = new Project { Id = 1, Name = "Project1" };

        _context.Projects.Add(project);

        _context.DataSources.AddRange(new List<DataSource>
        {
            new DataSource 
            { 
                Id = 1, 
                Name = "DataSource1", 
                ProjectId = project.Id, 
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), 
                CreatedBy = "TestUser",
                Project = project
            },
            new DataSource 
            { 
                Id = 2, 
                Name = "DataSource2", 
                ProjectId = project.Id, 
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), 
                CreatedBy = "TestUser",
                Project = project
            }
        });
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task GetAllDataSources_ReturnsAllDataSources()
    { 
        SeedDataSource();
        // Act
        var result = await _business.GetAllDataSources(1);
        
        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("DataSource1", resultList[0].Name);
        Assert.Equal("DataSource2", resultList[1].Name);
        Assert.Equal(1, resultList[0].ProjectId);
        Assert.Equal(1, resultList[1].ProjectId);
    }
    
    [Fact]
    public async Task UpdateDataSource_UpdatesAndReturnsUpdatedDataSource()
    {
        SeedDataSource();
        // Arrange
        var updateDto = new DataSourceRequestDto()
        {
            Name = "UpdatedName",
            Abbreviation = "UpdAbbr",
            Type = "UpdatedType",
            BaseUri = "http://updated.uri",
            Config = new JsonObject { ["a"] = "b" }
        };

        // Act
        var result = await _business.UpdateDataSource(1, 1,updateDto);

        // Assert
        Assert.Equal(updateDto.Name, result.Name);
        Assert.Equal(updateDto.Abbreviation, result.Abbreviation);
        Assert.Equal(updateDto.Type, result.Type);
        Assert.Equal(updateDto.BaseUri, result.BaseUri);
        Assert.Equal(updateDto.Config.ToString(), result.Config.ToString());
        
    }
}