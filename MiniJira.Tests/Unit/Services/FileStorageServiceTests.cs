using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services;
using NSubstitute;
using Task = MiniJira.Models.Task;

namespace MiniJira.Tests.Unit.Services;

public class FileStorageServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IFileStorageService _service;
    private readonly string _testStoragePath;

    public FileStorageServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Create a temporary directory for test files
        _testStoragePath = Path.Combine(Path.GetTempPath(), $"minijira_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testStoragePath);

        _configuration = Substitute.For<IConfiguration>();
        _configuration["FileStorage:Path"].Returns(_testStoragePath);

        _service = new FileStorageService(_context, _configuration);

        SeedTestData().Wait();
    }

    private async System.Threading.Tasks.Task SeedTestData()
    {
        var column = new Column
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Order = 0,
            Color = "#FF0000",
            IsSystem = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        column.Translations.Add(new ColumnTranslation
        {
            Id = Guid.NewGuid(),
            ColumnId = column.Id,
            Culture = "en-US",
            Name = "To Do",
            Description = "Tasks to be started"
        });

        _context.Columns.Add(column);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async System.Threading.Tasks.Task SaveFileAsync_ShouldSaveFileSuccessfully()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var columnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var fileName = "test.txt";
        var content = "Test file content"u8.ToArray();
        using var fileStream = new MemoryStream(content);

        // Act
        var attachment = await _service.SaveFileAsync(taskId, columnId, fileStream, fileName, "text/plain", "testuser");

        // Assert
        attachment.Should().NotBeNull();
        attachment.FileName.Should().Be(fileName);
        attachment.TaskId.Should().Be(taskId);
        attachment.ColumnId.Should().Be(columnId);
        attachment.ContentType.Should().Be("text/plain");
        attachment.UploadedBy.Should().Be("testuser");
        attachment.FileSizeBytes.Should().Be(content.Length);

        // Verify file exists on disk
        var filePath = Path.Combine(_testStoragePath, attachment.StoredFileName);
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task SaveFileAsync_ShouldThrowException_WhenFileSizeExceedsLimit()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var columnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var fileName = "large_file.txt";

        // Create a stream that reports size > 10MB
        var mockStream = Substitute.For<Stream>();
        mockStream.Length.Returns(11 * 1024 * 1024); // 11MB

        // Act
        var act = async () => await _service.SaveFileAsync(taskId, columnId, mockStream, fileName);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*exceeds maximum allowed size*");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetFileAsync_ShouldReturnFile_WhenAttachmentExists()
    {
        // Arrange - First save a file
        var taskId = Guid.NewGuid();
        var columnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var fileName = "test.txt";
        var content = "Test file content"u8.ToArray();
        using var saveStream = new MemoryStream(content);
        var savedAttachment = await _service.SaveFileAsync(taskId, columnId, saveStream, fileName);

        // Act
        var (fileStream, returnedFileName, contentType) = await _service.GetFileAsync(savedAttachment.Id);

        // Assert
        using (fileStream)
        {
            returnedFileName.Should().Be(fileName);
            using var reader = new StreamReader(fileStream);
            var fileContent = await reader.ReadToEndAsync();
            fileContent.Should().Be("Test file content");
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task GetFileAsync_ShouldThrowException_WhenAttachmentNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await _service.GetFileAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("Attachment not found");
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteFileAsync_ShouldDeleteFile_WhenAttachmentExists()
    {
        // Arrange - First save a file
        var taskId = Guid.NewGuid();
        var columnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var fileName = "test.txt";
        var content = "Test file content"u8.ToArray();
        using var saveStream = new MemoryStream(content);
        var savedAttachment = await _service.SaveFileAsync(taskId, columnId, saveStream, fileName);
        var filePath = Path.Combine(_testStoragePath, savedAttachment.StoredFileName);

        // Act
        var result = await _service.DeleteFileAsync(savedAttachment.Id);

        // Assert
        result.Should().BeTrue();
        File.Exists(filePath).Should().BeFalse();

        var deletedAttachment = await _context.TaskAttachments.FindAsync(savedAttachment.Id);
        deletedAttachment.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteFileAsync_ShouldReturnFalse_WhenAttachmentNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.DeleteFileAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskAttachmentsAsync_ShouldReturnAllAttachmentsForTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var columnId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Save multiple files
        using var stream1 = new MemoryStream("Content 1"u8.ToArray());
        using var stream2 = new MemoryStream("Content 2"u8.ToArray());
        await _service.SaveFileAsync(taskId, columnId, stream1, "file1.txt");
        await _service.SaveFileAsync(taskId, columnId, stream2, "file2.txt");

        // Act
        var attachments = await _service.GetTaskAttachmentsAsync(taskId);

        // Assert
        attachments.Should().HaveCount(2);
        attachments.Should().OnlyContain(a => a.TaskId == taskId);
        attachments.Should().Contain(a => a.FileName == "file1.txt");
        attachments.Should().Contain(a => a.FileName == "file2.txt");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskAttachmentsAsync_ShouldReturnEmptyList_WhenNoAttachments()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        var attachments = await _service.GetTaskAttachmentsAsync(taskId);

        // Assert
        attachments.Should().BeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAttachmentsByColumnAsync_ShouldReturnAttachmentsForSpecificColumn()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var columnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var otherColumnId = Guid.NewGuid();

        // Create another column
        var otherColumn = new Column
        {
            Id = otherColumnId,
            Order = 1,
            Color = "#00FF00",
            IsSystem = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        otherColumn.Translations.Add(new ColumnTranslation
        {
            Id = Guid.NewGuid(),
            ColumnId = otherColumn.Id,
            Culture = "en-US",
            Name = "In Progress",
            Description = "Working on it"
        });
        _context.Columns.Add(otherColumn);
        await _context.SaveChangesAsync();

        // Save files to different columns
        using var stream1 = new MemoryStream("Content 1"u8.ToArray());
        using var stream2 = new MemoryStream("Content 2"u8.ToArray());
        await _service.SaveFileAsync(taskId, columnId, stream1, "file1.txt");
        await _service.SaveFileAsync(taskId, otherColumnId, stream2, "file2.txt");

        // Act
        var attachments = await _service.GetAttachmentsByColumnAsync(taskId, columnId);

        // Assert
        attachments.Should().HaveCount(1);
        attachments[0].FileName.Should().Be("file1.txt");
        attachments[0].ColumnId.Should().Be(columnId);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAttachmentsByColumnAsync_ShouldReturnEmptyList_WhenNoAttachmentsInColumn()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var columnId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var attachments = await _service.GetAttachmentsByColumnAsync(taskId, columnId);

        // Assert
        attachments.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();

        // Clean up test storage directory
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, true);
        }
    }
}
