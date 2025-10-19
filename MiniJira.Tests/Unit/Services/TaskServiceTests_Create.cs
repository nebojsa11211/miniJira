using FluentAssertions;
using TaskEntity = MiniJira.Models.Task;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services;
using MiniJira.Services.DTOs;

namespace MiniJira.Tests.Unit.Services;

public class TaskServiceTests_Create : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskService _service;

    public TaskServiceTests_Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TaskService(_context, NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ShouldCreateTask_WithValidData()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High
        };

        // Act
        var result = await _service.CreateTaskAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Test Task");
        result.Value.Description.Should().Be("Test Description");
        result.Value.Status.Should().Be(Models.TaskStatus.ToDo); // Default
        result.Value.Priority.Should().Be(TaskPriority.High);
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = new string('a', 201), // Exceeds 200 char limit
            Description = "Valid description",
            Priority = TaskPriority.Medium
        };

        // Act
        var result = await _service.CreateTaskAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Title");
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "",
            Description = "Valid description",
            Priority = TaskPriority.Medium
        };

        // Act
        var result = await _service.CreateTaskAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Title is required");
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ShouldFail_WhenDescriptionIsEmpty()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Valid title",
            Description = "",
            Priority = TaskPriority.Medium
        };

        // Act
        var result = await _service.CreateTaskAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Description");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
