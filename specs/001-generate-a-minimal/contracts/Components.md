# Component Contracts: Blazor UI Components

**Feature**: 001-generate-a-minimal
**Purpose**: Define the interface and behavior of Blazor components

## Page Components

### Index.razor (Task List Page)

**Route**: `/`

**Purpose**: Display all tasks with filtering and navigation capabilities

**Parameters**: None

**Dependencies**:
- `ITaskService` (injected)

**Component State**:
```csharp
private List<TaskDto> tasks = new();
private List<TaskDto> filteredTasks = new();
private TaskStatus? filterStatus = null;
private bool isLoading = true;
private string? errorMessage = null;
```

**Public Methods**: None (page component)

**Events**:
- `OnInitializedAsync`: Loads all tasks from service
- `OnFilterChanged(TaskStatus? status)`: Filters tasks by status
- `OnTaskClicked(Guid taskId)`: Navigates to task detail page
- `OnCreateTaskClicked()`: Navigates to create task page

**UI Elements**:
- Filter buttons (All, To Do, In Progress, Done)
- Task list using `Virtualize` component (for 1000 tasks)
- Each task rendered as `TaskCard` component
- "Create Task" button
- Loading spinner while data loads
- Error message display area

**Component Tests**:

```csharp
[Fact]
public void Index_ShouldRenderLoadingSpinner_Initially()
{
    // Arrange
    var taskService = Substitute.For<ITaskService>();
    taskService.GetAllTasksAsync().Returns(Task.FromResult(new List<TaskDto>()));

    // Act
    var cut = RenderComponent<Index>(parameters => parameters
        .AddCascadingValue(taskService));

    // Assert
    cut.Find(".loading-spinner").Should().NotBeNull();
}

[Fact]
public async Task Index_ShouldRenderTasks_WhenLoaded()
{
    // Arrange
    var tasks = new List<TaskDto>
    {
        new() { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatus.ToDo },
        new() { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatus.InProgress }
    };
    var taskService = Substitute.For<ITaskService>();
    taskService.GetAllTasksAsync().Returns(Task.FromResult(tasks));

    // Act
    var cut = RenderComponent<Index>(parameters => parameters
        .AddCascadingValue(taskService));
    await Task.Delay(100); // Wait for async load

    // Assert
    cut.FindAll(".task-card").Count.Should().Be(2);
}

[Fact]
public async Task Index_ShouldFilterTasks_WhenFilterApplied()
{
    // Arrange
    var tasks = new List<TaskDto>
    {
        new() { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatus.ToDo },
        new() { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatus.InProgress }
    };
    var taskService = Substitute.For<ITaskService>();
    taskService.GetAllTasksAsync().Returns(Task.FromResult(tasks));

    var cut = RenderComponent<Index>(parameters => parameters
        .AddCascadingValue(taskService));

    // Act
    cut.Find("button[data-filter='InProgress']").Click();

    // Assert
    cut.FindAll(".task-card").Count.Should().Be(1);
    cut.Find(".task-card").TextContent.Should().Contain("Task 2");
}
```

---

### CreateTask.razor (Create Task Page)

**Route**: `/create-task`

**Purpose**: Provide form to create a new task

**Parameters**: None

**Dependencies**:
- `ITaskService` (injected)
- `NavigationManager` (injected)

**Component State**:
```csharp
private CreateTaskRequest model = new();
private bool isSaving = false;
private string? errorMessage = null;
private EditContext? editContext;
```

**Public Methods**: None

**Events**:
- `OnValidSubmit()`: Validates and creates task
- `OnInvalidSubmit()`: Displays validation errors
- `OnCancelClicked()`: Navigates back to task list

**UI Elements**:
- EditForm with validation
- Title input (text, required, max 200 chars)
- Description textarea (required, max 5000 chars)
- Priority dropdown (Low/Medium/High)
- Save button (disabled while saving)
- Cancel button
- Validation summary
- Success/error message display

**Validation**:
- Uses DataAnnotations from `CreateTaskRequest`
- Client-side validation before submit
- Server-side validation in service layer

**Component Tests**:

```csharp
[Fact]
public void CreateTask_ShouldRenderEmptyForm_Initially()
{
    // Arrange & Act
    var cut = RenderComponent<CreateTask>();

    // Assert
    cut.Find("input[name='Title']").GetAttribute("value").Should().BeEmpty();
    cut.Find("textarea[name='Description']").GetAttribute("value").Should().BeEmpty();
    cut.Find("select[name='Priority']").GetAttribute("value").Should().Be("Medium");
}

[Fact]
public async Task CreateTask_ShouldShowValidationError_WhenTitleEmpty()
{
    // Arrange
    var cut = RenderComponent<CreateTask>();

    // Act
    cut.Find("textarea[name='Description']").Change("Valid description");
    cut.Find("form").Submit();

    // Assert
    cut.Find(".validation-message").TextContent.Should().Contain("Title is required");
}

[Fact]
public async Task CreateTask_ShouldCreateTask_WhenFormValid()
{
    // Arrange
    var taskService = Substitute.For<ITaskService>();
    var navManager = Substitute.For<NavigationManager>();
    var expectedTask = new TaskDto { Id = Guid.NewGuid(), Title = "New Task" };
    taskService.CreateTaskAsync(Arg.Any<CreateTaskRequest>())
        .Returns(Result<TaskDto>.Success(expectedTask));

    var cut = RenderComponent<CreateTask>(parameters => parameters
        .AddCascadingValue(taskService)
        .AddCascadingValue(navManager));

    // Act
    cut.Find("input[name='Title']").Change("New Task");
    cut.Find("textarea[name='Description']").Change("Task description");
    cut.Find("select[name='Priority']").Change("High");
    cut.Find("form").Submit();
    await Task.Delay(50);

    // Assert
    await taskService.Received(1).CreateTaskAsync(Arg.Is<CreateTaskRequest>(r =>
        r.Title == "New Task" &&
        r.Description == "Task description" &&
        r.Priority == TaskPriority.High
    ));
    navManager.Received(1).NavigateTo("/");
}
```

---

### TaskDetail.razor (Task Detail/Edit Page)

**Route**: `/task/{Id:guid}`

**Purpose**: Display and edit a single task

**Parameters**:
```csharp
[Parameter] public Guid Id { get; set; }
```

**Dependencies**:
- `ITaskService` (injected)
- `NavigationManager` (injected)

**Component State**:
```csharp
private TaskDto? task = null;
private UpdateTaskRequest model = new();
private bool isEditing = false;
private bool isSaving = false;
private bool isLoading = true;
private string? errorMessage = null;
private EditContext? editContext;
```

**Public Methods**: None

**Events**:
- `OnInitializedAsync()`: Loads task by ID
- `OnParametersSetAsync()`: Reloads if ID changes
- `OnEditClicked()`: Enables edit mode
- `OnSaveClicked()`: Saves changes
- `OnCancelEditClicked()`: Cancels edit, reverts changes
- `OnStatusChanged(TaskStatus newStatus)`: Updates task status
- `OnDeleteClicked()`: Deletes task (with confirmation)

**UI Elements**:
- Task display mode (read-only):
  - Title (large heading)
  - Description (formatted text)
  - Status badge (colored)
  - Priority badge (colored)
  - Created/Updated timestamps
  - Edit button
  - Delete button
- Task edit mode:
  - EditForm with validation
  - Title input
  - Description textarea
  - Status dropdown
  - Priority dropdown
  - Save/Cancel buttons
- Status quick-change buttons (visible in both modes)
- Back to list button
- Loading spinner
- Error message display
- Delete confirmation dialog

**Component Tests**:

```csharp
[Fact]
public async Task TaskDetail_ShouldLoadTask_OnInitialize()
{
    // Arrange
    var taskId = Guid.NewGuid();
    var task = new TaskDto
    {
        Id = taskId,
        Title = "Test Task",
        Description = "Test Description",
        Status = TaskStatus.InProgress,
        Priority = TaskPriority.High
    };
    var taskService = Substitute.For<ITaskService>();
    taskService.GetTaskByIdAsync(taskId).Returns(Task.FromResult<TaskDto?>(task));

    // Act
    var cut = RenderComponent<TaskDetail>(parameters => parameters
        .Add(p => p.Id, taskId)
        .AddCascadingValue(taskService));
    await Task.Delay(50);

    // Assert
    cut.Find("h1").TextContent.Should().Be("Test Task");
    cut.Find(".description").TextContent.Should().Be("Test Description");
}

[Fact]
public async Task TaskDetail_ShouldShowError_WhenTaskNotFound()
{
    // Arrange
    var taskId = Guid.NewGuid();
    var taskService = Substitute.For<ITaskService>();
    taskService.GetTaskByIdAsync(taskId).Returns(Task.FromResult<TaskDto?>(null));

    // Act
    var cut = RenderComponent<TaskDetail>(parameters => parameters
        .Add(p => p.Id, taskId)
        .AddCascadingValue(taskService));
    await Task.Delay(50);

    // Assert
    cut.Find(".error-message").TextContent.Should().Contain("not found");
}

[Fact]
public async Task TaskDetail_ShouldEnableEditMode_WhenEditClicked()
{
    // Arrange
    var task = new TaskDto { Id = Guid.NewGuid(), Title = "Test", Description = "Test" };
    var taskService = Substitute.For<ITaskService>();
    taskService.GetTaskByIdAsync(task.Id).Returns(Task.FromResult<TaskDto?>(task));

    var cut = RenderComponent<TaskDetail>(parameters => parameters
        .Add(p => p.Id, task.Id)
        .AddCascadingValue(taskService));
    await Task.Delay(50);

    // Act
    cut.Find("button[data-action='edit']").Click();

    // Assert
    cut.Find("input[name='Title']").Should().NotBeNull();
    cut.Find("textarea[name='Description']").Should().NotBeNull();
}

[Fact]
public async Task TaskDetail_ShouldUpdateStatus_WhenStatusButtonClicked()
{
    // Arrange
    var task = new TaskDto { Id = Guid.NewGuid(), Title = "Test", Status = TaskStatus.ToDo };
    var taskService = Substitute.For<ITaskService>();
    taskService.GetTaskByIdAsync(task.Id).Returns(Task.FromResult<TaskDto?>(task));
    taskService.UpdateTaskStatusAsync(task.Id, TaskStatus.InProgress)
        .Returns(Result<TaskDto>.Success(task with { Status = TaskStatus.InProgress }));

    var cut = RenderComponent<TaskDetail>(parameters => parameters
        .Add(p => p.Id, task.Id)
        .AddCascadingValue(taskService));
    await Task.Delay(50);

    // Act
    cut.Find("button[data-status='InProgress']").Click();
    await Task.Delay(50);

    // Assert
    await taskService.Received(1).UpdateTaskStatusAsync(task.Id, TaskStatus.InProgress);
}
```

---

## Shared Components

### TaskCard.razor

**Purpose**: Reusable component to display a task summary

**Parameters**:
```csharp
[Parameter, EditorRequired] public TaskDto Task { get; set; } = null!;
[Parameter] public EventCallback<Guid> OnClick { get; set; }
```

**Component State**: None (stateless)

**UI Elements**:
- Card container (clickable)
- Task title (bold)
- Task description (truncated to 100 chars)
- Status badge component
- Priority badge component
- Created date (formatted)

**Component Tests**:

```csharp
[Fact]
public void TaskCard_ShouldRenderTaskDetails()
{
    // Arrange
    var task = new TaskDto
    {
        Id = Guid.NewGuid(),
        Title = "Test Task",
        Description = "This is a test description",
        Status = TaskStatus.InProgress,
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
    var task = new TaskDto { Id = Guid.NewGuid(), Title = "Test", Description = longDescription };

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
    var task = new TaskDto { Id = Guid.NewGuid(), Title = "Test" };
    var wasClicked = false;
    Guid? clickedId = null;

    var cut = RenderComponent<TaskCard>(parameters => parameters
        .Add(p => p.Task, task)
        .Add(p => p.OnClick, EventCallback.Factory.Create<Guid>(this, id =>
        {
            wasClicked = true;
            clickedId = id;
        })));

    // Act
    cut.Find(".task-card").Click();

    // Assert
    wasClicked.Should().BeTrue();
    clickedId.Should().Be(task.Id);
}
```

---

### StatusBadge.razor

**Purpose**: Display task status with appropriate styling

**Parameters**:
```csharp
[Parameter, EditorRequired] public TaskStatus Status { get; set; }
```

**Component State**: None (stateless)

**UI Elements**:
- Badge/pill element
- Status text (enum to display text conversion)
- Color coding:
  - ToDo: Gray (#6c757d)
  - InProgress: Blue (#0d6efd)
  - Done: Green (#198754)

**Component Tests**:

```csharp
[Theory]
[InlineData(TaskStatus.ToDo, "To Do", "bg-secondary")]
[InlineData(TaskStatus.InProgress, "In Progress", "bg-primary")]
[InlineData(TaskStatus.Done, "Done", "bg-success")]
public void StatusBadge_ShouldRenderCorrectly(TaskStatus status, string expectedText, string expectedClass)
{
    // Act
    var cut = RenderComponent<StatusBadge>(parameters => parameters
        .Add(p => p.Status, status));

    // Assert
    var badge = cut.Find(".badge");
    badge.TextContent.Should().Be(expectedText);
    badge.ClassList.Should().Contain(expectedClass);
}
```

---

## Component Communication

### Event Flow

**Task Creation Flow**:
1. User clicks "Create Task" on Index page
2. NavigationManager navigates to `/create-task`
3. User fills CreateTask form
4. Form validation occurs (client-side)
5. OnValidSubmit calls ITaskService.CreateTaskAsync
6. Service returns Result<TaskDto>
7. On success: Navigate to `/`
8. On failure: Display error message

**Task Status Update Flow**:
1. User clicks status button on TaskDetail page
2. Component calls ITaskService.UpdateTaskStatusAsync
3. Service updates database and returns updated task
4. Component updates local state
5. UI re-renders with new status badge

**Task Edit Flow**:
1. User clicks "Edit" on TaskDetail page
2. Component enters edit mode (isEditing = true)
3. Form populates with current task data
4. User modifies fields
5. User clicks "Save"
6. Validation occurs
7. OnValidSubmit calls ITaskService.UpdateTaskAsync
8. Service returns Result<TaskDto>
9. On success: Exit edit mode, refresh task
10. On failure: Display error message

## Routing Configuration

```csharp
// App.razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>
```

## Error Handling in Components

All components should handle errors gracefully:

```csharp
try
{
    var result = await TaskService.CreateTaskAsync(model);
    if (result.IsSuccess)
    {
        NavigationManager.NavigateTo("/");
    }
    else
    {
        errorMessage = result.ErrorMessage; // Display to user
    }
}
catch (Exception ex)
{
    errorMessage = "An unexpected error occurred. Please try again.";
    // Log exception (future: use ILogger)
    Console.Error.WriteLine($"Error: {ex.Message}");
}
```

## Accessibility Requirements

All components must be accessible:
- Proper semantic HTML (headings, lists, buttons vs. links)
- ARIA labels where needed
- Keyboard navigation support
- Focus management (especially for modals/dialogs)
- Color contrast ratios meet WCAG 2.1 AA standards
- Form labels associated with inputs

Example:
```razor
<button class="btn btn-primary"
        aria-label="Create new task"
        @onclick="OnCreateTaskClicked">
    Create Task
</button>
```
