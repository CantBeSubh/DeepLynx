using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class SensitivityLabelBusinessTests : IntegrationTestBase
    {
        private Config _config;
        private EventBusiness _eventBusiness;
        private INotificationBusiness _notificationBusiness = null!;
        private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
        private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
        private SensitivityLabelBusiness _labelBusiness;
        
        public long oid;    // organization ID
        public long pid;    // project ID
        public long lid;    // label ID
        public long lid2;   // archived label ID
        public long uid;
        public SensitivityLabelBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _config = new Config();
            _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
            _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
            _notificationBusiness = new NotificationBusiness(_config, Context, _mockNotificationLogger.Object, _mockHubContext.Object);
            _eventBusiness = new EventBusiness(_config, Context, _cacheBusiness, _notificationBusiness);
            _labelBusiness = new SensitivityLabelBusiness(Context, _cacheBusiness, _eventBusiness);
        }
        
        #region GetAllSensitivityLabels Tests
        
        [Fact]
        public async Task GetAllSensitivityLabels_ExcludesArchived()
        {
            // Act
            var result = await _labelBusiness.GetAllSensitivityLabels(null, null, true);
            var labels = result.ToList();
            
            // Assert
            Assert.All(labels, l => Assert.False(l.IsArchived));
            Assert.Contains(labels, l => l.Id == lid);
            Assert.DoesNotContain(labels, l => l.Id == lid2); // archived label
        }
        
        [Fact]
        public async Task GetAllSensitivityLabels_WithHideArchivedFalse_IncludesArchived()
        {
            // Act
            var result = await _labelBusiness.GetAllSensitivityLabels(null, null, false);
            var labels = result.ToList();
            
            // Assert
            Assert.Contains(labels, l => l.IsArchived);
            Assert.Contains(labels, l => l.Id == lid);
            Assert.Contains(labels, l => l.Id == lid2); // archived label
        }
        
        [Fact]
        public async Task GetAllSensitivityLabels_FiltersOnProjectId()
        {
            // Act
            var result = await _labelBusiness.GetAllSensitivityLabels(pid, null, true);
            var labels = result.ToList();
            
            // Assert
            Assert.All(labels, l => Assert.Equal(pid, l.ProjectId));
            Assert.Contains(labels, l => l.Id == lid);
        }
        
        [Fact]
        public async Task GetAllSensitivityLabels_FiltersOnOrganizationId()
        {
            // Create org-level label for this test
            var orgLabel = new SensitivityLabel
            {
                Name = "Org Label",
                Description = "Organization level label",
                OrganizationId = oid,
                IsArchived = false
            };
            Context.SensitivityLabels.Add(orgLabel);
            await Context.SaveChangesAsync();
            
            // Act
            var result = await _labelBusiness.GetAllSensitivityLabels(null, oid, true);
            var labels = result.ToList();
            
            // Assert
            Assert.All(labels, l => Assert.Equal(oid, l.OrganizationId));
            Assert.Contains(labels, l => l.Id == orgLabel.Id);
        }
        
        #endregion
        
        #region GetSensitivityLabel Tests

        [Fact]
        public async Task GetSensitivityLabel_Succeeds_WhenExists()
        {
            // Act
            var result = await _labelBusiness.GetSensitivityLabel(lid);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(lid, result.Id);
            Assert.Equal("Test Label", result.Name);
            Assert.Equal("Test label for unit tests", result.Description);
            Assert.False(result.IsArchived);
        }
        
        [Fact]
        public async Task GetSensitivityLabel_Fails_IfNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.GetSensitivityLabel(99999));
            
            Assert.Contains("Sensitivity label with id 99999 not found", exception.Message);
        }
        
        [Fact]
        public async Task GetSensitivityLabel_Fails_IfArchivedLabel()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.GetSensitivityLabel(lid2)); // archived label
            
            Assert.Contains($"Sensitivity label with id {lid2} is archived", exception.Message);
        }
        
        #endregion
        
        #region CreateSensitivityLabel Tests
        
        [Fact]
        public async Task CreateSensitivityLabel_Success_ReturnsLabel()
        {
            // Arrange
            var dto = new CreateSensitivityLabelRequestDto
            {
                Name = "New Test Label",
                Description = "New test label description"
            };
            
            // Act
            var result = await _labelBusiness.CreateSensitivityLabel(dto, pid, null);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Description, result.Description);
            Assert.Equal(pid, result.ProjectId);
            Assert.False(result.IsArchived);
            
            // verify label was actually created in database
            var createdLabel = await Context.SensitivityLabels.FindAsync(result.Id);
            Assert.NotNull(createdLabel);
            Assert.Equal(dto.Name, createdLabel.Name);
        }
        
        [Fact]
        public async Task CreateSensitivityLabel_Success_WithOrganization()
        {
            // Arrange
            var dto = new CreateSensitivityLabelRequestDto
            {
                Name = "New Org Label",
                Description = "New organization label description"
            };
            
            // Act
            var result = await _labelBusiness.CreateSensitivityLabel(dto, null, oid);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Description, result.Description);
            Assert.Equal(oid, result.OrganizationId);
            Assert.False(result.IsArchived);
            
            // verify label was actually created in database
            var createdLabel = await Context.SensitivityLabels.FindAsync(result.Id);
            Assert.NotNull(createdLabel);
            Assert.Equal(dto.Name, createdLabel.Name);
        }

        [Fact]
        public async Task CreateSensitivityLabel_Success_CreatesEvent()
        {
            // Arrange
            var dto = new CreateSensitivityLabelRequestDto
            {
                Name = "Event Test Label",
                Description = "A test label for event logging"
            };
            
            // Act
            var result = await _labelBusiness.CreateSensitivityLabel(dto, pid, null);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Event Test Label", result.Name);
            
            // Ensure that the SensitivityLabel create event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "sensitivity_label",
                EntityId = result.Id,
            });
        }
        
        [Fact]
        public async Task CreateSensitivityLabel_Fails_IfNoName()
        {
            // Arrange
            var dto = new CreateSensitivityLabelRequestDto
            {
                Description = "Label without name"
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _labelBusiness.CreateSensitivityLabel(dto, pid, null));
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task CreateSensitivityLabel_Fails_IfEmptyName()
        {
            // Arrange
            var dto = new CreateSensitivityLabelRequestDto
            {
                Name = "",
                Description = "Label with empty name"
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _labelBusiness.CreateSensitivityLabel(dto, pid, null));
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task CreateSensitivityLabel_Fails_IfBothProjectAndOrgAreSet()
        {
            // Arrange
            var dto = new CreateSensitivityLabelRequestDto
            {
                Name = "Dual Label",
                Description = "Label with both project and org"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _labelBusiness.CreateSensitivityLabel(dto, pid, oid));
            Assert.Contains("Please provide only one of Project ID or Organization ID, not both", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task CreateSensitivityLabel_Fails_IfNeitherProjectNorOrgAreSet()
        {
            // Arrange
            var dto = new CreateSensitivityLabelRequestDto
            {
                Name = "Orphaned Label",
                Description = "Label without project or org"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _labelBusiness.CreateSensitivityLabel(dto, null, null));
            Assert.Contains("One of Project ID or Organization ID must be provided", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region UpdateSensitivityLabel Tests
        
        [Fact]
        public async Task UpdateSensitivityLabel_Success_ReturnsLabel()
        {
            // Arrange
            var dto = new UpdateSensitivityLabelRequestDto
            {
                Name = "Updated Label",
                Description = "Updated description"
            };
            
            // Act
            var result = await _labelBusiness.UpdateSensitivityLabel(lid, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(lid, result.Id);
            Assert.Equal("Updated Label", result.Name);
            Assert.Equal("Updated description", result.Description);
            
            // Verify it was actually saved to DB
            var savedLabel = await Context.SensitivityLabels.FindAsync(lid);
            Assert.NotNull(savedLabel);
            Assert.Equal("Updated Label", savedLabel.Name);
            Assert.Equal("Updated description", savedLabel.Description);
        }
        
        [Fact]
        public async Task UpdateSensitivityLabel_Success_CreatesEvent()
        {
            // Arrange
            var dto = new UpdateSensitivityLabelRequestDto
            {
                Name = "Event Updated Label"
            };
            
            // Act
            var result = await _labelBusiness.UpdateSensitivityLabel(lid, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Event Updated Label", result.Name);
            
            // Ensure that the SensitivityLabel update event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "update",
                EntityType = "sensitivity_label",
                EntityId = result.Id,
            });
        }
        
        [Fact]
        public async Task UpdateSensitivityLabel_Fails_IfNotFound()
        {
            // Arrange
            var dto = new UpdateSensitivityLabelRequestDto
            {
                Name = "Updated Label"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.UpdateSensitivityLabel(99999, dto));
            
            Assert.Contains("Sensitivity label with id 99999 not found", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task UpdateSensitivityLabel_Fails_IfArchived()
        {
            // Arrange
            var dto = new UpdateSensitivityLabelRequestDto
            {
                Name = "Updated Archived Label"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.UpdateSensitivityLabel(lid2, dto)); // archived label
            
            Assert.Contains($"Sensitivity label with id {lid2} not found", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region ArchiveSensitivityLabel Tests
        
        [Fact]
        public async Task ArchiveSensitivityLabel_Succeeds_IfNotArchived()
        {
            // Act
            var result = await _labelBusiness.ArchiveSensitivityLabel(lid);
            
            // Assert
            Assert.True(result);
            
            // Verify it was actually saved to DB
            var savedLabel = await Context.SensitivityLabels.FindAsync(lid);
            Assert.NotNull(savedLabel);
            Assert.True(savedLabel.IsArchived);
            
            // Ensure that the SensitivityLabel archive event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "archive",
                EntityType = "sensitivity_label",
                EntityId = lid,
            });
        }
        
        [Fact]
        public async Task ArchiveSensitivityLabel_Fails_IfArchived()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.ArchiveSensitivityLabel(lid2)); // already archived
            
            Assert.Contains($"Sensitivity label with id {lid2} not found or is archived", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task ArchiveSensitivityLabel_Fails_IfNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.ArchiveSensitivityLabel(99999));
            
            Assert.Contains("Sensitivity label with id 99999 not found or is archived", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region UnarchiveSensitivityLabel Tests
        
        [Fact]
        public async Task UnarchiveSensitivityLabel_Succeeds_IfArchived()
        {
            // Act
            var result = await _labelBusiness.UnarchiveSensitivityLabel(lid2);
            
            // Assert
            Assert.True(result);
            
            // Verify it was actually saved to DB
            var savedLabel = await Context.SensitivityLabels.FindAsync(lid2);
            Assert.NotNull(savedLabel);
            Assert.False(savedLabel.IsArchived);
            
            // Ensure that the SensitivityLabel unarchive event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "unarchive",
                EntityType = "sensitivity_label",
                EntityId = lid2,
            });
        }
        
        [Fact]
        public async Task UnarchiveSensitivityLabel_Fails_IfNotArchived()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.UnarchiveSensitivityLabel(lid)); // not archived
            
            Assert.Contains($"Sensitivity label with id {lid} not found or is not archived", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task UnarchiveSensitivityLabel_Fails_IfNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.UnarchiveSensitivityLabel(99999));
            
            Assert.Contains("Sensitivity label with id 99999 not found or is not archived", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
        #region DeleteSensitivityLabel Tests
        
        [Fact]
        public async Task DeleteSensitivityLabel_Succeeds_WhenExists()
        {
            // Act
            var result = await _labelBusiness.DeleteSensitivityLabel(lid);
            
            // Assert
            Assert.True(result);
            
            // Verify it was actually deleted from DB
            var deletedLabel = await Context.SensitivityLabels.FindAsync(lid);
            Assert.Null(deletedLabel);
            
            // Ensure that the SensitivityLabel delete event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "delete",
                EntityType = "sensitivity_label",
                EntityId = lid,
            });
        }
        
        [Fact]
        public async Task DeleteSensitivityLabel_Fails_IfNotFound()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.DeleteSensitivityLabel(99999));
            
            Assert.Contains("Sensitivity label with id 99999 not found or is archived", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task DeleteSensitivityLabel_Fails_IfArchived()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _labelBusiness.DeleteSensitivityLabel(lid2)); // archived label
            
            Assert.Contains($"Sensitivity label with id {lid2} not found or is archived", exception.Message);
            
            // Ensure that no event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }
        
        #endregion
        
    #region LastUpdatedBy Tests

        [Fact]
        public async Task CreateSensitivityLabel_Success_StoresLastUpdatedByUserId()
        {
            // Arrange
            var testLabel = new SensitivityLabel
            {
                Name = "Test Label LastUpdatedBy",
                Description = "Test description",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid
            };
            
            // Act
            Context.SensitivityLabels.Add(testLabel);
            await Context.SaveChangesAsync();

            // Assert
            var savedLabel = await Context.SensitivityLabels.FindAsync(testLabel.Id);
            Assert.NotNull(savedLabel);
            Assert.Equal(uid, savedLabel.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateSensitivityLabel_Success_NavigationPropertyLoadsUser()
        {
            // Arrange
            var testLabel = new SensitivityLabel
            {
                Name = "Test Label Navigation",
                Description = "Test description 2",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid
            };
            
            Context.SensitivityLabels.Add(testLabel);
            await Context.SaveChangesAsync();

            // Act
            var labelWithUser = await Context.SensitivityLabels
                .Include(l => l.LastUpdatedByUser)
                .FirstAsync(l => l.Id == testLabel.Id);
            
            // Assert
            Assert.NotNull(labelWithUser.LastUpdatedByUser);
            Assert.Equal("Test User", labelWithUser.LastUpdatedByUser.Name);
            Assert.Equal("test_label@example.com", labelWithUser.LastUpdatedByUser.Email);
            Assert.Equal(uid, labelWithUser.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateSensitivityLabel_Success_WithNullLastUpdatedBy()
        {
            // Arrange
            var testLabel = new SensitivityLabel
            {
                Name = "Test Label Null",
                Description = "Test description 3",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            // Act
            Context.SensitivityLabels.Add(testLabel);
            await Context.SaveChangesAsync();

            // Assert
            var savedLabel = await Context.SensitivityLabels.FindAsync(testLabel.Id);
            Assert.NotNull(savedLabel);
            Assert.Null(savedLabel.LastUpdatedBy);
            
            var labelWithUser = await Context.SensitivityLabels
                .Include(l => l.LastUpdatedByUser)
                .FirstAsync(l => l.Id == testLabel.Id);
            
            Assert.Null(labelWithUser.LastUpdatedByUser);
        }

        [Fact]
        public async Task UpdateSensitivityLabel_Success_UpdatesLastUpdatedByUserId()
        {
            // Arrange
            var testLabel = new SensitivityLabel
            {
                Name = "Test Label Update",
                Description = "Test description 4",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.SensitivityLabels.Add(testLabel);
            await Context.SaveChangesAsync();

            // Act
            testLabel.LastUpdatedBy = uid;
            testLabel.Name = "Updated Label Name";
            testLabel.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            
            Context.SensitivityLabels.Update(testLabel);
            await Context.SaveChangesAsync();

            // Assert
            var updatedLabel = await Context.SensitivityLabels
                .Include(l => l.LastUpdatedByUser)
                .FirstAsync(l => l.Id == testLabel.Id);
            
            Assert.Equal(uid, updatedLabel.LastUpdatedBy);
            Assert.NotNull(updatedLabel.LastUpdatedByUser);
            Assert.Equal("Test User", updatedLabel.LastUpdatedByUser.Name);
            Assert.Equal("Updated Label Name", updatedLabel.Name);
        }

        #endregion
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            
            var user = new User
            {
                Name = "Test User",
                Email = "test_label@example.com",
                Password = "test_password",
                IsArchived = false
            };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            uid = user.Id;
            
            // create test organization
            var testOrg = new Organization
            {
                Name = "Test Organization",
                Description = "Test org for unit tests",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid,
                IsArchived = false
            };
            Context.Organizations.Add(testOrg);
            await Context.SaveChangesAsync();
            oid = testOrg.Id;

            // create test project
            var testProject = new Project
            {
                Name = "Test Project",
                Description = "Test project for unit tests",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid,
                IsArchived = false
            };
            Context.Projects.Add(testProject);
            await Context.SaveChangesAsync();
            pid = testProject.Id;
            
            // create test labels
            var testLabel = new SensitivityLabel
            {
                Name = "Test Label",
                Description = "Test label for unit tests",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid,
                IsArchived = false
            };
            var archivedLabel = new SensitivityLabel
            {
                Name = "Archived Label",
                Description = "Archived label for tests",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid,
                IsArchived = true
            };
            Context.SensitivityLabels.AddRange(testLabel, archivedLabel);
            await Context.SaveChangesAsync();
            lid = testLabel.Id;
            lid2 = archivedLabel.Id;
        }
    }
}