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
        public async Task FullTextSearchForSpecificCloneResultsIn1Async()
        {
            var result = await _queryBusiness.Search("rex");
            result.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task FullTextSearchForCloneForce99TagResultsIn4Async()
        {
            var result = await _queryBusiness.Search("99");
            result.Should().HaveCount(4);
        }
        
        [Fact]
        public async Task FullTextSearchForCloneOriginalIdResultsIn1Async()
        {
            var result = await _queryBusiness.Search("CT-9901");
            result.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task QueryBuilderFindSpecificCloneWithCommonDataSourceAndOriginalIdResultsIn1Async()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "DataSourceName", Operator = "LIKE", Value = "R2D2"
            };
            var result = _queryBusiness.QueryBuilder([dto], "CT-7567");
            result.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task QueryBuilderFindSpecificCloneBetweenTwoTimesResultsIn1Async()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "LastUpdatedAt", Operator = "<", Value = "2024-01-30"
            };
            var dto2 = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "LastUpdatedAt", Operator = ">", Value = "2024-01-21"
            };
            var result = _queryBusiness.QueryBuilder([dto, dto2], null);
            result.Should().HaveCount(1);
        }
        
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            var baseTime = DateTime.SpecifyKind(new DateTime(2024, 1, 1, 12, 0, 0), DateTimeKind.Unspecified);
            var time1 = DateTime.SpecifyKind(new DateTime(2024, 1, 5, 10, 30, 0), DateTimeKind.Unspecified);
            var time2 = DateTime.SpecifyKind(new DateTime(2024, 1, 10, 14, 15, 0), DateTimeKind.Unspecified);
            var time3 = DateTime.SpecifyKind(new DateTime(2024, 1, 15, 9, 45, 0), DateTimeKind.Unspecified);
            var time4 = DateTime.SpecifyKind(new DateTime(2024, 1, 20, 16, 20, 0), DateTimeKind.Unspecified);
            var time5 = DateTime.SpecifyKind(new DateTime(2024, 1, 25, 11, 10, 0), DateTimeKind.Unspecified);
            var time6 = DateTime.SpecifyKind(new DateTime(2024, 1, 30, 13, 30, 0), DateTimeKind.Unspecified);

            var project = new Project
            {
                Name = "Anakin",
                Description = "You turned her against me",
                LastUpdatedAt = baseTime
            };
            await Context.Projects.AddAsync(project);
            await Context.SaveChangesAsync();

            var tag = new Tag
            {
                Name = "Padme",
                ProjectId = project.Id,
                LastUpdatedAt = time1
            };

            var dataSource = new DataSource
            {
                Name = "R2D2",
                Description = "Weeeeeeeee!",
                ProjectId = project.Id,
                LastUpdatedAt = time2
            };
            await Context.DataSources.AddAsync(dataSource);

            var testClass = new Class
            {
                Name = "Darth Maul",
                Description = "My legs!",
                ProjectId = project.Id,
                LastUpdatedAt = time3
            };
            await Context.Classes.AddAsync(testClass);
            await Context.SaveChangesAsync();

            var rex = new Record
            {
                Name = "Captain Rex",
                Description = "Clankers!",
                OriginalId = "CT-7567",
                Properties = JsonSerializer.Serialize(new { Legion = "501st" }),
                ProjectId = project.Id,
                DataSourceId = dataSource.Id,
                ClassId = testClass.Id,
                LastUpdatedAt = time1,
                Tags =  new List<Tag> { tag },
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(rex);

            var hunter = new Record
            {
                Name = "Hunter",
                Description = "Omega, stop doing that",
                OriginalId = "CT-9901",
                Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
                ProjectId = project.Id,
                DataSourceId = dataSource.Id,
                ClassId = testClass.Id,
                LastUpdatedAt = time2,
                Tags =  new List<Tag> { tag },
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(hunter);

            var tech = new Record
            {
                Name = "Tech",
                Description = "RIP",
                OriginalId = "CT-9902",
                Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
                ProjectId = project.Id,
                DataSourceId = dataSource.Id,
                ClassId = testClass.Id,
                LastUpdatedAt = time4,
                Tags =  new List<Tag> { tag },
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(tech);

            var wrecker = new Record
            {
                Name = "Wrecker",
                Description = "Boom",
                OriginalId = "CT-9903",
                Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
                ProjectId = project.Id,
                DataSourceId = dataSource.Id,
                ClassId = testClass.Id,
                LastUpdatedAt = time5,
                Tags =  new List<Tag> { tag },
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(wrecker);

            var crosshair = new Record
            {
                Name = "Crosshair",
                Description = "Redemption Arch",
                OriginalId = "CT-9904",
                Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
                ProjectId = project.Id,
                DataSourceId = dataSource.Id,
                ClassId = testClass.Id,
                LastUpdatedAt = time6,
                Tags =  new List<Tag> { tag },
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(crosshair);
            await Context.SaveChangesAsync();
        }
    }
}