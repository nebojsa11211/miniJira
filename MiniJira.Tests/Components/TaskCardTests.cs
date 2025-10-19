using Bunit;
using FluentAssertions;
using MiniJira.Components.Shared;
using MiniJira.Models;
using MiniJira.Services.DTOs;

namespace MiniJira.Tests.Components;

public class TaskCardTests : TestContext
{
    [Fact]
    public void TaskCard_ShouldRenderTaskDetails()
    {
        // Arrange
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "This is a test description",
            Status = Models.TaskStatus.InProgress,
            Priority = TaskPriority.High,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var cut = RenderComponent<TaskCard>(parameters => parameters
            .Add(p => p.Task, task));

        // Assert
        cut.Find(".task-title").TextContent.Should().Be("Test Task");
        cut.Find(".task-description").TextContent.Should().Contain("This is a test description");
    }

    [Fact]
    public void TaskCard_ShouldTruncateDescription_WhenTooLong()
    {
        // Arrange
        var longDescription = new string('a', 200);
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = longDescription,
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var cut = RenderComponent<TaskCard>(parameters => parameters
            .Add(p => p.Task, task));

        // Assert
        var displayedText = cut.Find(".task-description").TextContent;
        displayedText.Length.Should().BeLessThanOrEqualTo(103); // 100 + "..."
    }

    [Fact]
    public void TaskCard_ShouldInvokeOnClick_WhenCardClicked()
    {
        // Arrange
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test description",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        var wasClicked = false;
        Guid? clickedId = null;

        var cut = RenderComponent<TaskCard>(parameters => parameters
            .Add(p => p.Task, task)
            .Add(p => p.OnClick, (Guid id) =>
            {
                wasClicked = true;
                clickedId = id;
            }));

        // Act
        cut.Find(".task-card").Click();

        // Assert
        wasClicked.Should().BeTrue();
        clickedId.Should().Be(task.Id);
    }

    [Fact]
    public void TaskCard_ShouldDisplayStatusBadge()
    {
        // Arrange
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Status = Models.TaskStatus.InProgress,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var cut = RenderComponent<TaskCard>(parameters => parameters
            .Add(p => p.Task, task));

        // Assert
        var badge = cut.Find(".badge");
        badge.Should().NotBeNull();
        badge.TextContent.Should().Contain("In Progress");
    }

    [Fact]
    public void TaskCard_ShouldDisplayCreatedDate()
    {
        // Arrange
        var createdDate = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = createdDate
        };

        // Act
        var cut = RenderComponent<TaskCard>(parameters => parameters
            .Add(p => p.Task, task));

        // Assert
        var dateElement = cut.Find(".created-date");
        dateElement.Should().NotBeNull();
        dateElement.TextContent.Should().NotBeEmpty();
    }
}
