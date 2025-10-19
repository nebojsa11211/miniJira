using FluentAssertions;
using TaskEntity = MiniJira.Models.Task;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services;

namespace MiniJira.Tests.Unit.Services;

public class TaskServiceTests_GetById : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskService _service;

    public TaskServiceTests_GetById()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TaskService(_context, NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskByIdAsync_ShouldReturnTask_WhenExists()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Find Me",
            Description = "Test",
            Priority = TaskPriority.Low,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTaskByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Find Me");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetTaskByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
