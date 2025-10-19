using FluentAssertions;
using TaskEntity = MiniJira.Models.Task;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services;

namespace MiniJira.Tests.Unit.Services;

public class TaskServiceTests_Delete : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskService _service;

    public TaskServiceTests_Delete()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TaskService(_context, NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTaskAsync_ShouldDeleteTask_WhenTaskExists()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "To Delete",
            Description = "Will be deleted",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteTaskAsync(task.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTaskAsync_ShouldFail_WhenTaskNotFound()
    {
        // Act
        var result = await _service.DeleteTaskAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
