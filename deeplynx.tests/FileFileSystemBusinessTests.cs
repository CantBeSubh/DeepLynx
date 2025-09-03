using System.Text;
using deeplynx.business;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.Extensions.Logging;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class FileFileSystemBusinessTests: IntegrationTestBase
{
    private ClassBusiness _classBusiness = null!;
    private ProjectBusiness _projectBusiness = null!;
    private EventBusiness _eventBusiness = null!;
    private FileFilesystemBusiness _fileBusiness;
    private Mock<IDataSourceBusiness> _dataSourceBusiness = null!;
    private Mock<IEdgeMappingBusiness> _edgeMappingBusiness = null!;
    private Mock<IRecordBusiness> _recordBusiness = null!;
    private Mock<IRecordMappingBusiness> _recordMappingBusiness = null!;
    private Mock<IRelationshipBusiness> _relationshipBusiness = null!;
    private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
    private Mock<IObjectStorageBusiness> _objectStorageBusiness = null!;
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), "FileBusinessTests");
    
    
    public FileFileSystemBusinessTests(TestSuiteFixture fixture) : base(fixture) {}

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _edgeMappingBusiness = new Mock<IEdgeMappingBusiness>();
        _recordBusiness = new Mock<IRecordBusiness>();
        _recordMappingBusiness = new Mock<IRecordMappingBusiness>();
        _relationshipBusiness = new Mock<IRelationshipBusiness>();
        _dataSourceBusiness = new Mock<IDataSourceBusiness>();
        _mockLogger = new Mock<ILogger<ProjectBusiness>>();
        _eventBusiness = new EventBusiness(Context);
        _objectStorageBusiness = new Mock<IObjectStorageBusiness>();

        _classBusiness = new ClassBusiness(
            Context, _edgeMappingBusiness.Object, _recordBusiness.Object, 
            _recordMappingBusiness.Object, _relationshipBusiness.Object, _eventBusiness);
            
        _projectBusiness = new ProjectBusiness(
            Context, _mockLogger.Object, _classBusiness, 
            _dataSourceBusiness.Object, _objectStorageBusiness.Object, _eventBusiness);
        
        _fileBusiness = new FileFilesystemBusiness(Context, _objectStorageBusiness.Object, _classBusiness, _recordBusiness.Object);
    }
    
    
    
    
    [Fact]
    public async Task UploadFile_ShouldSaveFileAndReturnPath() {
        // Arrange
        var config = new ObjectStorageConfigDto { MountPath = _testDirectory };
        var fileMock = new Mock<IFormFile>();
        var content = "Test file content";
        var fileName = "test.txt";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream));
    
        var guid = Guid.NewGuid();
        
        // need the try finally for if the test fails, still want to do cleanup
        try
        {
            // Act
            var result = await _fileBusiness.UploadFile(1, 1, config, fileMock.Object, guid);

            // Assert
            Assert.Contains(guid.ToString(), result);
            Assert.True(File.Exists(result));
            Assert.True(Directory.Exists(_testDirectory));
        }
        finally
        {
            // Simple cleanup - delete the entire test directory
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            Assert.False(Directory.Exists(_testDirectory));
        }
    }
    
    
    [Fact]
    public async Task UpdateFile_ShouldReplaceExistingFile()
    {
        // Arrange
        Directory.CreateDirectory(_testDirectory);
        var originalFilePath = Path.Combine(_testDirectory, "original.txt");
        await File.WriteAllTextAsync(originalFilePath, "Old content");

        var record = new RecordResponseDto
        {
            Uri = originalFilePath,
            OriginalId = Guid.NewGuid().ToString()
        };

        var newContent = "New content";
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(newContent));
        fileMock.Setup(f => f.FileName).Returns("new.txt");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Returns((Stream stream, System.Threading.CancellationToken token) => ms.CopyToAsync(stream));

        try
        {
            // Act
            var updatedPath = await _fileBusiness.UpdateFile(record, fileMock.Object);

            // Assert
            Assert.True(File.Exists(updatedPath));
            var updatedContent = await File.ReadAllTextAsync(updatedPath);
            Assert.Equal(newContent, updatedContent);
        }
        finally
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
    }

    
    [Fact]
    public async Task DownloadFile_ShouldReturnFileStreamResult()
    {
        // Arrange
        Directory.CreateDirectory(_testDirectory);
        var filePath = Path.Combine(_testDirectory, "download.txt");
        var content = "Downloadable content";
        await File.WriteAllTextAsync(filePath, content);

        var record = new RecordResponseDto
        {
            Uri = filePath,
            Name = "download.txt"
        };

        try
        {
            // Act
            var result = await _fileBusiness.DownloadFile(record);

            // Assert
            Assert.NotNull(result);
            using var reader = new StreamReader(result.FileStream);
            var resultContent = await reader.ReadToEndAsync();
            Assert.Equal(content, resultContent);
        }
        finally
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
    }
    
    [Fact]
    public async Task DeleteFile_ShouldDeleteFile() {
        // Arrange
        var config = new ObjectStorageConfigDto { MountPath = _testDirectory };
        var fileMock = new Mock<IFormFile>();
        var content = "Test file content";
        var fileName = "test.txt";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream));
    
        var guid = Guid.NewGuid();
        
        // need the try finally for if the test fails, still want to do cleanup
        try
        {
            // Act
            var result = await _fileBusiness.UploadFile(1, 1, config, fileMock.Object, guid);

            // Assert
            Assert.Contains(guid.ToString(), result);
            Assert.True(File.Exists(result));
            Assert.True(Directory.Exists(_testDirectory));
        }
        finally
        {
            // Simple cleanup - delete the entire test directory
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            Assert.False(Directory.Exists(_testDirectory));
        }
    }

}