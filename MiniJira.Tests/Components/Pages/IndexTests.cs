using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using IndexPage = MiniJira.Components.Pages.Index;
using MiniJira.Models;
using MiniJira.Services;
using MiniJira.Services.DTOs;
using NSubstitute;

namespace MiniJira.Tests.Components.Pages;

public class IndexTests : TestContext
{
    [Fact]
    public void Index_ShouldRenderLoadingSpinner_Initially()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        var taskCompletionSource = new TaskCompletionSource<List<TaskDto>>();
        taskService.GetAllTasksAsync().Returns(taskCompletionSource.Task);

        Services.AddSingleton(taskService);

        // Act
        var cut = RenderComponent<IndexPage>();

        // Assert
        var spinner = cut.FindAll(".loading-spinner, .spinner-border, [role='status']");
        spinner.Should().NotBeEmpty("loading spinner should be displayed initially");
    }

    [Fact]
    public async System.Threading.Tasks.Task Index_ShouldRenderTasks_WhenLoaded()
    {
        // Arrange
        var tasks = new List<TaskDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.ToDo, Priority = TaskPriority.Medium, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Task 2", Description = "Desc 2", Status = Models.TaskStatus.InProgress, Priority = TaskPriority.High, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetAllTasksAsync().Returns(System.Threading.Tasks.Task.FromResult(tasks));

        Services.AddSingleton(taskService);

        // Act
        var cut = RenderComponent<IndexPage>();
        await System.Threading.Tasks.Task.Delay(100); // Wait for async load

        // Assert
        cut.FindAll(".task-card").Count.Should().Be(2);
    }

    [Fact]
    public async System.Threading.Tasks.Task Index_ShouldFilterTasks_WhenFilterApplied()
    {
        // Arrange
        var tasks = new List<TaskDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.ToDo, Priority = TaskPriority.Medium, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Task 2", Description = "Desc 2", Status = Models.TaskStatus.InProgress, Priority = TaskPriority.High, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetAllTasksAsync().Returns(System.Threading.Tasks.Task.FromResult(tasks));

        Services.AddSingleton(taskService);

        var cut = RenderComponent<IndexPage>();
        await System.Threading.Tasks.Task.Delay(100);

        // Act
        var filterButton = cut.Find("button[data-filter='InProgress']");
        filterButton.Click();

        // Assert
        cut.FindAll(".task-card").Count.Should().Be(1);
        cut.Find(".task-card").TextContent.Should().Contain("Task 2");
    }

    [Fact]
    public async System.Threading.Tasks.Task Index_ShouldNavigateToCreatePage_WhenCreateButtonClicked()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        taskService.GetAllTasksAsync().Returns(System.Threading.Tasks.Task.FromResult(new List<TaskDto>()));

        Services.AddSingleton(taskService);
        var navManager = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        var cut = RenderComponent<IndexPage>();
        await System.Threading.Tasks.Task.Delay(100);

        // Act
        var createButton = cut.Find("button[data-action='create'], a[href='/create-task']");
        createButton.Click();

        // Assert
        navManager.Uri.Should().Contain("/create-task");
    }

    [Fact]
    public async System.Threading.Tasks.Task Index_ShouldNavigateToTaskDetail_WhenTaskCardClicked()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var tasks = new List<TaskDto>
        {
            new() { Id = taskId, Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.ToDo, Priority = TaskPriority.Medium, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetAllTasksAsync().Returns(System.Threading.Tasks.Task.FromResult(tasks));

        Services.AddSingleton(taskService);
        var navManager = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        var cut = RenderComponent<IndexPage>();
        await System.Threading.Tasks.Task.Delay(100);

        // Act
        var taskCard = cut.Find(".task-card");
        taskCard.Click();

        // Assert
        navManager.Uri.Should().Contain($"/task/{taskId}");
    }

    [Fact]
    public async System.Threading.Tasks.Task Index_ShouldShowAllTasks_WhenAllFilterClicked()
    {
        // Arrange
        var tasks = new List<TaskDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.ToDo, Priority = TaskPriority.Medium, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Task 2", Description = "Desc 2", Status = Models.TaskStatus.InProgress, Priority = TaskPriority.High, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Task 3", Description = "Desc 3", Status = Models.TaskStatus.Done, Priority = TaskPriority.Low, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        var taskService = Substitute.For<ITaskService>();
        taskService.GetAllTasksAsync().Returns(System.Threading.Tasks.Task.FromResult(tasks));

        Services.AddSingleton(taskService);

        var cut = RenderComponent<IndexPage>();
        await System.Threading.Tasks.Task.Delay(100);

        // First filter to InProgress
        cut.Find("button[data-filter='InProgress']").Click();
        cut.FindAll(".task-card").Count.Should().Be(1);

        // Act - Click "All" filter
        var allButton = cut.Find("button[data-filter='All'], button[data-filter='']");
        allButton.Click();

        // Assert
        cut.FindAll(".task-card").Count.Should().Be(3);
    }

    [Fact]
    public async System.Threading.Tasks.Task Index_ShouldDisplayError_WhenServiceFails()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        taskService.GetAllTasksAsync().Returns<List<TaskDto>>(_ => throw new Exception("Service error"));

        Services.AddSingleton(taskService);

        // Act
        var cut = RenderComponent<IndexPage>();
        await System.Threading.Tasks.Task.Delay(100);

        // Assert
        var errorElements = cut.FindAll(".error-message, .alert-danger");
        errorElements.Should().NotBeEmpty("error message should be displayed when service fails");
    }
}
