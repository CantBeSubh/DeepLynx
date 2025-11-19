using System.Security.Claims;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests.Middleware;

[Collection("Test Suite Collection")]
public class AuthMiddlewareTests : IntegrationTestBase
{
    private Mock<ILogger<OrgRolePermissionService>> _orgLoggerMock;
    private Mock<IOrgRolePermissionService> _orgRolePermissionServiceMock;
    private Mock<ILogger<ProjectRolePermissionService>> _projectLoggerMock;
    private Mock<IProjectRolePermissionService> _projectRolePermissionServiceMock;
    private Mock<ISysAdminService> _sysAdminServiceMock;
    private Mock<ILogger<SysAdminService>> _sysAdminLoggerMock;
    private Mock<IOrganizationService> _organizationServiceMock;

    public long groupId1;
    public long organizationId1;
    public long organizationId2;
    public long permissionId1;
    public long permissionId2;
    public long permissionId3;
    public long projectId1;
    public long projectId2;
    public long roleId1;
    public long roleId2;
    public long roleId3;

    public long userId1;
    public long userId2;
    public long userId3;

    public AuthMiddlewareTests(TestSuiteFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _orgRolePermissionServiceMock = new Mock<IOrgRolePermissionService>();
        _projectRolePermissionServiceMock = new Mock<IProjectRolePermissionService>();
        _sysAdminServiceMock = new Mock<ISysAdminService>();
        _orgLoggerMock = new Mock<ILogger<OrgRolePermissionService>>();
        _projectLoggerMock = new Mock<ILogger<ProjectRolePermissionService>>();
        _sysAdminLoggerMock = new Mock<ILogger<SysAdminService>>();
        _organizationServiceMock = new Mock<IOrganizationService>();
        

        // Reset UserContextStorage before each test
        UserContextStorage.UserId = 0;
    }

    // Helper methods

    private void SetAuthenticatedUser(HttpContext context, long userId)
    {
        UserContextStorage.UserId = userId;

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
            ctx => Task.CompletedTask,
            new EndpointMetadataCollection(new AuthAttribute(action, resource)),
            "Test");
        context.SetEndpoint(endpoint);

        return context;
    }

    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();

        // Create organizations
        var organization1 = new Organization
        {
            Name = $"Test Organization 1 {Guid.NewGuid()}",
            Description = "Test Organization 1 Description"
        };
        Context.Organizations.Add(organization1);

        var organization2 = new Organization
        {
            Name = $"Test Organization 2 {Guid.NewGuid()}",
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
        
        // Create users
        var user2 = new User
        {
            Name = "Test User 2",
            Email = "user2@test.com",
            Username = "user2",
            IsActive = true,
            IsArchived = false, 
            IsSysAdmin = true
        };
        Context.Users.Add(user2);
        
        var user3 = new User
        {
            Name = "Test User 3",
            Email = "user3@test.com",
            Username = "user3",
            IsActive = true,
            IsArchived = false
        };
        Context.Users.Add(user3);

        await Context.SaveChangesAsync();
        userId1 = user1.Id;
        userId2 = user2.Id;
        userId3 = user3.Id;

        // Create projects
        var project1 = new Project
        {
            Name = "Test Project 1",
            Description = "Test Description 1",
            OrganizationId = organizationId1,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Projects.Add(project1);

        var project2 = new Project
        {
            Name = "Test Project 2",
            Description = "Test Description 2",
            OrganizationId = organizationId1,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Projects.Add(project2);

        await Context.SaveChangesAsync();
        projectId1 = project1.Id;
        projectId2 = project2.Id;
        
        // Add user3 to organization1
        var orgUser = new OrganizationUser
        {
            UserId = userId3,
            OrganizationId = organizationId1, 
            IsOrgAdmin = true
        };
        Context.Set<OrganizationUser>().Add(orgUser);
        await Context.SaveChangesAsync();
    }

    #region Middleware Tests - No Auth Attributes

    [Fact]
    public async Task InvokeAsync_ContinuesPipeline_WhenNoEndpoint()
    {
        // Arrange
        var context = new DefaultHttpContext();
        SetAuthenticatedUser(context, userId1);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

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
            ctx => Task.CompletedTask,
            new EndpointMetadataCollection(),
            "Test");
        context.SetEndpoint(endpoint);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

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

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Returns401_WhenUserIdIsZero()
    {
        // Arrange
        UserContextStorage.UserId = 0;

        var context = CreateHttpContextWithAuth("read", "organization");
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Returns401_WhenUserIdIsNegative()
    {
        // Arrange
        UserContextStorage.UserId = -1;

        var context = CreateHttpContextWithAuth("read", "organization");
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    #endregion

    #region Middleware Tests - Bad Request (Missing IDs)

    [Fact]
    public async Task InvokeAsync_Returns400_WhenBothIdsAreMissing()
    {
        // Arrange
        var context = new DefaultHttpContext();
        SetAuthenticatedUser(context, userId1);

        var endpoint = new Endpoint(
            ctx => Task.CompletedTask,
            new EndpointMetadataCollection(new AuthAttribute("read", "organization")),
            "Test");
        context.SetEndpoint(endpoint);
        // No route values or query parameters

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Returns400_WhenOrgIdIsInvalid()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("read", "organization");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["organizationId"] = "not-a-number";

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    #endregion

    #region Middleware Tests - Admins
    
    [Fact]
    public async Task InvokeAsync_Passes_WhenUserIsSysAdmin()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("delete", "data");
        SetAuthenticatedUser(context, userId2);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();
        context.Request.RouteValues["projectId"] = projectId1.ToString();

        // User is sysadmin
        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId2, organizationId1, "delete", "data"))
            .ReturnsAsync(true);
        _projectRolePermissionServiceMock
            .Setup(x => x.PermissionInProject(userId2, projectId1, "delete", "data"))
            .ReturnsAsync(false);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
    }
    
    [Fact]
    public async Task InvokeAsync_Passes_WhenUserHasOrgPermission()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("delete", "data");
        SetAuthenticatedUser(context, userId3);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();
    
        // User has org permission as an org admin 
        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId3, organizationId1, "delete", "data"))
            .ReturnsAsync(true);
    
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
    
        var middleware = new AuthMiddleware(next);
    
        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);
    
        // Assert
        Assert.True(nextCalled);
    }
    
    #endregion
    
    #region Middleware Tests - Organization Only

    [Fact]
    public async Task InvokeAsync_ExtractsOrgIdFromRoute_Successfully()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("read", "organization");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();

        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
            .ReturnsAsync(true);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
        _orgRolePermissionServiceMock.Verify(
            x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"),
            Times.Once);
    }
    

    [Fact]
    public async Task InvokeAsync_Returns403_WhenUserLacksOrgPermission()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("write", "organization");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();

        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "write", "organization"))
            .ReturnsAsync(false);

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    #endregion

    #region Middleware Tests - Project Only

    [Fact]
    public async Task InvokeAsync_ExtractsProjectIdFromRoute_Successfully()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("read", "project");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["projectId"] = projectId1.ToString();

        _projectRolePermissionServiceMock
            .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
            .ReturnsAsync(true);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
        _projectRolePermissionServiceMock.Verify(
            x => x.PermissionInProject(userId1, projectId1, "read", "project"),
            Times.Once);
    }
    

    [Fact]
    public async Task InvokeAsync_Returns403_WhenUserLacksProjectPermission()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("write", "project");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["projectId"] = projectId1.ToString();

        _projectRolePermissionServiceMock
            .Setup(x => x.PermissionInProject(userId1, projectId1, "write", "project"))
            .ReturnsAsync(false);

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    #endregion

    #region Middleware Tests - Both Org and Project (OR Logic)

    [Fact]
    public async Task InvokeAsync_PassesWithOrgPermission_WhenBothIdsPresent()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("read", "data");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();
        context.Request.RouteValues["projectId"] = projectId1.ToString();

        // User has org permission but NOT project permission
        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "data"))
            .ReturnsAsync(true);
        _projectRolePermissionServiceMock
            .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "data"))
            .ReturnsAsync(false);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_PassesWithProjectPermission_WhenBothIdsPresent()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("write", "data");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();
        context.Request.RouteValues["projectId"] = projectId1.ToString();

        // User has project permission but NOT org permission
        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "write", "data"))
            .ReturnsAsync(false);
        _projectRolePermissionServiceMock
            .Setup(x => x.PermissionInProject(userId1, projectId1, "write", "data"))
            .ReturnsAsync(true);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_Returns403_WhenLacksBothPermissions()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("delete", "data");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();
        context.Request.RouteValues["projectId"] = projectId1.ToString();

        // User lacks BOTH org and project permission
        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "delete", "data"))
            .ReturnsAsync(false);
        _projectRolePermissionServiceMock
            .Setup(x => x.PermissionInProject(userId1, projectId1, "delete", "data"))
            .ReturnsAsync(false);

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_PassesWithBothPermissions_WhenBothIdsPresent()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("read", "project");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();
        context.Request.RouteValues["projectId"] = projectId1.ToString();

        // User has BOTH permissions
        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "project"))
            .ReturnsAsync(true);
        _projectRolePermissionServiceMock
            .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
            .ReturnsAsync(true);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ChecksBothScopes_WithBothIds()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("read", "data");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();
        context.Request.RouteValues["projectId"] = projectId1.ToString();

        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "data"))
            .ReturnsAsync(false);
        _projectRolePermissionServiceMock
            .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "data"))
            .ReturnsAsync(true);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
        // Verify project wins over organization permissions
        _projectRolePermissionServiceMock.Verify(
            x => x.PermissionInProject(userId1, projectId1, "read", "data"),
            Times.Once);
        _orgRolePermissionServiceMock.Verify(
            x => x.PermissionInOrg(userId1, organizationId1, "read", "data"),
            Times.Never);
    }

    #endregion

    #region Middleware Tests - Multiple Attributes

    [Fact]
    public async Task InvokeAsync_ChecksAllAttributes_WhenMultiplePresent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        SetAuthenticatedUser(context, userId1);

        var endpoint = new Endpoint(
            ctx => Task.CompletedTask,
            new EndpointMetadataCollection(
                new AuthAttribute("read", "organization"),
                new AuthAttribute("write", "data")),
            "Test");
        context.SetEndpoint(endpoint);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();

        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
            .ReturnsAsync(true);
        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "write", "data"))
            .ReturnsAsync(true);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
        _orgRolePermissionServiceMock.Verify(
            x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"),
            Times.Once);
        _orgRolePermissionServiceMock.Verify(
            x => x.PermissionInOrg(userId1, organizationId1, "write", "data"),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Returns403_WhenAnyAttributeCheckFails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        SetAuthenticatedUser(context, userId1);

        var endpoint = new Endpoint(
            ctx => Task.CompletedTask,
            new EndpointMetadataCollection(
                new AuthAttribute("read", "organization"),
                new AuthAttribute("delete", "data")), // User doesn't have this
            "Test");
        context.SetEndpoint(endpoint);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();

        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
            .ReturnsAsync(true);
        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "delete", "data"))
            .ReturnsAsync(false);

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_StopsAtFirstFailure_WithMultipleAttributes()
    {
        // Arrange
        var context = new DefaultHttpContext();
        SetAuthenticatedUser(context, userId1);

        var endpoint = new Endpoint(
            ctx => Task.CompletedTask,
            new EndpointMetadataCollection(
                new AuthAttribute("delete", "organization"), // Should fail here
                new AuthAttribute("write", "data")),
            "Test");
        context.SetEndpoint(endpoint);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();

        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "delete", "organization"))
            .ReturnsAsync(false);

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        // Second permission check should not be called
        _orgRolePermissionServiceMock.Verify(
            x => x.PermissionInOrg(userId1, organizationId1, "write", "data"),
            Times.Never);
    }

    #endregion

    #region Middleware Tests - Preference Priority

    [Fact]
    public async Task InvokeAsync_PrefersRouteOverQuery_ForOrgId()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("read", "organization");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["organizationId"] = organizationId1.ToString();
        context.Request.QueryString = new QueryString($"?organizationId={organizationId2}");

        _orgRolePermissionServiceMock
            .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
            .ReturnsAsync(true);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
        // Should use route value (organizationId1), not query parameter (organizationId2)
        _orgRolePermissionServiceMock.Verify(
            x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_PrefersRouteOverQuery_ForProjectId()
    {
        // Arrange
        var context = CreateHttpContextWithAuth("read", "project");
        SetAuthenticatedUser(context, userId1);
        context.Request.RouteValues["projectId"] = projectId1.ToString();
        context.Request.QueryString = new QueryString($"?projectId={projectId2}");

        _projectRolePermissionServiceMock
            .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
            .ReturnsAsync(true);

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
        _projectRolePermissionServiceMock.Verify(
            x => x.PermissionInProject(userId1, projectId1, "read", "project"),
            Times.Once);
    }

    #endregion
    
    #region Middleware Tests - SysAdmin Check Mock Setup

[Fact]
public async Task InvokeAsync_CallsSysAdminCheck_ForNonSysAdmin()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = organizationId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _orgRolePermissionServiceMock
        .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
        .ReturnsAsync(true);

    var nextCalled = false;
    RequestDelegate next = ctx =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    };

    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.True(nextCalled);
    _sysAdminServiceMock.Verify(x => x.SysAdminCheck(userId1), Times.Once);
}

[Fact]
public async Task InvokeAsync_SkipsPermissionChecks_WhenUserIsSysAdmin()
{
    // Arrange
    var context = CreateHttpContextWithAuth("delete", "data");
    SetAuthenticatedUser(context, userId2);
    context.Request.RouteValues["organizationId"] = organizationId1.ToString();
    context.Request.RouteValues["projectId"] = projectId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId2))
        .ReturnsAsync(true);

    var nextCalled = false;
    RequestDelegate next = ctx =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    };

    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.True(nextCalled);
    _sysAdminServiceMock.Verify(x => x.SysAdminCheck(userId2), Times.Once);
    // Verify permission checks were NOT called
    _orgRolePermissionServiceMock.Verify(
        x => x.PermissionInOrg(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
    _projectRolePermissionServiceMock.Verify(
        x => x.PermissionInProject(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
}

[Fact]
public async Task InvokeAsync_AllowsSysAdmin_EvenWithoutIds()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId2); // sysadmin
    // No route values or query parameters

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId2))
        .ReturnsAsync(true);

    var nextCalled = false;
    RequestDelegate next = ctx =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    };

    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.True(nextCalled);
    _sysAdminServiceMock.Verify(x => x.SysAdminCheck(userId2), Times.Once);
}

#endregion

#region Middleware Tests - OrganizationService.CheckExistence

[Fact]
public async Task InvokeAsync_CallsCheckExistence_WithBothIds()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = organizationId1.ToString();
    context.Request.RouteValues["projectId"] = projectId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _orgRolePermissionServiceMock
        .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
        .ReturnsAsync(true);

    var nextCalled = false;
    RequestDelegate next = ctx =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    };

    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.True(nextCalled);
    _organizationServiceMock.Verify(
        x => x.CheckExistence(projectId1, organizationId1),
        Times.Once);
}

[Fact]
public async Task InvokeAsync_CallsCheckExistence_WithOnlyOrgId()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = organizationId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _orgRolePermissionServiceMock
        .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
        .ReturnsAsync(true);

    var nextCalled = false;
    RequestDelegate next = ctx =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    };

    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.True(nextCalled);
    _organizationServiceMock.Verify(
        x => x.CheckExistence(null, organizationId1),
        Times.Once);
}

[Fact]
public async Task InvokeAsync_CallsCheckExistence_WithOnlyProjectId()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "project");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["projectId"] = projectId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _projectRolePermissionServiceMock
        .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
        .ReturnsAsync(true);

    var nextCalled = false;
    RequestDelegate next = ctx =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    };

    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.True(nextCalled);
    _organizationServiceMock.Verify(
        x => x.CheckExistence(projectId1, null),
        Times.Once);
}

[Fact]
public async Task InvokeAsync_DoesNotCallCheckExistence_ForSysAdmin()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId2); // sysadmin
    context.Request.RouteValues["organizationId"] = organizationId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId2))
        .ReturnsAsync(true);

    var nextCalled = false;
    RequestDelegate next = ctx =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    };

    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.True(nextCalled);
    _organizationServiceMock.Verify(
        x => x.CheckExistence(It.IsAny<long?>(), It.IsAny<long?>()),
        Times.Never);
}

#endregion

#region Middleware Tests - Negative Verification (Methods NOT Called)

[Fact]
public async Task InvokeAsync_DoesNotCallOrgPermission_WhenOnlyProjectIdPresent()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "project");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["projectId"] = projectId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _projectRolePermissionServiceMock
        .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
        .ReturnsAsync(true);

    var nextCalled = false;
    RequestDelegate next = ctx =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    };

    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.True(nextCalled);
    _orgRolePermissionServiceMock.Verify(
        x => x.PermissionInOrg(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
}

[Fact]
public async Task InvokeAsync_DoesNotCallProjectPermission_WhenOnlyOrgIdPresent()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = organizationId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _orgRolePermissionServiceMock
        .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
        .ReturnsAsync(true);

    var nextCalled = false;
    RequestDelegate next = ctx =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    };

    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.True(nextCalled);
    _projectRolePermissionServiceMock.Verify(
        x => x.PermissionInProject(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
}

#endregion

#region Middleware Tests - Exception Handling

[Fact]
public async Task InvokeAsync_ThrowsException_WhenSysAdminCheckFails()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = organizationId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ThrowsAsync(new Exception("Database error"));

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(async () =>
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object));
}

[Fact]
public async Task InvokeAsync_ThrowsException_WhenCheckExistenceFails()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = organizationId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _organizationServiceMock
        .Setup(x => x.CheckExistence(It.IsAny<long?>(), It.IsAny<long?>()))
        .ThrowsAsync(new Exception("Organization not found"));

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(async () =>
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object));
}

[Fact]
public async Task InvokeAsync_ThrowsException_WhenOrgPermissionCheckFails()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = organizationId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _orgRolePermissionServiceMock
        .Setup(x => x.PermissionInOrg(userId1, organizationId1, "read", "organization"))
        .ThrowsAsync(new Exception("Permission check failed"));

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(async () =>
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object));
}

[Fact]
public async Task InvokeAsync_ThrowsException_WhenProjectPermissionCheckFails()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "project");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["projectId"] = projectId1.ToString();

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _projectRolePermissionServiceMock
        .Setup(x => x.PermissionInProject(userId1, projectId1, "read", "project"))
        .ThrowsAsync(new Exception("Permission check failed"));

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(async () =>
        await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
            _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object));
}

#endregion

#region Middleware Tests - Empty/Null String Route Values

[Fact]
public async Task InvokeAsync_Returns400_WhenOrgIdIsEmptyString()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = "";

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
}

[Fact]
public async Task InvokeAsync_Returns400_WhenProjectIdIsEmptyString()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "project");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["projectId"] = "";

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
}

[Fact]
public async Task InvokeAsync_Returns400_WhenOrgIdIsWhitespace()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = "   ";

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
}

[Fact]
public async Task InvokeAsync_Returns400_WhenProjectIdIsWhitespace()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "project");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["projectId"] = "   ";

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
}

[Fact]
public async Task InvokeAsync_Returns400_WhenQueryOrgIdIsEmptyString()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.QueryString = new QueryString("?organizationId=");

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
}

[Fact]
public async Task InvokeAsync_Returns400_WhenQueryProjectIdIsEmptyString()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "project");
    SetAuthenticatedUser(context, userId1);
    context.Request.QueryString = new QueryString("?projectId=");

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
}

[Fact]
public async Task InvokeAsync_Returns400_WhenOrgIdIsNonNumeric()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = "abc123";

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
}

[Fact]
public async Task InvokeAsync_Returns400_WhenProjectIdIsNonNumeric()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "project");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["projectId"] = "xyz789";

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
}

#endregion

#region Middleware Tests - Zero/Negative ID Values

[Fact]
public async Task InvokeAsync_HandlesZeroOrgId_AsValidButChecksFail()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "organization");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["organizationId"] = "0";

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _orgRolePermissionServiceMock
        .Setup(x => x.PermissionInOrg(userId1, 0, "read", "organization"))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    // Should attempt to check permissions with 0, which will fail
    Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
}

[Fact]
public async Task InvokeAsync_HandlesNegativeProjectId_AsValidButChecksFail()
{
    // Arrange
    var context = CreateHttpContextWithAuth("read", "project");
    SetAuthenticatedUser(context, userId1);
    context.Request.RouteValues["projectId"] = "-1";

    _sysAdminServiceMock
        .Setup(x => x.SysAdminCheck(userId1))
        .ReturnsAsync(false);
    _projectRolePermissionServiceMock
        .Setup(x => x.PermissionInProject(userId1, -1, "read", "project"))
        .ReturnsAsync(false);

    RequestDelegate next = ctx => Task.CompletedTask;
    var middleware = new AuthMiddleware(next);

    // Act
    await middleware.InvokeAsync(context, _orgRolePermissionServiceMock.Object,
        _projectRolePermissionServiceMock.Object, _sysAdminServiceMock.Object, _organizationServiceMock.Object);

    // Assert
    Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
}

#endregion
}