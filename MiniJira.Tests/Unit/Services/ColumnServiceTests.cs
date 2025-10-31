using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services;
using MiniJira.Services.DTOs;
using NSubstitute;
using System.Globalization;
using Task = MiniJira.Models.Task;

namespace MiniJira.Tests.Unit.Services;

public class ColumnServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;
    private readonly IColumnService _service;

    public ColumnServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _localizationService = Substitute.For<ILocalizationService>();

        // Setup default culture
        _localizationService.CurrentCulture.Returns(new CultureInfo("en-US"));
        _localizationService.SupportedCultures.Returns(new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("hr-HR")
        });

        _service = new ColumnService(_context, _localizationService, NullLogger<ColumnService>.Instance);

        SeedTestData().Wait();
    }

    private async System.Threading.Tasks.Task SeedTestData()
    {
        var todoColumn = new Column
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Order = 0,
            Color = "#FF0000",
            IsSystem = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        todoColumn.Translations.Add(new ColumnTranslation
        {
            Id = Guid.NewGuid(),
            ColumnId = todoColumn.Id,
            Culture = "en-US",
            Name = "To Do",
            Description = "Tasks to be started"
        });

        todoColumn.Translations.Add(new ColumnTranslation
        {
            Id = Guid.NewGuid(),
            ColumnId = todoColumn.Id,
            Culture = "hr-HR",
            Name = "Za napraviti",
            Description = "Zadaci koje treba započeti"
        });

        var inProgressColumn = new Column
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Order = 1,
            Color = "#00FF00",
            IsSystem = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        inProgressColumn.Translations.Add(new ColumnTranslation
        {
            Id = Guid.NewGuid(),
            ColumnId = inProgressColumn.Id,
            Culture = "en-US",
            Name = "In Progress",
            Description = "Tasks being worked on"
        });

        inProgressColumn.Translations.Add(new ColumnTranslation
        {
            Id = Guid.NewGuid(),
            ColumnId = inProgressColumn.Id,
            Culture = "hr-HR",
            Name = "U tijeku",
            Description = "Zadaci na kojima se radi"
        });

        _context.Columns.AddRange(todoColumn, inProgressColumn);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllColumnsAsync_ShouldReturnAllActiveColumns_WithCurrentCulture()
    {
        // Act
        var result = await _service.GetAllColumnsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(c => c.Order);
        result[0].Name.Should().Be("To Do");
        result[1].Name.Should().Be("In Progress");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllColumnsAsync_ShouldReturnColumnsWithSpecifiedCulture()
    {
        // Act
        var result = await _service.GetAllColumnsAsync("hr-HR");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Za napraviti");
        result[1].Name.Should().Be("U tijeku");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetColumnByIdAsync_ShouldReturnColumn_WhenExists()
    {
        // Arrange
        var columnId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetColumnByIdAsync(columnId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(columnId);
        result.Name.Should().Be("To Do");
        result.Color.Should().Be("#FF0000");
        result.IsSystem.Should().BeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetColumnByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.GetColumnByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateColumnAsync_ShouldCreateColumn_WithValidData()
    {
        // Arrange
        var request = new CreateColumnRequest
        {
            Color = "#0000FF",
            Translations = new Dictionary<string, ColumnTranslationDto>
            {
                { "en-US", new ColumnTranslationDto { Name = "Review", Description = "Under review" } },
                { "hr-HR", new ColumnTranslationDto { Name = "Pregled", Description = "Na pregledu" } }
            }
        };

        // Act
        var result = await _service.CreateColumnAsync(request, "en-US");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Review");
        result.Value.Color.Should().Be("#0000FF");
        result.Value.IsSystem.Should().BeFalse();
        result.Value.Translations.Should().HaveCount(2);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateColumnAsync_ShouldFail_WhenMissingTranslations()
    {
        // Arrange
        var request = new CreateColumnRequest
        {
            Color = "#0000FF",
            Translations = new Dictionary<string, ColumnTranslationDto>
            {
                { "en-US", new ColumnTranslationDto { Name = "Review", Description = "Under review" } }
                // Missing hr-HR translation
            }
        };

        // Act
        var result = await _service.CreateColumnAsync(request, "en-US");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Translations missing");
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateColumnAsync_ShouldFail_WhenTranslationNameIsEmpty()
    {
        // Arrange
        var request = new CreateColumnRequest
        {
            Color = "#0000FF",
            Translations = new Dictionary<string, ColumnTranslationDto>
            {
                { "en-US", new ColumnTranslationDto { Name = "", Description = "Under review" } },
                { "hr-HR", new ColumnTranslationDto { Name = "Pregled", Description = "Na pregledu" } }
            }
        };

        // Act
        var result = await _service.CreateColumnAsync(request, "en-US");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Name is required");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateColumnAsync_ShouldUpdateColumn_WithValidData()
    {
        // Arrange
        var columnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var request = new UpdateColumnRequest
        {
            Color = "#FFFF00",
            Translations = new Dictionary<string, ColumnTranslationDto>
            {
                { "en-US", new ColumnTranslationDto { Name = "To Do Updated", Description = "Updated description" } }
            }
        };

        // Act
        var result = await _service.UpdateColumnAsync(columnId, request, "en-US");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Color.Should().Be("#FFFF00");
        result.Value.Name.Should().Be("To Do Updated");
        result.Value.Description.Should().Be("Updated description");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateColumnAsync_ShouldFail_WhenColumnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateColumnRequest
        {
            Color = "#FFFF00"
        };

        // Act
        var result = await _service.UpdateColumnAsync(nonExistentId, request, "en-US");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteColumnAsync_ShouldDeleteColumn_WhenNoTasksAssigned()
    {
        // Arrange - Create a custom column without tasks
        var customColumn = new Column
        {
            Id = Guid.NewGuid(),
            Order = 10,
            Color = "#ABCDEF",
            IsSystem = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        customColumn.Translations.Add(new ColumnTranslation
        {
            Id = Guid.NewGuid(),
            ColumnId = customColumn.Id,
            Culture = "en-US",
            Name = "Custom Column",
            Description = "Test column"
        });

        _context.Columns.Add(customColumn);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteColumnAsync(customColumn.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedColumn = await _context.Columns.FindAsync(customColumn.Id);
        deletedColumn.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteColumnAsync_ShouldFail_WhenColumnIsSystem()
    {
        // Arrange
        var systemColumnId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.DeleteColumnAsync(systemColumnId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Cannot delete system columns");
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteColumnAsync_ShouldFail_WhenColumnHasTasks()
    {
        // Arrange - Create a column with a task
        var customColumn = new Column
        {
            Id = Guid.NewGuid(),
            Order = 10,
            Color = "#ABCDEF",
            IsSystem = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        customColumn.Translations.Add(new ColumnTranslation
        {
            Id = Guid.NewGuid(),
            ColumnId = customColumn.Id,
            Culture = "en-US",
            Name = "Custom Column",
            Description = "Test column"
        });

        var task = new Models.Task
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.ToDo,
            Priority = TaskPriority.Low,
            ColumnId = customColumn.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Columns.Add(customColumn);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteColumnAsync(customColumn.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Cannot delete column with existing tasks");
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteColumnAsync_ShouldFail_WhenColumnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.DeleteColumnAsync(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderColumnsAsync_ShouldReorderColumns_WithValidData()
    {
        // Arrange
        var request = new ReorderColumnsRequest
        {
            ColumnIds = new List<Guid>
            {
                Guid.Parse("22222222-2222-2222-2222-222222222222"), // In Progress -> 0
                Guid.Parse("11111111-1111-1111-1111-111111111111")  // To Do -> 1
            }
        };

        // Act
        var result = await _service.ReorderColumnsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var columns = await _context.Columns.OrderBy(c => c.Order).ToListAsync();
        columns[0].Id.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        columns[1].Id.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderColumnsAsync_ShouldFail_WhenInvalidColumnIds()
    {
        // Arrange
        var request = new ReorderColumnsRequest
        {
            ColumnIds = new List<Guid>
            {
                Guid.NewGuid(), // Invalid ID
                Guid.Parse("11111111-1111-1111-1111-111111111111")
            }
        };

        // Act
        var result = await _service.ReorderColumnsAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("invalid");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
