using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MiniJira.Components.Pages;
using MiniJira.Models;
using MiniJira.Services;
using MiniJira.Services.DTOs;
using NSubstitute;

namespace MiniJira.Tests.Components.Pages;

public class TaskDetailTests : TestContext
{
    [Fact]
    public async System.Threading.Tasks.Task TaskDetail_ShouldLoadTask_OnInitialize()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskDto
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = Models.TaskStatus.InProgress,
            Priority = TaskPriority.High,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetTaskByIdAsync(taskId).Returns(System.Threading.Tasks.Task.FromResult<TaskDto?>(task));

        Services.AddSingleton(taskService);

        // Act
        var cut = RenderComponent<TaskDetail>(parameters => parameters
            .Add(p => p.Id, taskId));
        await System.Threading.Tasks.Task.Delay(100);

        // Assert
        cut.Find("h1, .task-title").TextContent.Should().Contain("Test Task");
        cut.Find(".description, .task-description").TextContent.Should().Contain("Test Description");
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskDetail_ShouldShowError_WhenTaskNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var taskService = Substitute.For<ITaskService>();
        taskService.GetTaskByIdAsync(taskId).Returns(System.Threading.Tasks.Task.FromResult<TaskDto?>(null));

        Services.AddSingleton(taskService);

        // Act
        var cut = RenderComponent<TaskDetail>(parameters => parameters
            .Add(p => p.Id, taskId));
        await System.Threading.Tasks.Task.Delay(100);

        // Assert
        var errorElements = cut.FindAll(".error-message, .alert-danger");
        errorElements.Should().NotBeEmpty("error message should be displayed when task not found");
        cut.Markup.Should().Contain("not found");
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskDetail_ShouldEnableEditMode_WhenEditClicked()
    {
        // Arrange
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetTaskByIdAsync(task.Id).Returns(System.Threading.Tasks.Task.FromResult<TaskDto?>(task));

        Services.AddSingleton(taskService);

        var cut = RenderComponent<TaskDetail>(parameters => parameters
            .Add(p => p.Id, task.Id));
        await System.Threading.Tasks.Task.Delay(100);

        // Act
        var editButton = cut.Find("button[data-action='edit']");
        editButton.Click();

        // Assert
        cut.Find("input[name='Title'], input#Title").Should().NotBeNull();
        cut.Find("textarea[name='Description'], textarea#Description").Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskDetail_ShouldUpdateStatus_WhenStatusButtonClicked()
    {
        // Arrange
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var updatedTask = new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = Models.TaskStatus.InProgress,
            Priority = task.Priority,
            CreatedAt = task.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetTaskByIdAsync(task.Id).Returns(System.Threading.Tasks.Task.FromResult<TaskDto?>(task));
        taskService.UpdateTaskStatusAsync(task.Id, Models.TaskStatus.InProgress)
            .Returns(Result<TaskDto>.Success(updatedTask));

        Services.AddSingleton(taskService);

        var cut = RenderComponent<TaskDetail>(parameters => parameters
            .Add(p => p.Id, task.Id));
        await System.Threading.Tasks.Task.Delay(100);

        // Act
        var statusButton = cut.Find("button[data-status='InProgress']");
        statusButton.Click();
        await System.Threading.Tasks.Task.Delay(100);

        // Assert
        await taskService.Received(1).UpdateTaskStatusAsync(task.Id, Models.TaskStatus.InProgress);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskDetail_ShouldSaveChanges_WhenSaveClicked()
    {
        // Arrange
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Description = "Original Description",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var updatedTask = new TaskDto
        {
            Id = task.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            Status = Models.TaskStatus.InProgress,
            Priority = TaskPriority.High,
            CreatedAt = task.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetTaskByIdAsync(task.Id).Returns(System.Threading.Tasks.Task.FromResult<TaskDto?>(task));
        taskService.UpdateTaskAsync(task.Id, Arg.Any<UpdateTaskRequest>())
            .Returns(Result<TaskDto>.Success(updatedTask));

        Services.AddSingleton(taskService);

        var cut = RenderComponent<TaskDetail>(parameters => parameters
            .Add(p => p.Id, task.Id));
        await System.Threading.Tasks.Task.Delay(100);

        // Enter edit mode
        cut.Find("button[data-action='edit']").Click();

        // Act - Modify and save
        cut.Find("input[name='Title'], input#Title").Change("Updated Title");
        cut.Find("textarea[name='Description'], textarea#Description").Change("Updated Description");
        cut.Find("select[name='Priority'], select#Priority").Change("2"); // High
        cut.Find("select[name='Status'], select#Status").Change("1"); // InProgress
        cut.Find("button[data-action='save'], button[type='submit']").Click();
        await System.Threading.Tasks.Task.Delay(100);

        // Assert
        await taskService.Received(1).UpdateTaskAsync(task.Id, Arg.Is<UpdateTaskRequest>(r =>
            r.Title == "Updated Title" &&
            r.Description == "Updated Description" &&
            r.Priority == TaskPriority.High &&
            r.Status == Models.TaskStatus.InProgress
        ));
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskDetail_ShouldCancelEdit_WhenCancelClicked()
    {
        // Arrange
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Description = "Original Description",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetTaskByIdAsync(task.Id).Returns(System.Threading.Tasks.Task.FromResult<TaskDto?>(task));

        Services.AddSingleton(taskService);

        var cut = RenderComponent<TaskDetail>(parameters => parameters
            .Add(p => p.Id, task.Id));
        await System.Threading.Tasks.Task.Delay(100);

        // Enter edit mode
        cut.Find("button[data-action='edit']").Click();
        cut.Find("input[name='Title'], input#Title").Change("Modified Title");

        // Act - Cancel
        var cancelButton = cut.Find("button[data-action='cancel']");
        cancelButton.Click();

        // Assert
        // Should exit edit mode and not save changes
        await taskService.DidNotReceive().UpdateTaskAsync(Arg.Any<Guid>(), Arg.Any<UpdateTaskRequest>());
        cut.Find("h1, .task-title").TextContent.Should().Contain("Original Title");
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskDetail_ShouldDeleteTask_WhenDeleteConfirmed()
    {
        // Arrange
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetTaskByIdAsync(task.Id).Returns(System.Threading.Tasks.Task.FromResult<TaskDto?>(task));
        taskService.DeleteTaskAsync(task.Id).Returns(Result.Success());

        Services.AddSingleton(taskService);
        var navManager = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        var cut = RenderComponent<TaskDetail>(parameters => parameters
            .Add(p => p.Id, task.Id));
        await System.Threading.Tasks.Task.Delay(100);

        // Act - Click delete and confirm
        var deleteButton = cut.Find("button[data-action='delete']");
        deleteButton.Click();

        // Confirm deletion (assuming confirmation dialog or second click)
        var confirmButton = cut.Find("button[data-action='confirm-delete']");
        confirmButton.Click();
        await System.Threading.Tasks.Task.Delay(100);

        // Assert
        await taskService.Received(1).DeleteTaskAsync(task.Id);
        navManager.Uri.Should().EndWith("/");
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskDetail_ShouldNavigateBack_WhenBackClicked()
    {
        // Arrange
        var task = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetTaskByIdAsync(task.Id).Returns(System.Threading.Tasks.Task.FromResult<TaskDto?>(task));

        Services.AddSingleton(taskService);
        var navManager = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        var cut = RenderComponent<TaskDetail>(parameters => parameters
            .Add(p => p.Id, task.Id));
        await System.Threading.Tasks.Task.Delay(100);

        // Act
        var backButton = cut.Find("button[data-action='back'], a[href='/']");
        backButton.Click();

        // Assert
        navManager.Uri.Should().EndWith("/");
    }
}
