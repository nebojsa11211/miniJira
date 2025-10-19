using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MiniJira.Components.Pages;
using MiniJira.Models;
using MiniJira.Services;
using MiniJira.Services.DTOs;
using NSubstitute;

namespace MiniJira.Tests.Components.Pages;

public class CreateTaskTests : TestContext
{
    [Fact]
    public void CreateTask_ShouldRenderEmptyForm_Initially()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        Services.AddSingleton(taskService);

        // Act
        var cut = RenderComponent<CreateTask>();

        // Assert
        var titleInput = cut.Find("input[name='Title'], input#Title");
        var descriptionInput = cut.Find("textarea[name='Description'], textarea#Description");
        var prioritySelect = cut.Find("select[name='Priority'], select#Priority");

        titleInput.GetAttribute("value").Should().BeNullOrEmpty();
        descriptionInput.TextContent.Should().BeEmpty();
        prioritySelect.GetAttribute("value").Should().Be("Medium"); // Default is Medium
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_ShouldShowValidationError_WhenTitleEmpty()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        Services.AddSingleton(taskService);

        var cut = RenderComponent<CreateTask>();

        // Act
        cut.Find("textarea[name='Description'], textarea#Description").Change("Valid description");
        cut.Find("form").Submit();
        await System.Threading.Tasks.Task.Delay(50);

        // Assert
        var validationMessages = cut.FindAll(".validation-message, .invalid-feedback");
        validationMessages.Should().NotBeEmpty("validation message should be shown for empty title");
        cut.Markup.Should().Contain("Title", "error message should mention Title field");
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_ShouldShowValidationError_WhenDescriptionEmpty()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        Services.AddSingleton(taskService);

        var cut = RenderComponent<CreateTask>();

        // Act
        cut.Find("input[name='Title'], input#Title").Change("Valid title");
        cut.Find("form").Submit();
        await System.Threading.Tasks.Task.Delay(50);

        // Assert
        var validationMessages = cut.FindAll(".validation-message, .invalid-feedback");
        validationMessages.Should().NotBeEmpty("validation message should be shown for empty description");
        cut.Markup.Should().Contain("Description", "error message should mention Description field");
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_ShouldCreateTask_WhenFormValid()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        var expectedTask = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "New Task",
            Description = "Task description",
            Status = Models.TaskStatus.ToDo,
            Priority = TaskPriority.High,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        taskService.CreateTaskAsync(Arg.Any<CreateTaskRequest>())
            .Returns(Result<TaskDto>.Success(expectedTask));

        Services.AddSingleton(taskService);
        var navManager = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        var cut = RenderComponent<CreateTask>();

        // Act
        cut.Find("input[name='Title'], input#Title").Change("New Task");
        cut.Find("textarea[name='Description'], textarea#Description").Change("Task description");
        cut.Find("select[name='Priority'], select#Priority").Change("2"); // High = 2
        cut.Find("form").Submit();
        await System.Threading.Tasks.Task.Delay(100);

        // Assert
        await taskService.Received(1).CreateTaskAsync(Arg.Is<CreateTaskRequest>(r =>
            r.Title == "New Task" &&
            r.Description == "Task description" &&
            r.Priority == TaskPriority.High
        ));
        navManager.Uri.Should().EndWith("/");
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_ShouldNavigateToIndex_WhenCancelClicked()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        Services.AddSingleton(taskService);
        var navManager = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        var cut = RenderComponent<CreateTask>();

        // Act
        var cancelButton = cut.Find("button[data-action='cancel'], a[href='/']");
        cancelButton.Click();

        // Assert
        navManager.Uri.Should().EndWith("/");
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_ShouldShowError_WhenServiceFails()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        taskService.CreateTaskAsync(Arg.Any<CreateTaskRequest>())
            .Returns(Result<TaskDto>.Failure("Failed to create task"));

        Services.AddSingleton(taskService);

        var cut = RenderComponent<CreateTask>();

        // Act
        cut.Find("input[name='Title'], input#Title").Change("New Task");
        cut.Find("textarea[name='Description'], textarea#Description").Change("Description");
        cut.Find("form").Submit();
        await System.Threading.Tasks.Task.Delay(100);

        // Assert
        var errorElements = cut.FindAll(".error-message, .alert-danger");
        errorElements.Should().NotBeEmpty("error message should be displayed when creation fails");
        cut.Markup.Should().Contain("Failed to create task");
    }

    [Fact]
    public void CreateTask_ShouldDisableSaveButton_WhileSaving()
    {
        // Arrange
        var taskService = Substitute.For<ITaskService>();
        var taskCompletionSource = new TaskCompletionSource<Result<TaskDto>>();
        taskService.CreateTaskAsync(Arg.Any<CreateTaskRequest>())
            .Returns(taskCompletionSource.Task);

        Services.AddSingleton(taskService);

        var cut = RenderComponent<CreateTask>();

        // Act
        cut.Find("input[name='Title'], input#Title").Change("New Task");
        cut.Find("textarea[name='Description'], textarea#Description").Change("Description");
        cut.Find("form").Submit();

        // Assert
        var saveButton = cut.Find("button[type='submit']");
        saveButton.HasAttribute("disabled").Should().BeTrue("save button should be disabled while saving");
    }
}
