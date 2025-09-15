using System;
using System.Text.Json;
using System.Threading.Tasks;
using deeplynx.business;
using FluentAssertions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class QueryBusinessTests : IntegrationTestBase
    {
        private QueryBusiness _queryBusiness = null!;
        public long pid;
        private DateTime now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        private long mockUserId;
        private long mockUser2Id;
        private long mockActionId;
        private long mockDataSourceId;
        private long mockDataSource2Id;

        public QueryBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _queryBusiness = new QueryBusiness(Context);
        }

        [Fact]
        public async Task ConfirmResultsFromFullTextSearchAsync()
        {
            // await SeedTestDataAsync();
            // var project = new Project
            // {
            //     Name = "Anakin",
            //     Description = "You turned her against me",
            //     LastUpdatedAt = now
            // };
            // await Context.Projects.AddAsync(project);
            // await Context.SaveChangesAsync();
            //
            // var tag = new Tag
            // {
            //     Name = "Padme",
            //     ProjectId = project.Id,
            //     LastUpdatedAt = now
            // };
            //
            // var dataSource = new DataSource
            // {
            //     Name = "R2D2",
            //     Description = "Weeeeeeeee!",
            //     ProjectId = project.Id,
            //     LastUpdatedAt = now
            // };
            // await Context.DataSources.AddAsync(dataSource);
            //
            // var testClass = new Class
            // {
            //     Name = "Darth Maul",
            //     Description = "My legs!",
            //     ProjectId = project.Id,
            //     LastUpdatedAt = now
            // };
            // await Context.Classes.AddAsync(testClass);
            // await Context.SaveChangesAsync();
            //
            // var rex = new Record
            // {
            //     Name = "Captain Rex",
            //     Description = "Clankers!",
            //     OriginalId = "CT-7567",
            //     Properties = JsonSerializer.Serialize(new { Legion = "501st" }),
            //     ProjectId = project.Id,
            //     DataSourceId = dataSource.Id,
            //     ClassId = testClass.Id,
            //     LastUpdatedAt = now,
            //     Tags =  new List<Tag> { tag },
            //     Uri = "localhost:8090"
            // };
            // await Context.Records.AddAsync(rex);
            //
            // var hunter = new Record
            // {
            //     Name = "Hunter",
            //     Description = "Clankers!",
            //     OriginalId = "CT-9901",
            //     Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
            //     ProjectId = project.Id,
            //     DataSourceId = dataSource.Id,
            //     ClassId = testClass.Id,
            //     LastUpdatedAt = now,
            //     Tags =  new List<Tag> { tag },
            //     Uri = "localhost:8090"
            // };
            // await Context.Records.AddAsync(hunter);
            //
            // var tech = new Record
            // {
            //     Name = "Tech",
            //     Description = "RIP",
            //     OriginalId = "CT-9902",
            //     Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
            //     ProjectId = project.Id,
            //     DataSourceId = dataSource.Id,
            //     ClassId = testClass.Id,
            //     LastUpdatedAt = now,
            //     Tags =  new List<Tag> { tag },
            //     Uri = "localhost:8090"
            // };
            // await Context.Records.AddAsync(tech);
            //
            // var wrecker = new Record
            // {
            //     Name = "Wrecker",
            //     Description = "Boom",
            //     OriginalId = "CT-9903",
            //     Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
            //     ProjectId = project.Id,
            //     DataSourceId = dataSource.Id,
            //     ClassId = testClass.Id,
            //     LastUpdatedAt = now,
            //     Tags =  new List<Tag> { tag },
            //     Uri = "localhost:8090"
            // };
            // await Context.Records.AddAsync(wrecker);
            //
            // var crosshair = new Record
            // {
            //     Name = "Crosshair",
            //     Description = "Kind of good",
            //     OriginalId = "CT-9904",
            //     Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
            //     ProjectId = project.Id,
            //     DataSourceId = dataSource.Id,
            //     ClassId = testClass.Id,
            //     LastUpdatedAt = now,
            //     Tags =  new List<Tag> { tag },
            //     Uri = "localhost:8090"
            // };
            // await Context.Records.AddAsync(crosshair);
            // await Context.SaveChangesAsync();
            //
            // var result = await _queryBusiness.Search("rex");
            // result.Should().HaveCount(1);
        }
    }
}