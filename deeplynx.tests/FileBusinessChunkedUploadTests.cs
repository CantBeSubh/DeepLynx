using System.Text;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using Moq;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class FileBusinessChunkedUploadTests : IntegrationTestBase
{
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), "FileBusinessChunkedTests");
    private Mock<IClassBusiness> _classBusiness = null!;
    private Mock<IDataSourceBusiness> _dataSourceBusiness = null!;
    private FileBusiness _fileBusiness = null!;
    private Mock<IFileBusinessFactory> _fileBusinessFactory = null!;
    private Mock<IFileBusiness> _innerFileBusiness = null!;
    private Mock<IObjectStorageBusiness> _objectStorageBusiness = null!;
    private Mock<IRecordBusiness> _recordBusiness = null!;
    private long did;
    private long oid;
    private long osid;
    private long pid;
    private long uid;

    public FileBusinessChunkedUploadTests(TestSuiteFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Directory.CreateDirectory(_testDirectory);

        _dataSourceBusiness = new Mock<IDataSourceBusiness>();
        _objectStorageBusiness = new Mock<IObjectStorageBusiness>();
        _classBusiness = new Mock<IClassBusiness>();
        _recordBusiness = new Mock<IRecordBusiness>();
        _fileBusinessFactory = new Mock<IFileBusinessFactory>();
        _innerFileBusiness = new Mock<IFileBusiness>();

        await SeedTestDataAsync();

        // Default data source
        _dataSourceBusiness
            .Setup(x => x.GetDefaultDataSource(oid, pid))
            .ReturnsAsync(new DataSourceResponseDto { Id = did, ProjectId = pid });

        // Default object storage
        _objectStorageBusiness
            .Setup(x => x.GetDefaultObjectStorage(oid, pid))
            .ReturnsAsync(new ObjectStorageResponseDto { Id = osid });

        // Factory returns mocked inner file business
        _fileBusinessFactory
            .Setup(x => x.CreateFileBusiness("filesystem"))
            .Returns(_innerFileBusiness.Object);

        // Stub UploadFile to just return a URI (we’ll assert on the content via callback)
        _innerFileBusiness
            .Setup(x => x.UploadFile(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<ObjectStorageConfigDto>(),
                It.IsAny<IFormFile>(),
                It.IsAny<Guid>()))
            .ReturnsAsync("dummy-uri");

        // Construct FileBusiness – adjust ctor args to match your actual type
        _fileBusiness = new FileBusiness(
            Context,
            _fileBusinessFactory.Object,
            _objectStorageBusiness.Object,
            _dataSourceBusiness.Object,
            _classBusiness.Object,
            _recordBusiness.Object
        );
    }

    [Fact]
    public async Task StartUpload_CreatesUploadDirectory_AndReturnsSessionInfo()
    {
        // Arrange
        var request = new FileUploadInitRequestDto
        {
            FileName = "bigfile.bin",
            FileSize = 2L * 1024 * 1024 // any positive size; we won't assert exact chunk count
        };

        // Act
        var session = await _fileBusiness.StartUpload(
            uid, // currentUserId
            oid, // organizationId
            pid, // projectId
            null, // use default data source
            null, // use default object storage
            request
        );

        // Assert
        Assert.NotNull(session);
        Assert.False(string.IsNullOrWhiteSpace(session.UploadId));
        Assert.True(session.TotalChunks > 0);

        var uploadPath = Path.Combine(
            _testDirectory,
            $"org_{oid}",
            $"project_{pid}",
            $"datasource_{did}",
            "uploads",
            session.UploadId
        );

        Assert.True(Directory.Exists(uploadPath));
    }

    [Fact]
    public async Task UploadChunk_WritesChunkFile_WhenSessionExists()
    {
        // Arrange: create an upload session first
        var initRequest = new FileUploadInitRequestDto
        {
            FileName = "file.txt",
            FileSize = 1024 // one small chunk
        };

        var session = await _fileBusiness.StartUpload(
            uid,
            oid,
            pid,
            null,
            null,
            initRequest
        );

        var content = "CHUNK-0-CONTENT";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var formFile = new FormFile(ms, 0, ms.Length, "chunk", "chunk0.part")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };

        var expectedChunkPath = Path.Combine(
            _testDirectory,
            $"org_{oid}",
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
            null, // default data source
            null, // default object storage
            formFile,
            session.UploadId,
            0 // chunkNumber
        );

        // Assert
        Assert.Equal("success", result);
        Assert.True(File.Exists(expectedChunkPath));

        var saved = await File.ReadAllTextAsync(expectedChunkPath);
        Assert.Equal(content, saved);
    }

    [Fact]
    public async Task CompleteUpload_MergesChunks_PassesMergedFileToInnerBusiness_AndCleansUp()
    {
        // Arrange: create an upload session
        var fileName = "final.txt";
        var initRequest = new FileUploadInitRequestDto
        {
            FileName = fileName,
            FileSize = 2048 // assume 2 chunks in this test scenario
        };

        var session = await _fileBusiness.StartUpload(
            uid,
            oid,
            pid,
            null,
            null,
            initRequest
        );

        var uploadPath = Path.Combine(
            _testDirectory,
            $"org_{oid}",
            $"project_{pid}",
            $"datasource_{did}",
            "uploads",
            session.UploadId
        );

        // Make sure directory exists (StartUpload should have done this, but this guards refactors)
        Directory.CreateDirectory(uploadPath);

        // Two chunks we expect to be merged in order
        await File.WriteAllTextAsync(Path.Combine(uploadPath, "0.part"), "first-");
        await File.WriteAllTextAsync(Path.Combine(uploadPath, "1.part"), "second");

        var completeRequest = new FileUploadCompleteRequestDto
        {
            UploadId = session.UploadId,
            FileName = fileName,
            TotalChunks = 2
        };

        // file class
        _classBusiness
            .Setup(x => x.GetOrCreateClass(uid, oid, pid, "File"))
            .ReturnsAsync(new ClassResponseDto { Id = 10, Name = "File" });

        var expectedRecord = new RecordResponseDto
        {
            Id = 123,
            Name = fileName,
            Uri = "dummy-uri",
            ProjectId = pid,
            ObjectStorageId = osid
        };

        _recordBusiness
            .Setup(x => x.CreateRecord(
                uid,
                oid,
                pid,
                did,
                It.IsAny<CreateRecordRequestDto>()))
            .ReturnsAsync(expectedRecord);

        // Capture merged file content passed to inner UploadFile
        string? capturedMergedContent = null;

        _innerFileBusiness
            .Setup(x => x.UploadFile(
                pid,
                did,
                It.IsAny<ObjectStorageConfigDto>(),
                It.IsAny<IFormFile>(),
                It.IsAny<Guid>()))
            .Callback<long, long, ObjectStorageConfigDto, IFormFile,
                Guid>(async (projId, dsId, config, formFile, guid) =>
            {
                using var ms = new MemoryStream();
                await formFile.CopyToAsync(ms);
                capturedMergedContent = Encoding.UTF8.GetString(ms.ToArray());
            })
            .ReturnsAsync("dummy-uri");

        // Act
        var result = await _fileBusiness.CompleteUpload(
            uid,
            oid,
            pid,
            null,
            null,
            completeRequest
        );

        // Assert: record returned
        Assert.Equal(expectedRecord.Id, result.Id);
        Assert.Equal(expectedRecord.Name, result.Name);
        Assert.Equal(expectedRecord.Uri, result.Uri);

        // Assert: merged content correct and in order
        Assert.Equal("first-second", capturedMergedContent);

        // Assert: inner UploadFile called once
        _innerFileBusiness.Verify(x => x.UploadFile(
                pid,
                did,
                It.IsAny<ObjectStorageConfigDto>(),
                It.IsAny<IFormFile>(),
                It.IsAny<Guid>()),
            Times.Once);

        // Assert: upload directory cleaned up
        Assert.False(Directory.Exists(uploadPath));
    }


    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();

        var user = new User
        {
            Name = "Test User",
            Email = "test_record@example.com",
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
            Description = "Test data source for unit tests",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = uid,
            OrganizationId = oid
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
    }

    // public override Task DisposeAsync()
    // {
    //     if (Directory.Exists(_testDirectory))
    //         Directory.Delete(_testDirectory, true);
    //
    //     return base.DisposeAsync();
    // }
}