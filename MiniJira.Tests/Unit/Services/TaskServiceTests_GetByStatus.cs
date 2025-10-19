using FluentAssertions;
using TaskEntity = MiniJira.Models.Task;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services;

namespace MiniJira.Tests.Unit.Services;

public class TaskServiceTests_GetByStatus : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskService _service;

    public TaskServiceTests_GetByStatus()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TaskService(_context, NullLogger<TaskService>.Instance);
    }

    [Theory]
    [InlineData(Models.TaskStatus.ToDo)]
    [InlineData(Models.TaskStatus.InProgress)]
    [InlineData(Models.TaskStatus.Done)]
    public async System.Threading.Tasks.Task GetTasksByStatusAsync_ShouldFilterByStatus(Models.TaskStatus status)
    {
        // Arrange
        _context.Tasks.AddRange(
            new TaskEntity { Id = Guid.NewGuid(), Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.ToDo, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TaskEntity { Id = Guid.NewGuid(), Title = "Task 2", Description = "Desc 2", Status = Models.TaskStatus.InProgress, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TaskEntity { Id = Guid.NewGuid(), Title = "Task 3", Description = "Desc 3", Status = Models.TaskStatus.Done, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTasksByStatusAsync(status);

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(status);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
