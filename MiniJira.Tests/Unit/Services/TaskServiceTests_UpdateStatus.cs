using FluentAssertions;
using TaskEntity = MiniJira.Models.Task;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services;

namespace MiniJira.Tests.Unit.Services;

public class TaskServiceTests_UpdateStatus : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskService _service;

    public TaskServiceTests_UpdateStatus()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TaskService(_context, NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskStatusAsync_ShouldUpdateStatus_WhenTaskExists()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Priority = TaskPriority.Medium,
            Status = Models.TaskStatus.ToDo,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UpdateTaskStatusAsync(task.Id, Models.TaskStatus.InProgress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(Models.TaskStatus.InProgress);
        result.Value.UpdatedAt.Should().BeAfter(task.CreatedAt);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskStatusAsync_ShouldFail_WhenTaskNotFound()
    {
        // Act
        var result = await _service.UpdateTaskStatusAsync(Guid.NewGuid(), Models.TaskStatus.Done);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
