using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services;
using MiniJira.Services.DTOs;
using TaskEntity = MiniJira.Models.Task;

namespace MiniJira.Tests.Unit.Services;

public class TaskServiceTests_GetAll : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;
    private readonly ITaskService _service;

    public TaskServiceTests_GetAll()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userService = Substitute.For<IUserService>();
        _service = new TaskService(_context, NullLogger<TaskService>.Instance, _userService);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllTasksAsync_ShouldReturnEmptyList_WhenNoTasksExist()
    {
        // Act
        var result = await _service.GetAllTasksAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllTasksAsync_ShouldReturnTasks_OrderedByCreatedDateDesc()
    {
        // Arrange
        var task1 = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "First",
            Description = "First task",
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        await System.Threading.Tasks.Task.Delay(10); // Ensure different timestamps

        var task2 = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Second",
            Description = "Second task",
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllTasksAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Second"); // Newest first
        result[1].Title.Should().Be("First");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
