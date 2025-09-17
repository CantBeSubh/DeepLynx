using System;
using System.ComponentModel.DataAnnotations;
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
                Connector = "AND", Filter = "data_source_name", Operator = "LIKE", Value = "R2D2"
            };
            var result = _queryBusiness.QueryBuilder([dto], "CT-7567");
            result.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task QueryBuilderFindSpecificCloneBetweenTwoTimesResultsIn1Async()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "last_updated_at", Operator = "<", Value = "2024-01-30"
            };
            var dto2 = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "last_updated_at", Operator = ">", Value = "2024-01-21"
            };
            var result = _queryBusiness.QueryBuilder([dto, dto2], null);
            result.Should().HaveCount(1);
        }
        
        
        [Fact]
        public async Task FullTextSearchEmptyStringReturnsAllRecordsAsync()
        {
            await Assert.ThrowsAsync<Exception>(() => 
                _queryBusiness.Search(""));
        }
        
        
        [Fact]
        public async Task FullTextSearchNullThrowsExceptionAsync()
        {
            await Assert.ThrowsAsync<Exception>(() => 
                _queryBusiness.Search(null));
        }
        
        [Fact]
        public async Task FullTextSearchWhitespaceOnlyReturnsAllRecordsAsync()
        {
            await Assert.ThrowsAsync<Exception>(() => 
                    _queryBusiness.Search("     "));
        }
        
        [Fact]
        public async Task FullTextSearchCaseInsensitiveReturnsCorrectResultsAsync()
        {
            var result = await _queryBusiness.Search("Rex");
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Captain Rex");
        }
        
        [Fact]
        public async Task FullTextSearchPartialMatchInDescriptionAsync()
        {
            var result = await _queryBusiness.Search("Omega");
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Hunter");
        }
        
        [Fact]
        public async Task FullTextSearchInPropertiesJsonAsync()
        {
            var result = await _queryBusiness.Search("501st");
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Captain Rex");
        }
        
        [Fact]
        public async Task FullTextSearchSpecialCharactersAsync()
        {
            var result = await _queryBusiness.Search("CT-");
            result.Should().HaveCount(5); // All clones have CT- prefix
        }
        
        [Fact]
        public async Task FullTextSearchNonExistentTermReturnsEmptyAsync()
        {
            var result = await _queryBusiness.Search("Jedi");
            result.Should().BeEmpty();
        }
        
        [Fact]
        public async Task QueryBuilderWithEmptyFiltersReturnsAllRecordsAsync()
        {
            var result = _queryBusiness.QueryBuilder(new CustomQueryRequestDto[0], null);
            result.Should().HaveCount(5);
        }
        
        [Fact]
        public async Task QueryBuilderWithNullFiltersReturnsAllRecordsAsync()
        {
            Assert.Throws<Exception>(() => 
                _queryBusiness.QueryBuilder(null, null));
        }
        
        [Fact]
        public async Task QueryBuilderEqualityOperatorAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "Name", Operator = "=", Value = "Tech"
            };
            var result = _queryBusiness.QueryBuilder([dto], null);
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Tech");
        }
        
        
        [Fact]
        public async Task QueryBuilderLikeOperatorCaseInsensitiveAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "Name", Operator = "LIKE", Value = "tech"
            };
            var result = _queryBusiness.QueryBuilder([dto], null);
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Tech");
        }
        
        // [Fact]
        // public async Task QueryBuilderGreaterThanDateOperatorAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "last_updated_at", Operator = ">", Value = "2024-01-15"
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto], null);
        //     result.Should().HaveCount(3); // Tech, Wrecker, Crosshair (after Jan 15)
        // }
        
        // [Fact]
        // public async Task QueryBuilderLessThanDateOperatorAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "last_updated_at", Operator = "<", Value = "2024-01-15"
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto], null);
        //     result.Should().HaveCount(2); // Rex, Hunter (before Jan 15)
        // }
        
        // [Fact]
        // public async Task QueryBuilderGreaterThanOrEqualDateOperatorAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "last_updated_at", Operator = ">", Value = "2024-01-20"
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto], null);
        //     result.Should().HaveCount(3); // Tech, Wrecker, Crosshair (on or after Jan 20)
        // }
        //
        // [Fact]
        // public async Task QueryBuilderLessThanOrEqualDateOperatorAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "last_updated_at", Operator = "<", Value = "2024-01-10"
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto], null);
        //     result.Should().HaveCount(2); // Rex, Hunter (on or before Jan 10)
        // }
        
        [Fact]
        public async Task QueryBuilderMultipleAndConditionsAsync()
        {
            var dto1 = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "data_source_name", Operator = "LIKE", Value = "R2D2"
            };
            var dto2 = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "original_id", Operator = "LIKE", Value = "CT-9902"
            };
            var result = _queryBusiness.QueryBuilder([dto1, dto2], null);
            result.Should().HaveCount(1); // Tech, Wrecker, Crosshair (after Jan 15 with R2D2 datasource)
        }
        
        [Fact]
        public async Task QueryBuilderOrConditionAsync()
        {
            var dto1 = new CustomQueryRequestDto
            {
                Connector = "", Filter = "name", Operator = "=", Value = "Tech"
            };
            var dto2 = new CustomQueryRequestDto
            {
                Connector = "OR", Filter = "name", Operator = "=", Value = "Wrecker"
            };
            var result = _queryBusiness.QueryBuilder([dto1, dto2]);
            result.Should().HaveCount(2); // Tech and Wrecker
        }
        
        [Fact]
        public async Task QueryBuilderNullAndOrConditionsAsync()
        {
            var dto1 = new CustomQueryRequestDto
            {
                Connector = null, Filter = "name", Operator = "LIKE", Value = "rex"
            };
            var dto2 = new CustomQueryRequestDto
            {
                Connector = "OR", Filter = "Name", Operator = "=", Value = "Tech"
            };
            var dto3 = new CustomQueryRequestDto
            {
                Connector = "OR", Filter = "Name", Operator = "=", Value = "Hunter"
            };
            var result = _queryBusiness.QueryBuilder([dto1, dto2, dto3], null);
            result.Should().HaveCount(3); 
        }
        
        [Fact]
        public async Task QueryBuilderMixedAndOrConditionsAsync()
        {
            var dto1 = new CustomQueryRequestDto
            {
                Connector = null, Filter = "project_name", Operator = "LIKE", Value = "Anakin"
            };
            var dto2 = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "Name", Operator = "=", Value = "Tech"
            };
            var dto3 = new CustomQueryRequestDto
            {
                Connector = "OR", Filter = "Name", Operator = "=", Value = "Hunter"
            };
            var result = _queryBusiness.QueryBuilder([dto1, dto2, dto3], null);
            result.Should().HaveCount(2); 
        }
        
        [Fact]
        public async Task QueryBuilderWithSearchTermCombinedAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "data_source_name", Operator = "LIKE", Value = "R2D2"
            };
            var result = _queryBusiness.QueryBuilder([dto], "99");
            result.Should().HaveCount(4); // All Bad Batch members with CloneForce 99
        }
        
        // [Fact]
        // public async Task QueryBuilderInvalidFilterFieldAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "InvalidField", Operator = "=", Value = "test"
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto], null);
        //     result.Should().BeEmpty(); // Should handle gracefully
        // }
        //
        // [Fact]
        // public async Task QueryBuilderInvalidOperatorAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "Name", Operator = "INVALID", Value = "test"
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto], null);
        //     result.Should().BeEmpty(); // Should handle gracefully
        // }
        //
        // [Fact]
        // public async Task QueryBuilderInvalidDateFormatAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "last_updated_at", Operator = ">", Value = "invalid-date"
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto], null);
        //     result.Should().BeEmpty(); // Should handle gracefully
        // }
        
        // [Fact]
        // public async Task QueryBuilderNullValueAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "Name", Operator = "=", Value = null
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto], null);
        //     result.Should().BeEmpty(); // Should handle gracefully
        // }
        
        // [Fact]
        // public async Task QueryBuilderEmptyValueAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "Name", Operator = "=", Value = ""
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto], null);
        //     result.Should().BeEmpty(); // Should handle gracefully
        // }
        
        // [Fact]
        // public async Task QueryBuilderExactDateMatchAsync()
        // {
        //     var dto = new CustomQueryRequestDto
        //     {
        //         Connector = "AND", Filter = "last_updated_at", Operator = "=", Value = "2024-01-20"
        //     };
        //     var result = _queryBusiness.QueryBuilder([dto]);
        //     result.Should().HaveCount(1); // Only Tech on exactly Jan 20
        //     result.First().Name.Should().Be("Tech");
        // }
        
        [Fact]
        public async Task QueryBuilderDateRangeExclusiveAsync()
        {
            var dto1 = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "last_updated_at", Operator = ">", Value = "2024-01-11"
            };
            var dto2 = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "last_updated_at", Operator = "<", Value = "2024-01-25"
            };
            var result = _queryBusiness.QueryBuilder([dto1, dto2], null);
            result.Should().HaveCount(1); // Only Tech (Jan 20 is between Jan 10 and Jan 25)
            result.First().Name.Should().Be("Tech");
        }
        
        [Fact]
        public async Task QueryBuilderContainsOperatorInDescriptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "Description", Operator = "LIKE", Value = "stop"
            };
            var result = _queryBusiness.QueryBuilder([dto]);
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Hunter");
        }
        
        [Fact]
        public async Task QueryBuilderFilterByOriginalIdPrefixAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "original_id", Operator = "LIKE", Value = "CT-99"
            };
            var result = _queryBusiness.QueryBuilder([dto], null);
            result.Should().HaveCount(4); // All Bad Batch members
        }
        
        [Fact]
        public async Task FullTextSearchMultipleTermsAsync()
        {
            var result = await _queryBusiness.Search("Captain Rex");
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Captain Rex");
        }
        
        [Fact]
        public async Task FullTextSearchNumericTermAsync()
        {
            var result = await _queryBusiness.Search("CT");
            result.Should().HaveCount(5);
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
            await Context.Tags.AddAsync(tag);
            await Context.SaveChangesAsync();

            var dataSource = new DataSource
            {
                Name = "R2D2",
                Description = "Weeeeeeeee!",
                ProjectId = project.Id,
                LastUpdatedAt = time2
            };
            await Context.DataSources.AddAsync(dataSource);
            await Context.SaveChangesAsync();

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
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(rex);
            await Context.SaveChangesAsync();

            var hunter = new Record
            {
                Name = "Hunter",
                Description = "Omega, stop doing that",
                OriginalId = "CT-9901",
                Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
                ProjectId = project.Id,
                DataSourceId = dataSource.Id,
                ClassId = testClass.Id,
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