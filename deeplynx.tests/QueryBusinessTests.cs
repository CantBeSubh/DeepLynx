using System.Text.Json;
using deeplynx.business;
using FluentAssertions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class QueryBusinessTests : IntegrationTestBase
    {
        private QueryBusiness _queryBusiness = null!;
        public long pid;
        public long did;
        public long cid;
        public DateTime timeGrab;
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
            result.First().Name.Should().Be("Captain Rex");
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
        public async Task FullTextSearchEmptyStringThrowsExceptionAsync()
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
        public async Task FullTextSearchWhitespaceOnlyThrowsExceptionAsync()
        {
            await Assert.ThrowsAsync<Exception>(() => 
                    _queryBusiness.Search("     "));
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
            result.Should().HaveCount(5);
        }
        
        [Fact]
        public async Task FullTextSearchNonExistentTermReturnsEmptyAsync()
        {
            var result = await _queryBusiness.Search("Jedi");
            result.Should().BeEmpty();
        }
        
        [Fact]
        public async Task QueryBuilderWithNullFiltersThrowsException()
        {
            Assert.Throws<ArgumentException>(() => 
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
        [Fact]
        public async Task QueryBuilderLessThanDateOperatorAsync()
        {
            var hunterRecord = await Context.Records
                .Where(r => r.Name == "Hunter")
                .FirstAsync();
    
            var baselineTime = hunterRecord.LastUpdatedAt;
            
            var dto = new CustomQueryRequestDto
            {
                Connector = null, 
                Filter = "last_updated_at", 
                Operator = "<", 
                Value = now.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss")
            };
    
            var result = _queryBusiness.QueryBuilder([dto]);
            result.Should().HaveCount(5); 
        }

        [Fact]
        public async Task QueryBuilderGreaterThanDateOperatorAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = null, 
                Filter = "last_updated_at", 
                Operator = ">", 
                Value = now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            var result = _queryBusiness.QueryBuilder([dto], null);
            result.Should().HaveCount(5); 
            result.All(r => r.LastUpdatedAt < now.AddMinutes(30)).Should().BeTrue();
        }

        [Fact]
        public async Task QueryBuilderDateRangeAsync()
        {
            var ahsoka = new Record
            {
                Name = "Ahsoka Tano",
                Description = "Favorite",
                OriginalId = "Snips",
                Properties = JsonSerializer.Serialize(new { Jedi = "Apprentice" }),
                ProjectId = pid,
                DataSourceId = did,
                ClassId = cid,
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(ahsoka);
            await Context.SaveChangesAsync();
        var rexRecord = await Context.Records
            .Where(r => r.Name == "Captain Rex")
            .FirstAsync();
    
        var baselineRex = rexRecord.LastUpdatedAt;
    
        var baselineAhsoka = ahsoka.LastUpdatedAt.AddMinutes(10);
        
        var dto1 = new CustomQueryRequestDto
        {
            Connector = null, 
            Filter = "last_updated_at", 
            Operator = ">", 
            Value = baselineRex.ToString("yyyy-MM-dd HH:mm:ss")
        };
        var dto2 = new CustomQueryRequestDto
        {
            Connector = "AND", 
            Filter = "last_updated_at", 
            Operator = "<", 
            Value = baselineAhsoka.ToString("yyyy-MM-dd HH:mm:ss")
        };
        
        var result = _queryBusiness.QueryBuilder([dto1, dto2], null);
        result.Should().HaveCount(6); 
        }
        
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
            result.Should().HaveCount(1); 
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
            result.Should().HaveCount(2); 
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
            result.Should().HaveCount(4); 
        }
        
        [Fact]
        public async Task QueryBuilderInvalidFilterFieldThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "InvalidField", Operator = "=", Value = "test"
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto]));
        }
        
        [Fact]
        public async Task QueryBuilderInvalidOperatorThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "Name", Operator = "INVALID", Value = "test"
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto]));
        }
        
        [Fact]
        public async Task QueryBuilderInvalidDateFormatThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "last_updated_at", Operator = ">", Value = "invalid-date"
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto]));
        }
        
        [Fact]
        public async Task QueryBuilderNullValueThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "Name", Operator = "=", Value = null
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto]));
        }
        
        [Fact]
        public async Task QueryBuilderEmptyValueThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "Name", Operator = "=", Value = ""
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto]));
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
            result.Should().HaveCount(4);
        }
        
        
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            
            var project = new Project
            {
                Name = "Anakin",
                Description = "You turned her against me"
            };
            await Context.Projects.AddAsync(project);
            await Context.SaveChangesAsync();
            pid = project.Id;

            var tag = new Tag
            {
                Name = "Padme",
                ProjectId = project.Id
            };
            await Context.Tags.AddAsync(tag);
            await Context.SaveChangesAsync();

            var dataSource = new DataSource
            {
                Name = "R2D2",
                Description = "Weeeeeeeee!",
                ProjectId = project.Id
            };
            await Context.DataSources.AddAsync(dataSource);
            await Context.SaveChangesAsync();
            did = dataSource.Id;
            
            var testClass = new Class
            {
                Name = "Darth Maul",
                Description = "My legs!",
                ProjectId = project.Id
            };
            await Context.Classes.AddAsync(testClass);
            await Context.SaveChangesAsync();
            cid = testClass.Id;

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
                Tags = new List<Tag> { tag },
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(hunter);
            await Context.SaveChangesAsync();

            var tech = new Record
            {
                Name = "Tech",
                Description = "RIP",
                OriginalId = "CT-9902",
                Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
                ProjectId = project.Id,
                DataSourceId = dataSource.Id,
                ClassId = testClass.Id,
                Tags = new List<Tag> { tag },
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(tech);
            await Context.SaveChangesAsync();

            var wrecker = new Record
            {
                Name = "Wrecker",
                Description = "Boom",
                OriginalId = "CT-9903",
                Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
                ProjectId = project.Id,
                DataSourceId = dataSource.Id,
                ClassId = testClass.Id,
                Tags = new List<Tag> { tag },
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(wrecker);
            await Context.SaveChangesAsync();

            var crosshair = new Record
            {
                Name = "Crosshair",
                Description = "Redemption Arch",
                OriginalId = "CT-9904",
                Properties = JsonSerializer.Serialize(new { CloneForce = "99" }),
                ProjectId = project.Id,
                DataSourceId = dataSource.Id,
                ClassId = testClass.Id,
                Tags = new List<Tag> { tag },
                Uri = "localhost:8090"
            };
            await Context.Records.AddAsync(crosshair);
            await Context.SaveChangesAsync();
            timeGrab = rex.LastUpdatedAt; 
        }
    }
}