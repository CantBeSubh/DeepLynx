using System.Security.Claims;
using System.Text;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests.Middleware
{
    [Collection("Test Suite Collection")]
    public class AuthInProjectMiddlewareTests : IntegrationTestBase
    {
        private Mock<IProjectRolePermissionService> _rolePermissionServiceMock;
        private Mock<ILogger<ProjectRolePermissionService>> _loggerMock;

        public long userId1;
        public long userId2;
        public long projectId1;
        public long projectId2;
        public long roleId1;
        public long roleId2;
        public long permissionId1;
        public long permissionId2;
        public long groupId1;
        public long organizationId1;

        public AuthInProjectMiddlewareTests(TestSuiteFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _rolePermissionServiceMock = new Mock<IProjectRolePermissionService>();
            _loggerMock = new Mock<ILogger<ProjectRolePermissionService>>();

            // Reset UserContextStorage before each test
            UserContextStorage.UserId = 0;
        }

        #region Middleware Tests - No Auth Attributes

        [Fact]
        public async Task InvokeAsync_ContinuesPipeline_WhenNoEndpoint()
        {
            // Arrange
            var context = new DefaultHttpContext();
            SetAuthenticatedUser(context, userId1);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ContinuesPipeline_WhenNoAuthAttributes()
        {
            // Arrange
            var context = new DefaultHttpContext();
            SetAuthenticatedUser(context, userId1);

            var endpoint = new Endpoint(
                requestDelegate: (ctx) => Task.CompletedTask,
                metadata: new EndpointMetadataCollection(),
                displayName: "Test");
            context.SetEndpoint(endpoint);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
        }

        #endregion

        #region Middleware Tests - Unauthorized

        [Fact]
        public async Task InvokeAsync_Returns401_WhenUserNotAuthenticated()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "project");
            // Don't set any authentication - UserContextStorage.UserId remains 0

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns401_WhenUserIdIsZero()
        {
            // Arrange
            UserContextStorage.UserId = 0;

            var context = CreateHttpContextWithAuth("read", "project");
            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns401_WhenUserIdIsNegative()
        {
            // Arrange
            UserContextStorage.UserId = -1;

            var context = CreateHttpContextWithAuth("read", "project");
            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        #endregion

        #region Middleware Tests - Bad Request (Missing ProjectId)

        [Fact]
        public async Task InvokeAsync_Returns400_WhenProjectIdMissingFromRoute()
        {
            // Arrange
            var context = new DefaultHttpContext();
            SetAuthenticatedUser(context, userId1);

            var endpoint = new Endpoint(
                requestDelegate: (ctx) => Task.CompletedTask,
                metadata: new EndpointMetadataCollection(new AuthInProjectAttribute("read", "project")),
                displayName: "Test");
            context.SetEndpoint(endpoint);
            // No route values or query parameters

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns400_WhenProjectIdIsInvalid()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "project");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["projectId"] = "not-a-number";

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns400_WhenProjectIdIsEmpty()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "project");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["projectId"] = "";

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        #endregion

        #region Middleware Tests - ProjectId Extraction

        [Fact]
        public async Task InvokeAsync_ExtractsProjectIdFromRoute_Successfully()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "project");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["projectId"] = projectId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInProject(userId1, projectId1, "read", "project"),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ExtractsProjectIdFromQuery_WhenNotInRoute()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "project");
            SetAuthenticatedUser(context, userId1);
            context.Request.QueryString = new QueryString($"?projectId={projectId1}");

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInProject(userId1, projectId1, "read", "project"),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_PrefersRouteValue_OverQueryParameter()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "project");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["projectId"] = projectId1.ToString();
            context.Request.QueryString = new QueryString($"?projectId={projectId2}");

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
            // Should use route value (projectId1), not query parameter (projectId2)
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInProject(userId1, projectId1, "read", "project"),
                Times.Once);
        }

        #endregion

        #region Middleware Tests - Permission Checks

        [Fact]
        public async Task InvokeAsync_Returns403_WhenUserLacksPermission()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("write", "project");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["projectId"] = projectId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "write", "project"))
                .ReturnsAsync(false);

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ContinuesPipeline_WhenUserHasPermission()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "project");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["projectId"] = projectId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ChecksAllAttributes_WhenMultiplePresent()
        {
            // Arrange
            var context = new DefaultHttpContext();
            SetAuthenticatedUser(context, userId1);

            var endpoint = new Endpoint(
                requestDelegate: (ctx) => Task.CompletedTask,
                metadata: new EndpointMetadataCollection(
                    new AuthInProjectAttribute("read", "project"),
                    new AuthInProjectAttribute("write", "data")),
                displayName: "Test");
            context.SetEndpoint(endpoint);
            context.Request.RouteValues["projectId"] = projectId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
                .ReturnsAsync(true);
            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "write", "data"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInProject(userId1, projectId1, "read", "project"),
                Times.Once);
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInProject(userId1, projectId1, "write", "data"),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_Returns403_WhenAnyPermissionCheckFails()
        {
            // Arrange
            var context = new DefaultHttpContext();
            SetAuthenticatedUser(context, userId1);

            var endpoint = new Endpoint(
                requestDelegate: (ctx) => Task.CompletedTask,
                metadata: new EndpointMetadataCollection(
                    new AuthInProjectAttribute("read", "project"),
                    new AuthInProjectAttribute("delete", "data")), // User doesn't have this
                displayName: "Test");
            context.SetEndpoint(endpoint);
            context.Request.RouteValues["projectId"] = projectId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
                .ReturnsAsync(true);
            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "delete", "data"))
                .ReturnsAsync(false);

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_StopsAtFirstFailure_WhenMultiplePermissions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            SetAuthenticatedUser(context, userId1);

            var endpoint = new Endpoint(
                requestDelegate: (ctx) => Task.CompletedTask,
                metadata: new EndpointMetadataCollection(
                    new AuthInProjectAttribute("delete", "project"), // Should fail here
                    new AuthInProjectAttribute("write", "data")),
                displayName: "Test");
            context.SetEndpoint(endpoint);
            context.Request.RouteValues["projectId"] = projectId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInProject(userId1, projectId1, "delete", "project"))
                .ReturnsAsync(false);

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInProjectMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            // Second permission check should not be called
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInProject(userId1, projectId1, "write", "data"),
                Times.Never);
        }

        #endregion

        #region RolePermissionService Tests - Direct Permissions

        [Fact]
        public async Task PermissionInProject_ReturnsTrue_WhenUserHasDirectPermission()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInProject(
                userId1, projectId1, "read", "project");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsFalse_WhenUserLacksPermission()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act - user1 doesn't have "delete" permission
            var result = await service.PermissionInProject(
                userId1, projectId1, "delete", "project");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsFalse_WhenUserNotInProject()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act - userId2 is not a member of projectId1
            var result = await service.PermissionInProject(
                userId2, projectId1, "read", "project");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsFalse_WhenProjectDoesNotExist()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInProject(
                userId1, 99999, "read", "project");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInProject(
                99999, projectId1, "read", "project");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsFalse_WhenRoleIsArchived()
        {
            // Arrange
            var archivedRole = new Role
            {
                Name = "Archived Role",
                IsArchived = true
            };
            Context.Roles.Add(archivedRole);
            await Context.SaveChangesAsync();

            var archivedPermission = new Permission
            {
                Name = "Archived Action Permission",
                Action = "archived-action",
                Resource = "archived-resource",
                IsArchived = false
            };
            Context.Permissions.Add(archivedPermission);
            await Context.SaveChangesAsync();

            archivedRole.Permissions.Add(archivedPermission);
            await Context.SaveChangesAsync();

            // Use userId2 and projectId2 to avoid conflicts with existing memberships
            var projectMember = new ProjectMember
            {
                UserId = userId2,
                ProjectId = projectId2,
                RoleId = archivedRole.Id
            };
            Context.ProjectMembers.Add(projectMember);
            await Context.SaveChangesAsync();

            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInProject(
                userId2, projectId2, "archived-action", "archived-resource");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsFalse_WhenPermissionIsArchived()
        {
            // Arrange
            var activeRole = new Role
            {
                Name = "Active Role",
                IsArchived = false
            };
            Context.Roles.Add(activeRole);
            await Context.SaveChangesAsync();

            var archivedPermission = new Permission
            {
                Name = "Read Archived Data",
                Action = "read",
                Resource = "archived-data",
                IsArchived = true
            };
            Context.Permissions.Add(archivedPermission);
            await Context.SaveChangesAsync();

            activeRole.Permissions.Add(archivedPermission);
            await Context.SaveChangesAsync();

            // Use userId2 and projectId2 to avoid conflicts with existing memberships
            var projectMember = new ProjectMember
            {
                UserId = userId2,
                ProjectId = projectId2,
                RoleId = activeRole.Id
            };
            Context.ProjectMembers.Add(projectMember);
            await Context.SaveChangesAsync();

            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInProject(
                userId2, projectId2, "read", "archived-data");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsFalse_WhenRoleIdIsNull()
        {
            // Arrange
            var projectMember = new ProjectMember
            {
                UserId = userId2,
                ProjectId = projectId2,
                RoleId = null // No role assigned
            };
            Context.ProjectMembers.Add(projectMember);
            await Context.SaveChangesAsync();

            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInProject(
                userId2, projectId2, "read", "project");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region RolePermissionService Tests - Group Permissions

        [Fact]
        public async Task PermissionInProject_ReturnsTrue_WhenUserHasGroupPermission()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // userId1 is in groupId1, which has write permission in projectId1

            // Act
            var result = await service.PermissionInProject(
                userId1, projectId1, "write", "data");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsFalse_WhenGroupLacksPermission()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act - group doesn't have delete permission
            var result = await service.PermissionInProject(
                userId1, projectId1, "delete", "data");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsTrue_WhenUserHasDirectOrGroupPermission()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // userId1 has direct "read" permission and group "write" permission

            // Act - check direct permission
            var directResult = await service.PermissionInProject(
                userId1, projectId1, "read", "project");

            // Act - check group permission
            var groupResult = await service.PermissionInProject(
                userId1, projectId1, "write", "data");

            // Assert
            Assert.True(directResult);
            Assert.True(groupResult);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsFalse_WhenNotInAnyGroup()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // userId2 is not in any groups

            // Act
            var result = await service.PermissionInProject(
                userId2, projectId1, "write", "data");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region RolePermissionService Tests - Multiple Roles

        [Fact]
        public async Task PermissionInProject_ReturnsTrue_WhenUserHasMultipleRoles()
        {
            // Arrange
            // Add second role membership for user1
            var projectMember = new ProjectMember
            {
                UserId = userId1,
                ProjectId = projectId1,
                RoleId = roleId2
            };
            Context.ProjectMembers.Add(projectMember);
            await Context.SaveChangesAsync();

            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act - check permission from second role
            var result = await service.PermissionInProject(
                userId1, projectId1, "write", "project");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PermissionInProject_ReturnsTrue_WhenAnyRoleHasPermission()
        {
            // Arrange
            // Add a third role with different permission
            var deletePermission = new Permission
            {
                Name = "Delete Project",
                Action = "delete",
                Resource = "project",
                IsArchived = false
            };
            Context.Permissions.Add(deletePermission);

            var deleteRole = new Role
            {
                Name = "Deleter Role",
                IsArchived = false
            };
            deleteRole.Permissions.Add(deletePermission);
            Context.Roles.Add(deleteRole);
            await Context.SaveChangesAsync();

            var projectMember = new ProjectMember
            {
                UserId = userId1,
                ProjectId = projectId1,
                RoleId = deleteRole.Id
            };
            Context.ProjectMembers.Add(projectMember);
            await Context.SaveChangesAsync();

            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act - user1 now has read (role1), write (role2), and delete (deleteRole)
            var readResult = await service.PermissionInProject(
                userId1, projectId1, "read", "project");
            var deleteResult = await service.PermissionInProject(
                userId1, projectId1, "delete", "project");

            // Assert
            Assert.True(readResult);
            Assert.True(deleteResult);
        }

        #endregion

        #region RolePermissionService Tests - Case Sensitivity

        [Fact]
        public async Task PermissionInProject_IsCaseSensitive_ForAction()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act - "Read" vs "read"
            var result = await service.PermissionInProject(
                userId1, projectId1, "Read", "project");

            // Assert - should be case sensitive
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInProject_IsCaseSensitive_ForResource()
        {
            // Arrange
            var service = new ProjectRolePermissionService(Context, _loggerMock.Object);

            // Act - "Project" vs "project"
            var result = await service.PermissionInProject(
                userId1, projectId1, "read", "Project");

            // Assert - should be case sensitive
            Assert.False(result);
        }

        #endregion

        // Helper methods

        private void SetAuthenticatedUser(HttpContext context, long userId)
        {
            // Set the UserContextStorage which the middleware reads
            UserContextStorage.UserId = userId;

            // Also set the User principal for completeness
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, $"user{userId}@test.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        private HttpContext CreateHttpContextWithAuth(string action, string resource)
        {
            var context = new DefaultHttpContext();
            var endpoint = new Endpoint(
                requestDelegate: (ctx) => Task.CompletedTask,
                metadata: new EndpointMetadataCollection(new AuthInProjectAttribute(action, resource)),
                displayName: "Test");
            context.SetEndpoint(endpoint);

            return context;
        }

        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();

            // Create organization first (required for projects AND groups)
            var organization1 = new Organization
            {
                Name = $"Test Organization {Guid.NewGuid()}", // Make unique to avoid conflicts
                Description = "Test Organization Description"
            };
            Context.Organizations.Add(organization1);
            await Context.SaveChangesAsync();
            organizationId1 = organization1.Id;

            // Create users
            var user1 = new User
            {
                Name = "Test User 1",
                Email = "user1@test.com",
                Username = "user1",
                IsActive = true,
                IsArchived = false
            };
            Context.Users.Add(user1);

            var user2 = new User
            {
                Name = "Test User 2",
                Email = "user2@test.com",
                Username = "user2",
                IsActive = true,
                IsArchived = false
            };
            Context.Users.Add(user2);

            await Context.SaveChangesAsync();
            userId1 = user1.Id;
            userId2 = user2.Id;

            // Create projects - NOW WITH ORGANIZATION ID
            var project1 = new Project
            {
                Name = "Test Project 1",
                Description = "Test Description 1",
                OrganizationId = organizationId1, // ← FIX: Add the foreign key
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Projects.Add(project1);

            var project2 = new Project
            {
                Name = "Test Project 2",
                Description = "Test Description 2",
                OrganizationId = organizationId1, // ← FIX: Add the foreign key
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Projects.Add(project2);

            await Context.SaveChangesAsync();
            projectId1 = project1.Id;
            projectId2 = project2.Id;

            // Create permissions
            var permission1 = new Permission
            {
                Name = "Read Project",
                Action = "read",
                Resource = "project",
                IsArchived = false
            };
            Context.Permissions.Add(permission1);

            var permission2 = new Permission
            {
                Name = "Write Project",
                Action = "write",
                Resource = "project",
                IsArchived = false
            };
            Context.Permissions.Add(permission2);

            var permission3 = new Permission
            {
                Name = "Write Data",
                Action = "write",
                Resource = "data",
                IsArchived = false
            };
            Context.Permissions.Add(permission3);

            await Context.SaveChangesAsync();
            permissionId1 = permission1.Id;
            permissionId2 = permission2.Id;

            // Create roles
            var role1 = new Role
            {
                Name = "Reader Role",
                IsArchived = false
            };
            role1.Permissions.Add(permission1);
            Context.Roles.Add(role1);

            var role2 = new Role
            {
                Name = "Writer Role",
                IsArchived = false
            };
            role2.Permissions.Add(permission2);
            Context.Roles.Add(role2);

            var role3 = new Role
            {
                Name = "Data Writer Role",
                IsArchived = false
            };
            role3.Permissions.Add(permission3);
            Context.Roles.Add(role3);

            await Context.SaveChangesAsync();
            roleId1 = role1.Id;
            roleId2 = role2.Id;

            // Create group
            var group1 = new Group
            {
                Name = "Test Group 1",
                OrganizationId = organizationId1
            };
            Context.Groups.Add(group1);
            await Context.SaveChangesAsync();
            groupId1 = group1.Id;

            // Add user1 to group1 using the navigation property
            group1.Users.Add(user1);
            await Context.SaveChangesAsync();

            // Add direct project membership for user1 with role1
            var projectMember1 = new ProjectMember
            {
                UserId = userId1,
                ProjectId = projectId1,
                RoleId = roleId1
            };
            Context.ProjectMembers.Add(projectMember1);

            // Add group project membership with role3 (write data permission)
            var projectMember2 = new ProjectMember
            {
                GroupId = groupId1,
                ProjectId = projectId1,
                RoleId = role3.Id
            };
            Context.ProjectMembers.Add(projectMember2);

            await Context.SaveChangesAsync();
        }
    }
}