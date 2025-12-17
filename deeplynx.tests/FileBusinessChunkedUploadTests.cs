using System.Text;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class FileBusinessChunkedUploadTests : IntegrationTestBase
{
    private readonly Mock<IFileBusiness> _innerFileBusiness = null!;
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), "FileBusinessChunkedTests");
    private ClassBusiness _classBusiness = null!;
    private DataSourceBusiness _dataSourceBusiness = null!;
    private Mock<IEdgeBusiness> _edgeBusiness = null!;
    private EventBusiness _eventBusiness = null!;
    private FileBusiness _fileBusiness = null!;
    private Mock<IFileBusinessFactory> _fileBusinessFactory = null!;
    private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
    private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
    private INotificationBusiness _notificationBusiness = null!;
    private ObjectStorageBusiness _objectStorageBusiness = null!;
    private RecordBusiness _recordBusiness = null!;
    private Mock<IRelationshipBusiness> _relationshipBusiness = null!;
    private TagBusiness _tagBusiness = null!;

    public long did; // datasource ID
    public long oid; // organization ID
    public long osid; // object storage ID
    public long pid; // project ID
    public long uid; // user ID

    public FileBusinessChunkedUploadTests(TestSuiteFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Directory.CreateDirectory(_testDirectory);

        _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
        _edgeBusiness = new Mock<IEdgeBusiness>();
        _relationshipBusiness = new Mock<IRelationshipBusiness>();
        _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
        _notificationBusiness =
            new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
        _eventBusiness = new EventBusiness(Context, _notificationBusiness);


        _fileBusinessFactory = new Mock<IFileBusinessFactory>();

        _dataSourceBusiness =
            new DataSourceBusiness(Context, _edgeBusiness.Object, _recordBusiness, _eventBusiness);
        _objectStorageBusiness = new ObjectStorageBusiness(Context);

        _tagBusiness = new TagBusiness(Context, _eventBusiness);
        _recordBusiness = new RecordBusiness(Context, _eventBusiness, _tagBusiness);
        _classBusiness = new ClassBusiness(Context, _recordBusiness, _relationshipBusiness.Object, _eventBusiness);

        var realFileFilesystemBusiness =
            new FileFilesystemBusiness(Context, _objectStorageBusiness, _classBusiness, _recordBusiness);

        // Object storage should also determine this implicitly - but we can also add this failsafe for now
        _fileBusinessFactory
            .Setup(x => x.CreateFileBusiness("filesystem"))
            .Returns(realFileFilesystemBusiness);

        _fileBusiness = new FileBusiness(
            Context,
            _fileBusinessFactory.Object,
            _objectStorageBusiness,
            _dataSourceBusiness,
            _classBusiness,
            _recordBusiness
        );
    }

    #region StartUpload Tests

    [Fact]
    public async Task StartUpload_CreatesUploadDirectory_AndReturnsSessionInfo()
    {
        // Arrange
        var request = new FileUploadInitRequestDto
        {
            FileName = "bigfile.bin",
            FileSize = 2L * 1024 * 1024 * 1024 // 2GB
        };

        // Act
        var session = await _fileBusiness.StartUpload(
            uid,
            oid,
            pid,
            did,
            osid,
            request
        );

        // Assert
        Assert.NotNull(session);
        Assert.False(string.IsNullOrWhiteSpace(session.UploadId));
        Assert.Equal(400_000_000, session.ChunkSize);
        Assert.Equal(6, session.TotalChunks); // 2GB / 400MB = 6 chunks

        var uploadPath = Path.Combine(
            _testDirectory,
            $"project_{pid}",
            $"datasource_{did}",
            "uploads",
            session.UploadId
        );

        Assert.True(Directory.Exists(uploadPath));
    }

    #endregion

    #region UploadChunk Tests

    [Fact]
    public async Task UploadChunk_WritesChunkFile_WhenSessionExists()
    {
        // Arrange: create an upload session first
        var initRequest = new FileUploadInitRequestDto
        {
            FileName = "file.txt",
            FileSize = 1024
        };

        var session = await _fileBusiness.StartUpload(uid, oid, pid, did, osid, initRequest);

        var content = "CHUNK-0-CONTENT";
        var formFile = CreateFormFile(content);

        var expectedChunkPath = Path.Combine(
            _testDirectory,
            $"project_{pid}",
            $"datasource_{did}",
            "uploads",
            session.UploadId,
            "0.part"
        );

        // Act
        var result = await _fileBusiness.UploadChunk(
            uid,
            oid,
            pid,
            did,
            osid,
            formFile,
            session.UploadId,
            0
        );

        // Assert
        Assert.Equal("success", result);
        Assert.True(File.Exists(expectedChunkPath));

        var saved = await File.ReadAllTextAsync(expectedChunkPath);
        Assert.Equal(content, saved);
    }

    #endregion

    private IFormFile CreateFormFile(string content)
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return new FormFile(ms, 0, ms.Length, "chunk", "chunk.part")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };
    }

    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();

        var user = new User
        {
            Name = "Test User",
            Email = "test_chunked@example.com",
            Password = "test_password",
            IsArchived = false
        };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        uid = user.Id;

        var organization = new Organization { Name = "Test Organization" };
        Context.Organizations.Add(organization);
        await Context.SaveChangesAsync();
        oid = organization.Id;

        var project = new Project { Name = "Test Project", OrganizationId = oid };
        Context.Projects.Add(project);
        await Context.SaveChangesAsync();
        pid = project.Id;

        var dataSource = new DataSource
        {
            Name = "Test Data Source",
            Description = "Test data source for chunked upload tests",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = uid,
            OrganizationId = oid,
            Default = true
        };
        Context.DataSources.Add(dataSource);
        await Context.SaveChangesAsync();
        did = dataSource.Id;

        var osConfig = new JsonObject
        {
            ["mountPath"] = _testDirectory
        };

        var objectStorage = new ObjectStorage
        {
            Name = "Test Object Storage",
            ProjectId = pid,
            OrganizationId = oid,
            Type = "filesystem",
            Config = osConfig.ToString(),
            Default = true
        };

        Context.ObjectStorages.Add(objectStorage);
        await Context.SaveChangesAsync();
        osid = objectStorage.Id;

        var testClass = new Class
        {
            Name = "File",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            OrganizationId = oid
        };
        Context.Classes.Add(testClass);
        await Context.SaveChangesAsync();
    }

    #region CompleteUpload Tests

    [Fact]
    public async Task CompleteUpload_MergesChunks_AndCreatesRecord()
    {
        // Arrange: create an upload session
        var fileName = "final.txt";
        var initRequest = new FileUploadInitRequestDto
        {
            FileName = fileName,
            FileSize = 2048
        };

        var session = await _fileBusiness.StartUpload(uid, oid, pid, did, osid, initRequest);

        // Upload two chunks
        var chunk0 = CreateFormFile("first-");
        await _fileBusiness.UploadChunk(uid, oid, pid, did, osid, chunk0, session.UploadId, 0);

        var chunk1 = CreateFormFile("second");
        await _fileBusiness.UploadChunk(uid, oid, pid, did, osid, chunk1, session.UploadId, 1);

        var completeRequest = new FileUploadCompleteRequestDto
        {
            UploadId = session.UploadId,
            FileName = fileName,
            TotalChunks = 2
        };

        var uploadPath = Path.Combine(
            _testDirectory,
            $"project_{pid}",
            $"datasource_{did}",
            "uploads",
            session.UploadId
        );

        // Act
        var result = await _fileBusiness.CompleteUpload(uid, oid, pid, did, osid, completeRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileName, result.Name);
        Assert.NotNull(result.Uri);
        Assert.Equal(osid, result.ObjectStorageId);

        // Upload directory should be cleaned up
        Assert.False(Directory.Exists(uploadPath));

        // Verify final file exists in object storage location
        var finalFilePath = result.Uri; // This is the path returned by FileFilesystemBusiness
        Assert.True(File.Exists(finalFilePath));

        // Verify merged content
        var mergedContent = await File.ReadAllTextAsync(finalFilePath);
        Assert.Equal("first-second", mergedContent);
    }

    [Fact]
    public async Task CompleteUpload_MissingChunk_ThrowsException()
    {
        // Arrange
        var session = await _fileBusiness.StartUpload(
            uid, oid, pid, did, osid,
            new FileUploadInitRequestDto { FileName = "test.txt", FileSize = 2048 }
        );

        // Upload only chunk 0, skip chunk 1
        await _fileBusiness.UploadChunk(
            uid, oid, pid, did, osid,
            CreateFormFile("chunk0"),
            session.UploadId,
            0
        );

        var completeRequest = new FileUploadCompleteRequestDto
        {
            UploadId = session.UploadId,
            FileName = "test.txt",
            TotalChunks = 2
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fileBusiness.CompleteUpload(uid, oid, pid, did, osid, completeRequest)
        );

        // Verify cleanup happened
        var uploadPath = Path.Combine(
            _testDirectory,
            $"project_{pid}",
            $"datasource_{did}",
            "uploads",
            session.UploadId
        );
        Assert.False(Directory.Exists(uploadPath));
    }

    #endregion


    // public override Task DisposeAsync()
    // {
    //     if (Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory, true);
    //
    //     return base.DisposeAsync();
    // }
}