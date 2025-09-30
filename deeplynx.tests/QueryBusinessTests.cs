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
        private CacheBusiness _cacheBusiness = null!;
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
            _queryBusiness = new QueryBusiness(Context, _cacheBusiness);
        }

       [Fact]
        public async Task FullTextSearchForSpecificCloneResultsIn1Async()
        {
            var result = await _queryBusiness.Search("rex", new[] { pid });
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Captain Rex");
        }

        [Fact]
        public async Task FullTextSearchForCloneForce99TagResultsIn4Async()
        {
            var result = await _queryBusiness.Search("99", new[] { pid });
            result.Should().HaveCount(4);
        }

        [Fact]
        public async Task FullTextSearchForCloneOriginalIdResultsIn1Async()
        {
            var result = await _queryBusiness.Search("CT-9901", new[] { pid });
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task QueryBuilderFindSpecificCloneWithCommonDataSourceAndOriginalIdResultsIn1Async()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "data_source_name", Operator = "LIKE", Value = "R2D2"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, "CT-7567");
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task FullTextSearchEmptyStringThrowsExceptionAsync()
        {
            await Assert.ThrowsAsync<Exception>(() => 
                _queryBusiness.Search("", new[] { pid }));
        }

        [Fact]
        public async Task FullTextSearchNullThrowsExceptionAsync()
        {
            await Assert.ThrowsAsync<Exception>(() => 
                _queryBusiness.Search(null, new[] { pid }));
        }

        [Fact]
        public async Task FullTextSearchWhitespaceOnlyThrowsExceptionAsync()
        {
            await Assert.ThrowsAsync<Exception>(() => 
                    _queryBusiness.Search("     ", new[] { pid }));
        }

        [Fact]
        public async Task FullTextSearchPartialMatchInDescriptionAsync()
        {
            var result = await _queryBusiness.Search("Omega", new[] { pid });
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Hunter");
        }

        [Fact]
        public async Task FullTextSearchInPropertiesJsonAsync()
        {
            var result = await _queryBusiness.Search("501st", new[] { pid });
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Captain Rex");
        }

        [Fact]
        public async Task FullTextSearchSpecialCharactersAsync()
        {
            var result = await _queryBusiness.Search("CT-", new[] { pid });
            result.Should().HaveCount(5);
        }

        [Fact]
        public async Task FullTextSearchForJediAcrossAllProjectsAsync()
        {
            var allProjectIds = await Context.Projects.Select(p => p.Id).ToArrayAsync();
            var result = await _queryBusiness.Search("Jedi", allProjectIds);
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Luke Skywalker");
        }

        [Fact]
        public async Task FullTextSearchForBeskarReturnsMandalorianAsync()
        {
            var mandoProjectId = await Context.Projects
                .Where(p => p.Name == "Mandalorians")
                .Select(p => p.Id)
                .FirstAsync();
            var result = await _queryBusiness.Search("Beskar", new[] { mandoProjectId });
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Din Djarin");
        }

        [Fact]
        public async Task FullTextSearchForSithReturnsVaderAsync()
        {
            var empireProjectId = await Context.Projects
                .Where(p => p.Name == "The Galactic Empire")
                .Select(p => p.Id)
                .FirstAsync();
            var result = await _queryBusiness.Search("Sith", new[] { empireProjectId });
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Darth Vader");
        }

        [Fact]
        public async Task QueryBuilderWithNullFiltersThrowsException()
        {
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder(null, new[] { pid }, null));
        }

        [Fact]
        public async Task QueryBuilderEqualityOperatorAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "name", Operator = "=", Value = "Tech"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, null);
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Tech");
        }

        [Fact]
        public async Task QueryBuilderLikeOperatorCaseInsensitiveAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "name", Operator = "LIKE", Value = "tech"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, null);
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Tech");
        }

        [Fact]
        public async Task QueryBuilderGreaterThanDateOperatorAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = null, 
                Filter = "last_updated_at", 
                Operator = ">", 
                Value = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, null);
            result.Should().HaveCount(5); 
            result.All(r => r.LastUpdatedAt > DateTime.Now.AddMinutes(-30)).Should().BeTrue();
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
            
            var result = _queryBusiness.QueryBuilder([dto1, dto2], new[] { pid }, null);
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
                Connector = "AND", Filter = "original_id", Operator = "LIKE", Value = "CT-7567"
            };
            var result = _queryBusiness.QueryBuilder([dto1, dto2], new[] { pid }, null);
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
            var result = _queryBusiness.QueryBuilder([dto1, dto2], new[] { pid });
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
                Connector = "OR", Filter = "name", Operator = "=", Value = "Tech"
            };
            var dto3 = new CustomQueryRequestDto
            {
                Connector = "OR", Filter = "name", Operator = "=", Value = "Hunter"
            };
            var result = _queryBusiness.QueryBuilder([dto1, dto2, dto3], new[] { pid }, null);
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
                Connector = "AND", Filter = "name", Operator = "=", Value = "Tech"
            };
            var dto3 = new CustomQueryRequestDto
            {
                Connector = "OR", Filter = "name", Operator = "=", Value = "Hunter"
            };
            var result = _queryBusiness.QueryBuilder([dto1, dto2, dto3], new[] { pid }, null);
            result.Should().HaveCount(2); 
        }

        [Fact]
        public async Task QueryBuilderWithSearchTermCombinedAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "data_source_name", Operator = "LIKE", Value = "R2D2"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, "Captain");
            result.Should().HaveCount(1); 
        }

        [Fact]
        public async Task QueryBuilderInvalidFilterFieldThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "InvalidField", Operator = "=", Value = "test"
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto], new[] { pid }));
        }

        [Fact]
        public async Task QueryBuilderInvalidOperatorThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "name", Operator = "INVALID", Value = "test"
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto], new[] { pid }));
        }

        [Fact]
        public async Task QueryBuilderInvalidDateFormatThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "last_updated_at", Operator = ">", Value = "invalid-date"
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto], new[] { pid }));
        }

        [Fact]
        public async Task QueryBuilderNullValueThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "name", Operator = "=", Value = null
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto], new[] { pid }));
        }

        [Fact]
        public async Task QueryBuilderEmptyValueThrowsExceptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "name", Operator = "=", Value = ""
            };
            Assert.Throws<ArgumentException>(() => 
                _queryBusiness.QueryBuilder([dto], new[] { pid }));
        }

        [Fact]
        public async Task QueryBuilderContainsOperatorInDescriptionAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = "AND", Filter = "description", Operator = "LIKE", Value = "stop"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid });
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
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, null);
            result.Should().HaveCount(4);
        }
        
        [Fact]
        public async Task QueryBuilderFilterByMultipleProjectIdsAsync()
        {
            var anakinId = await Context.Projects.Where(p => p.Name == "Anakin").Select(p => p.Id).FirstAsync();
            var rebellionId = await Context.Projects.Where(p => p.Name == "The Rebellion").Select(p => p.Id).FirstAsync();
            
            var dto = new CustomQueryRequestDto
            {
                Connector = null, Filter = "name", Operator = "LIKE", Value = "a"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { anakinId, rebellionId }, null);
            result.Should().HaveCount(6); 
        }

        [Fact]
        public async Task QueryBuilderFilterByProjectNameRebellionAsync()
        {
            var rebellionId =
                await Context.Projects.Where(p => p.Name == "The Rebellion").Select(p => p.Id).FirstAsync();

            var dto = new CustomQueryRequestDto
            {
                Connector = null, Filter = "project_name", Operator = "LIKE", Value = "Rebellion"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { rebellionId }, null);
            result.Should().HaveCount(4);
        }


        [Fact]
        public async Task QueryBuilderFilterByProjectNameEmpireAsync()
        {
            var empireId = await Context.Projects.Where(p => p.Name == "The Galactic Empire").Select(p => p.Id).FirstAsync();
            
            var dto = new CustomQueryRequestDto
            {
                Connector = null, Filter = "project_name", Operator = "=", Value = "The Galactic Empire"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { empireId }, null);
            result.Should().HaveCount(3); 
        }

        [Fact]
        public async Task QueryBuilderFilterByProjectNameMandoAsync()
        {
            var mandoId = await Context.Projects.Where(p => p.Name == "Mandalorians").Select(p => p.Id).FirstAsync();
            
            var dto = new CustomQueryRequestDto
            {
                Connector = null, Filter = "project_name", Operator = "LIKE", Value = "Mandalorians"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { mandoId }, null);
            result.Should().HaveCount(4); 
        }

        [Fact]
        public async Task QueryBuilderUserAccessToSpecificProjectsOnlyAsync()
        {
            var anakinId = await Context.Projects.Where(p => p.Name == "Anakin").Select(p => p.Id).FirstAsync();
            var empireId = await Context.Projects.Where(p => p.Name == "The Galactic Empire").Select(p => p.Id).FirstAsync();
            
            var result = _queryBusiness.QueryBuilder([], new[] { anakinId, empireId }, null);
            result.Should().HaveCount(8); 
        }

        [Fact]
        public async Task FullTextSearchRestrictedToUserProjectsAsync()
        {
            var rebellionId = await Context.Projects.Where(p => p.Name == "The Rebellion").Select(p => p.Id).FirstAsync();
            
            var result = await _queryBusiness.Search("the", new[] { rebellionId });
            result.All(r => r.ProjectId == rebellionId).Should().BeTrue();
        }

        [Fact]
        public async Task QueryBuilderRecordsInAnakinProjectWithCrossProjectResourcesAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = null, Filter = "project_name", Operator = "=", Value = "Anakin"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, null);
            result.Should().HaveCount(5);
        }

        [Fact]
        public async Task QueryBuilderFilterByDataSourceAcrossAllowedProjectsAsync()
        {
            var allProjectIds = await Context.Projects.Select(p => p.Id).ToArrayAsync();
            
            var dto = new CustomQueryRequestDto
            {
                Connector = null, Filter = "data_source_name", Operator = "LIKE", Value = "Yavin"
            };
            var result = _queryBusiness.QueryBuilder([dto], allProjectIds, null);
            result.Should().HaveCount(4); 
        }

        [Fact]
        public async Task QueryBuilderNoAccessToProjectReturnsEmptyAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = null, Filter = "name", Operator = "LIKE", Value = "a"
            };
            var result = _queryBusiness.QueryBuilder([dto], Array.Empty<long>(), null);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task FullTextSearchAcrossAllProjectsUserHasAccessToAsync()
        {
            var allProjectIds = await Context.Projects.Select(p => p.Id).ToArrayAsync();
            var result = await _queryBusiness.Search("Captain", allProjectIds);
            result.Should().HaveCount(2); 
        }

        [Fact]
        public async Task QueryBuilderFilterByOriginalIdPrefixREBWithProjectAccessAsync()
        {
            var rebellionId = await Context.Projects.Where(p => p.Name == "The Rebellion").Select(p => p.Id).FirstAsync();
            
            var dto = new CustomQueryRequestDto
            {
                Connector = null, Filter = "original_id", Operator = "LIKE", Value = "REB-"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { rebellionId }, null);
            result.Should().HaveCount(4); 
        }

        [Fact]
        public async Task QueryBuilderComplexQueryWithLimitedProjectAccessAsync()
        {
            var anakinId = await Context.Projects.Where(p => p.Name == "Anakin").Select(p => p.Id).FirstAsync();
            var mandoId = await Context.Projects.Where(p => p.Name == "Mandalorians").Select(p => p.Id).FirstAsync();
            
            var dto = new CustomQueryRequestDto
            {
                Connector = null, Filter = "original_id", Operator = "LIKE", Value = "CT-"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { anakinId, mandoId }, null);
            result.Should().HaveCount(5); 
        }

        [Fact]
        public async Task FullTextSearchWithProjectFilterFindsRecordsUsingCrossProjectResourcesAsync()
        {
            var result = await _queryBusiness.Search("Death Star", new[] { pid });
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Tech");
            result.First().ProjectId.Should().Be(pid);
        }

        [Fact]
        public async Task QueryBuilderMultipleProjectsWithOrConditionAsync()
        {
            var allProjectIds = await Context.Projects.Select(p => p.Id).ToArrayAsync();
            
            var dto1 = new CustomQueryRequestDto
            {
                Connector = null, Filter = "project_name", Operator = "=", Value = "Anakin"
            };
            var dto2 = new CustomQueryRequestDto
            {
                Connector = "OR", Filter = "project_name", Operator = "=", Value = "The Galactic Empire"
            };
            var result = _queryBusiness.QueryBuilder([dto1, dto2], allProjectIds, null);
            result.Should().HaveCount(8); 
        }
        
        [Fact]
        public async Task FullTextSearchPartialTagNameAsync()
        {
            var result = await _queryBusiness.Search("Padme", new[] { pid });
            result.Should().HaveCount(4); 
        }

        [Fact]
        public async Task FullTextSearchPartialTagNameCaseInsensitiveAsync()
        {
            var result = await _queryBusiness.Search("padme", new[] { pid });
            result.Should().HaveCount(4);
        }

        [Fact]
        public async Task FullTextSearchInTagsAcrossProjectsAsync()
        {
            var allProjectIds = await Context.Projects.Select(p => p.Id).ToArrayAsync();
            var result = await _queryBusiness.Search("Bounty", allProjectIds);
            result.Should().HaveCount(2); 
        }
        [Fact]
        public async Task QueryBuilderKeyValueSearchForLegion501stAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = null,
                Filter = "properties",
                Operator = "KEY_VALUE",
                Json = JsonSerializer.Serialize(new { Legion = "501st" })
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, null);
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Captain Rex");
        }

        [Fact]
        public async Task QueryBuilderKeyValueSearchForCloneForce99Async()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = null,
                Filter = "properties",
                Operator = "KEY_VALUE",
                Json = JsonSerializer.Serialize(new { CloneForce = "99" })
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, null);
            result.Should().HaveCount(4); 
        }
        
        [Fact]
        public async Task QueryBuilderLikeOperatorOnPropertiesJsonbAsync()
        {
            var dto = new CustomQueryRequestDto
            {
                Connector = null,
                Filter = "properties",
                Operator = "LIKE",
                Value = "501"
            };
            var result = _queryBusiness.QueryBuilder([dto], new[] { pid }, null);
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Captain Rex");
        }
        
        
        protected override async Task SeedTestDataAsync()
{
        await base.SeedTestDataAsync();
        
        // Project 1: Anakin
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

        // Project 2: The Rebellion
        var rebellionProject = new Project
        {
            Name = "The Rebellion",
            Description = "Hope is like the sun"
        };
        await Context.Projects.AddAsync(rebellionProject);
        await Context.SaveChangesAsync();

        var rebelTag = new Tag
        {
            Name = "Alliance",
            ProjectId = rebellionProject.Id
        };
        await Context.Tags.AddAsync(rebelTag);
        await Context.SaveChangesAsync();

        var rebelDataSource = new DataSource
        {
            Name = "Yavin IV Base",
            Description = "May the Force be with you",
            ProjectId = rebellionProject.Id
        };
        await Context.DataSources.AddAsync(rebelDataSource);
        await Context.SaveChangesAsync();

        var rebelClass = new Class
        {
            Name = "Rebel Leaders",
            Description = "Leaders of the Rebellion",
            ProjectId = rebellionProject.Id
        };
        await Context.Classes.AddAsync(rebelClass);
        await Context.SaveChangesAsync();

        // Project 3: The Empire
        var empireProject = new Project
        {
            Name = "The Galactic Empire",
            Description = "Peace through power"
        };
        await Context.Projects.AddAsync(empireProject);
        await Context.SaveChangesAsync();

        var imperialTag = new Tag
        {
            Name = "Imperial Officer",
            ProjectId = empireProject.Id
        };
        await Context.Tags.AddAsync(imperialTag);
        await Context.SaveChangesAsync();

        var empireDataSource = new DataSource
        {
            Name = "Death Star",
            Description = "That's no moon",
            ProjectId = empireProject.Id
        };
        await Context.DataSources.AddAsync(empireDataSource);
        await Context.SaveChangesAsync();

        var empireClass = new Class
        {
            Name = "Imperial Command",
            Description = "High-ranking Imperial officers",
            ProjectId = empireProject.Id
        };
        await Context.Classes.AddAsync(empireClass);
        await Context.SaveChangesAsync();

        // Project 4: Mandalorians
        var mandoProject = new Project
        {
            Name = "Mandalorians",
            Description = "This is the Way"
        };
        await Context.Projects.AddAsync(mandoProject);
        await Context.SaveChangesAsync();

        var mandoTag = new Tag
        {
            Name = "Bounty Hunter",
            ProjectId = mandoProject.Id
        };
        var clanTag = new Tag
        {
            Name = "Clan Leader",
            ProjectId = mandoProject.Id
        };
        await Context.Tags.AddAsync(mandoTag);
        await Context.Tags.AddAsync(clanTag);
        await Context.SaveChangesAsync();

        var mandoDataSource = new DataSource
        {
            Name = "Nevarro",
            Description = "Covert hideout",
            ProjectId = mandoProject.Id
        };
        await Context.DataSources.AddAsync(mandoDataSource);
        await Context.SaveChangesAsync();

        var mandoClass = new Class
        {
            Name = "Warriors",
            Description = "Mandalorian warriors and bounty hunters",
            ProjectId = mandoProject.Id
        };
        await Context.Classes.AddAsync(mandoClass);
        await Context.SaveChangesAsync();

        // MIXED RECORDS - Project 1 (Anakin) records using various datasources and classes
        var rex = new Record
        {
            Name = "Captain Rex",
            Description = "Clankers!",
            OriginalId = "CT-7567",
            Properties = JsonSerializer.Serialize(new { Legion = "501st" }),
            ProjectId = project.Id,
            DataSourceId = dataSource.Id, // R2D2 datasource
            ClassId = testClass.Id, // Darth Maul class
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
            DataSourceId = rebelDataSource.Id, // Using Rebellion datasource!
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
            DataSourceId = empireDataSource.Id, // Using Empire datasource!
            ClassId = rebelClass.Id, // Using Rebel class!
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
            DataSourceId = mandoDataSource.Id, // Using Mando datasource!
            ClassId = mandoClass.Id, // Using Mando class!
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
            ClassId = empireClass.Id, // Using Empire class!
            Tags = new List<Tag> { tag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(crosshair);
        await Context.SaveChangesAsync();
        timeGrab = rex.LastUpdatedAt;

        // MIXED RECORDS - Project 2 (Rebellion) with cross-project references
        var leia = new Record
        {
            Name = "Princess Leia",
            Description = "Rebel leader and princess",
            OriginalId = "REB-001",
            Properties = JsonSerializer.Serialize(new { Homeworld = "Alderaan", Rank = "General" }),
            ProjectId = rebellionProject.Id,
            DataSourceId = rebelDataSource.Id,
            ClassId = rebelClass.Id,
            Tags = new List<Tag> { rebelTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(leia);
        await Context.SaveChangesAsync();

        var luke = new Record
        {
            Name = "Luke Skywalker",
            Description = "Last of the Jedi",
            OriginalId = "REB-002",
            Properties = JsonSerializer.Serialize(new { Homeworld = "Tatooine", Rank = "Commander" }),
            ProjectId = rebellionProject.Id,
            DataSourceId = dataSource.Id, // Using Anakin's R2D2 datasource!
            ClassId = rebelClass.Id,
            Tags = new List<Tag> { rebelTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(luke);
        await Context.SaveChangesAsync();

        var han = new Record
        {
            Name = "Han Solo",
            Description = "Smuggler turned hero",
            OriginalId = "REB-003",
            Properties = JsonSerializer.Serialize(new { Ship = "Millennium Falcon", Rank = "Captain" }),
            ProjectId = rebellionProject.Id,
            DataSourceId = mandoDataSource.Id, // Using Mando datasource!
            ClassId = mandoClass.Id, // Using Mando class!
            Tags = new List<Tag> { rebelTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(han);
        await Context.SaveChangesAsync();

        var wedge = new Record
        {
            Name = "Wedge Antilles",
            Description = "Best pilot in the galaxy",
            OriginalId = "REB-004",
            Properties = JsonSerializer.Serialize(new { Squadron = "Rogue Squadron", Rank = "Wing Commander" }),
            ProjectId = rebellionProject.Id,
            DataSourceId = empireDataSource.Id, // Using Empire datasource!
            ClassId = testClass.Id, // Using Anakin's Darth Maul class!
            Tags = new List<Tag> { rebelTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(wedge);
        await Context.SaveChangesAsync();

        // MIXED RECORDS - Project 3 (Empire) with cross-project references
        var vader = new Record
        {
            Name = "Darth Vader",
            Description = "I find your lack of faith disturbing",
            OriginalId = "IMP-001",
            Properties = JsonSerializer.Serialize(new { Title = "Dark Lord of the Sith", Rank = "Supreme Commander" }),
            ProjectId = empireProject.Id,
            DataSourceId = empireDataSource.Id,
            ClassId = empireClass.Id,
            Tags = new List<Tag> { imperialTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(vader);
        await Context.SaveChangesAsync();

        var tarkin = new Record
        {
            Name = "Grand Moff Tarkin",
            Description = "You may fire when ready",
            OriginalId = "IMP-002",
            Properties = JsonSerializer.Serialize(new { Title = "Grand Moff", Station = "Death Star" }),
            ProjectId = empireProject.Id,
            DataSourceId = rebelDataSource.Id, // Using Rebellion datasource!
            ClassId = rebelClass.Id, // Using Rebel class! (Infiltration?)
            Tags = new List<Tag> { imperialTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(tarkin);
        await Context.SaveChangesAsync();

        var thrawn = new Record
        {
            Name = "Grand Admiral Thrawn",
            Description = "Tactical genius",
            OriginalId = "IMP-003",
            Properties = JsonSerializer.Serialize(new { Species = "Chiss", Rank = "Grand Admiral" }),
            ProjectId = empireProject.Id,
            DataSourceId = dataSource.Id, // Using Anakin's datasource!
            ClassId = mandoClass.Id, // Using Mando class!
            Tags = new List<Tag> { imperialTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(thrawn);
        await Context.SaveChangesAsync();

        // MIXED RECORDS - Project 4 (Mandalorians) with cross-project references
        var dinDjarin = new Record
        {
            Name = "Din Djarin",
            Description = "The Mandalorian",
            OriginalId = "MANDO-001",
            Properties = JsonSerializer.Serialize(new { Armor = "Beskar", Title = "Mand'alor" }),
            ProjectId = mandoProject.Id,
            DataSourceId = mandoDataSource.Id,
            ClassId = mandoClass.Id,
            Tags = new List<Tag> { mandoTag, clanTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(dinDjarin);
        await Context.SaveChangesAsync();

        var boKatan = new Record
        {
            Name = "Bo-Katan Kryze",
            Description = "Rightful ruler of Mandalore",
            OriginalId = "MANDO-002",
            Properties = JsonSerializer.Serialize(new { Clan = "Kryze", Title = "Leader of Mandalore" }),
            ProjectId = mandoProject.Id,
            DataSourceId = rebelDataSource.Id, // Using Rebellion datasource!
            ClassId = rebelClass.Id, // Using Rebel class!
            Tags = new List<Tag> { clanTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(boKatan);
        await Context.SaveChangesAsync();

        var bobafett = new Record
        {
            Name = "Boba Fett",
            Description = "Like my father before me",
            OriginalId = "MANDO-003",
            Properties = JsonSerializer.Serialize(new { Ship = "Slave I", Occupation = "Daimyo" }),
            ProjectId = mandoProject.Id,
            DataSourceId = empireDataSource.Id, // Using Empire datasource!
            ClassId = empireClass.Id, // Using Empire class!
            Tags = new List<Tag> { mandoTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(bobafett);
        await Context.SaveChangesAsync();

        var pazVizsla = new Record
        {
            Name = "Paz Vizsla",
            Description = "Heavy infantry",
            OriginalId = "MANDO-004",
            Properties = JsonSerializer.Serialize(new { Clan = "Vizsla", Weapon = "Heavy Blaster" }),
            ProjectId = mandoProject.Id,
            DataSourceId = dataSource.Id, // Using Anakin's datasource!
            ClassId = testClass.Id, // Using Anakin's class!
            Tags = new List<Tag> { clanTag },
            Uri = "localhost:8090"
        };
        await Context.Records.AddAsync(pazVizsla);
        await Context.SaveChangesAsync();
    }
    }
}