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
    public class AuthInOrgMiddlewareTests : IntegrationTestBase
    {
        private Mock<IOrgRolePermissionService> _rolePermissionServiceMock;
        private Mock<ILogger<OrgRolePermissionService>> _loggerMock;

        public long userId1;
        public long userId2;
        public long organizationId1;
        public long organizationId2;
        public long roleId1;
        public long roleId2;
        public long permissionId1;
        public long permissionId2;

        public AuthInOrgMiddlewareTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _rolePermissionServiceMock = new Mock<IOrgRolePermissionService>();
            _loggerMock = new Mock<ILogger<OrgRolePermissionService>>();
            
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

            var middleware = new AuthInOrgMiddleware(next);

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

            var middleware = new AuthInOrgMiddleware(next);

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
            var context = CreateHttpContextWithAuth("read", "organization");
            // Don't set any authentication - UserContextStorage.UserId remains 0

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInOrgMiddleware(next);

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

            var context = CreateHttpContextWithAuth("read", "organization");
            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInOrgMiddleware(next);

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

            var context = CreateHttpContextWithAuth("read", "organization");
            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        #endregion

        #region Middleware Tests - Bad Request (Missing OrgId)

        [Fact]
        public async Task InvokeAsync_Returns400_WhenOrgIdMissingFromRoute()
        {
            // Arrange
            var context = new DefaultHttpContext();
            SetAuthenticatedUser(context, userId1);
            
            var endpoint = new Endpoint(
                requestDelegate: (ctx) => Task.CompletedTask,
                metadata: new EndpointMetadataCollection(new AuthInOrgAttribute("read", "organization")),
                displayName: "Test");
            context.SetEndpoint(endpoint);
            // No route values or query parameters

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns400_WhenOrgIdIsInvalid()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "organization");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["orgId"] = "not-a-number";

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns400_WhenOrgIdIsEmpty()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "organization");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["orgId"] = "";

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        #endregion

        #region Middleware Tests - OrgId Extraction

        [Fact]
        public async Task InvokeAsync_ExtractsOrgIdFromRoute_Successfully()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "organization");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["orgId"] = organizationId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ExtractsOrgIdFromQuery_WhenNotInRoute()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "organization");
            SetAuthenticatedUser(context, userId1);
            context.Request.QueryString = new QueryString($"?orgId={organizationId1}");

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_PrefersRouteValue_OverQueryParameter()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "organization");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["orgId"] = organizationId1.ToString();
            context.Request.QueryString = new QueryString($"?orgId={organizationId2}");

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
            // Should use route value (organizationId1), not query parameter (organizationId2)
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"),
                Times.Once);
        }

        #endregion

        #region Middleware Tests - Permission Checks

        [Fact]
        public async Task InvokeAsync_Returns403_WhenUserLacksPermission()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("write", "organization");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["orgId"] = organizationId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "write", "organization"))
                .ReturnsAsync(false);

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ContinuesPipeline_WhenUserHasPermission()
        {
            // Arrange
            var context = CreateHttpContextWithAuth("read", "organization");
            SetAuthenticatedUser(context, userId1);
            context.Request.RouteValues["orgId"] = organizationId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInOrgMiddleware(next);

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
                    new AuthInOrgAttribute("read", "organization"),
                    new AuthInOrgAttribute("write", "users")),
                displayName: "Test");
            context.SetEndpoint(endpoint);
            context.Request.RouteValues["orgId"] = organizationId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
                .ReturnsAsync(true);
            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "write", "users"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.True(nextCalled);
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"),
                Times.Once);
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInOrg(userId1, organizationId1, "write", "users"),
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
                    new AuthInOrgAttribute("read", "organization"),
                    new AuthInOrgAttribute("delete", "users")), // User doesn't have this
                displayName: "Test");
            context.SetEndpoint(endpoint);
            context.Request.RouteValues["orgId"] = organizationId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
                .ReturnsAsync(true);
            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "delete", "users"))
                .ReturnsAsync(false);

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInOrgMiddleware(next);

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
                    new AuthInOrgAttribute("delete", "organization"), // Should fail here
                    new AuthInOrgAttribute("write", "users")),
                displayName: "Test");
            context.SetEndpoint(endpoint);
            context.Request.RouteValues["orgId"] = organizationId1.ToString();

            _rolePermissionServiceMock
                .Setup(x => x.PermissionInOrg(userId1, organizationId1, "delete", "organization"))
                .ReturnsAsync(false);

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new AuthInOrgMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _rolePermissionServiceMock.Object);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            // Second permission check should not be called
            _rolePermissionServiceMock.Verify(
                x => x.PermissionInOrg(userId1, organizationId1, "write", "users"),
                Times.Never);
        }

        #endregion

        #region RolePermissionService Tests - Basic Permissions

        [Fact]
        public async Task PermissionInOrg_ReturnsTrue_WhenUserHasPermission()
        {
            // Arrange
            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInOrg(
                userId1, organizationId1, "read", "organization");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PermissionInOrg_ReturnsFalse_WhenUserLacksPermission()
        {
            // Arrange
            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act - user1 doesn't have "delete" permission
            var result = await service.PermissionInOrg(
                userId1, organizationId1, "delete", "organization");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInOrg_ReturnsFalse_WhenUserNotInOrganization()
        {
            // Arrange
            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act - userId2 is not a member of organizationId1
            var result = await service.PermissionInOrg(
                userId2, organizationId1, "read", "organization");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInOrg_ReturnsFalse_WhenOrganizationDoesNotExist()
        {
            // Arrange
            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInOrg(
                userId1, 99999, "read", "organization");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInOrg_ReturnsFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInOrg(
                99999, organizationId1, "read", "organization");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInOrg_ReturnsFalse_WhenRoleIsArchived()
        {
            // Arrange
            var archivedRole = new Role
            {
                Name = "Archived Org Role",
                OrganizationId = organizationId2,
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
            archivedRole.Permissions.Add(archivedPermission);
            Context.Permissions.Add(archivedPermission);
            await Context.SaveChangesAsync();

            // Add user2 to organizationId2
            var orgUser = new OrganizationUser
            {
                UserId = userId2,
                OrganizationId = organizationId2
            };
            Context.Set<OrganizationUser>().Add(orgUser);
            await Context.SaveChangesAsync();

            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInOrg(
                userId2, organizationId2, "archived-action", "archived-resource");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInOrg_ReturnsFalse_WhenPermissionIsArchived()
        {
            // Arrange
            var activeRole = new Role
            {
                Name = "Active Org Role",
                OrganizationId = organizationId2,
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
            activeRole.Permissions.Add(archivedPermission);
            Context.Permissions.Add(archivedPermission);
            await Context.SaveChangesAsync();

            // Add user2 to organizationId2
            var orgUser = new OrganizationUser
            {
                UserId = userId2,
                OrganizationId = organizationId2
            };
            Context.Set<OrganizationUser>().Add(orgUser);
            await Context.SaveChangesAsync();

            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act
            var result = await service.PermissionInOrg(
                userId2, organizationId2, "read", "archived-data");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region RolePermissionService Tests - Multiple Roles

        [Fact]
        public async Task PermissionInOrg_ReturnsTrue_WhenUserHasMultipleRolesInOrg()
        {
            // Arrange
            // Create a second role with write permission
            var writeRole = new Role
            {
                Name = "I am a writer role",
                OrganizationId = organizationId1,
                IsArchived = false
            };
            Context.Roles.Add(writeRole);
            await Context.SaveChangesAsync();

            var writePermission = Context.Permissions.Find(permissionId2);
            writeRole.Permissions.Add(writePermission);
            await Context.SaveChangesAsync();

            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act - user1 should have both read (from existing role) and write (from new role)
            var readResult = await service.PermissionInOrg(
                userId1, organizationId1, "read", "organization");
            var writeResult = await service.PermissionInOrg(
                userId1, organizationId1, "write", "organization");

            // Assert
            Assert.True(readResult);
            Assert.True(writeResult);
        }

        [Fact]
        public async Task PermissionInOrg_ReturnsTrue_WhenAnyRoleHasPermission()
        {
            // Arrange
            // Create a role with delete permission
            var deleteRole = new Role
            {
                Name = "Org Delete Role",
                OrganizationId = organizationId1,
                IsArchived = false
            };
            Context.Roles.Add(deleteRole);
            await Context.SaveChangesAsync();

            var deletePermission = new Permission
            {
                Name = "Delete Organization",
                Action = "delete",
                Resource = "organization",
                IsArchived = false
            };
            deleteRole.Permissions.Add(deletePermission);
            Context.Permissions.Add(deletePermission);
            await Context.SaveChangesAsync();

            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act - user1 now has read (from existing role) and delete (from new role)
            var readResult = await service.PermissionInOrg(
                userId1, organizationId1, "read", "organization");
            var deleteResult = await service.PermissionInOrg(
                userId1, organizationId1, "delete", "organization");

            // Assert
            Assert.True(readResult);
            Assert.True(deleteResult);
        }

        #endregion

        #region RolePermissionService Tests - Case Sensitivity

        [Fact]
        public async Task PermissionInOrg_IsCaseSensitive_ForAction()
        {
            // Arrange
            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act - "Read" vs "read"
            var result = await service.PermissionInOrg(
                userId1, organizationId1, "Read", "organization");

            // Assert - should be case sensitive
            Assert.False(result);
        }

        [Fact]
        public async Task PermissionInOrg_IsCaseSensitive_ForResource()
        {
            // Arrange
            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act - "Organization" vs "organization"
            var result = await service.PermissionInOrg(
                userId1, organizationId1, "read", "Organization");

            // Assert - should be case sensitive
            Assert.False(result);
        }

        #endregion

        #region RolePermissionService Tests - Different Resources

        [Fact]
        public async Task PermissionInOrg_CanCheckDifferentResources()
        {
            // Arrange
            var usersPermission = new Permission
            {
                Name = "Manage Users",
                Action = "write",
                Resource = "users",
                IsArchived = false
            };
            Context.Roles.Find(roleId1).Permissions.Add(usersPermission);
            Context.Permissions.Add(usersPermission);
            await Context.SaveChangesAsync();
            
            await Context.SaveChangesAsync();

            var service = new OrgRolePermissionService(Context, _loggerMock.Object);

            // Act
            var orgResult = await service.PermissionInOrg(
                userId1, organizationId1, "read", "organization");
            var usersResult = await service.PermissionInOrg(
                userId1, organizationId1, "write", "users");

            // Assert
            Assert.True(orgResult);
            Assert.True(usersResult);
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
                metadata: new EndpointMetadataCollection(new AuthInOrgAttribute(action, resource)),
                displayName: "Test");
            context.SetEndpoint(endpoint);

            return context;
        }

        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();

            // Create organizations
            var organization1 = new Organization
            {
                Name = "Test Organization 1",
                Description = "Test Organization 1 Description"
            };
            Context.Organizations.Add(organization1);

            var organization2 = new Organization
            {
                Name = "Test Organization 2",
                Description = "Test Organization 2 Description"
            };
            Context.Organizations.Add(organization2);

            await Context.SaveChangesAsync();
            organizationId1 = organization1.Id;
            organizationId2 = organization2.Id;

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

            // Create permissions
            var permission1 = new Permission
            {
                Name = "Read Organization",
                Action = "read",
                Resource = "organization",
                IsArchived = false
            };
            Context.Permissions.Add(permission1);

            var permission2 = new Permission
            {
                Name = "Write Organization",
                Action = "write",
                Resource = "organization",
                IsArchived = false
            };
            Context.Permissions.Add(permission2);

            await Context.SaveChangesAsync();
            permissionId1 = permission1.Id;
            permissionId2 = permission2.Id;

            // Create roles for organization1
            var role1 = new Role
            {
                Name = "Org Reader Role",
                OrganizationId = organizationId1,
                IsArchived = false
            };
            role1.Permissions.Add(permission1);
            Context.Roles.Add(role1);

            var role2 = new Role
            {
                Name = "Org Writer Role",
                OrganizationId = organizationId1,
                IsArchived = false
            };
            role2.Permissions.Add(permission2);
            Context.Roles.Add(role2);

            await Context.SaveChangesAsync();
            roleId1 = role1.Id;
            roleId2 = role2.Id;

            // Add user1 to organization1
            var orgUser = new OrganizationUser
            {
                UserId = userId1,
                OrganizationId = organizationId1
            };
            Context.Set<OrganizationUser>().Add(orgUser);

            await Context.SaveChangesAsync();
        }
    }
}