using FluentAssertions;
using TaskEntity = MiniJira.Models.Task;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services;
using MiniJira.Services.DTOs;

namespace MiniJira.Tests.Unit.Services;

public class TaskServiceTests_Update : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskService _service;

    public TaskServiceTests_Update()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TaskService(_context, NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskAsync_ShouldUpdateTask_WhenTaskExists()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Original",
            Description = "Original Desc",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Low,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated",
            Description = "Updated Desc",
            Status = Models.TaskStatus.InProgress,
            Priority = TaskPriority.High
        };

        // Act
        var result = await _service.UpdateTaskAsync(task.Id, updateRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated");
        result.Value.Description.Should().Be("Updated Desc");
        result.Value.Status.Should().Be(Models.TaskStatus.InProgress);
        result.Value.Priority.Should().Be(TaskPriority.High);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskAsync_ShouldFail_WhenTaskNotFound()
    {
        // Arrange
        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated",
            Description = "Updated Desc",
            Status = Models.TaskStatus.InProgress,
            Priority = TaskPriority.High
        };

        // Act
        var result = await _service.UpdateTaskAsync(Guid.NewGuid(), updateRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskAsync_ShouldFail_WithInvalidData()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Original",
            Description = "Original Desc",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateTaskRequest
        {
            Title = new string('a', 201), // Exceeds limit
            Description = "Valid",
            Status = Models.TaskStatus.InProgress,
            Priority = TaskPriority.High
        };

        // Act
        var result = await _service.UpdateTaskAsync(task.Id, updateRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
