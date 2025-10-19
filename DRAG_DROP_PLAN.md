# Drag & Drop Implementation Plan for MiniJira Kanban Board

## 🎯 Goal
Enable drag-and-drop functionality for task cards across kanban columns (TO DO → IN PROGRESS → DONE)

## 📋 Current State Analysis

### ✅ What We Have
- **TaskService**: `UpdateTaskStatusAsync(Guid id, TaskStatus newStatus)` method exists
- **TaskCard Component**: Simple, clean component with OnClick event
- **Three Columns**: TO DO, IN PROGRESS, DONE
- **Column Structure**: Each has `kanban-column-content` div for cards
- **Existing IDs**: Cards have unique IDs (`task-card-{guid}`)

### 🚧 What We Need
- Make TaskCard draggable
- Make columns accept drops
- Handle drag state
- Update backend on drop
- Visual feedback during drag
- Error handling & rollback

---

## 🏗️ Architecture Decision

### Recommended Approach: **HTML5 Drag & Drop API**

**Why?**
- ✅ Native browser support (no external dependencies)
- ✅ Keeps bundle size small
- ✅ Blazor has good interop support
- ✅ Works well with existing Jira-style UI
- ✅ No conflicts with current design system

**Alternatives Considered:**
- ❌ SortableJS: External dependency, overkill for simple kanban
- ❌ MudBlazor/Radzen: Too heavy, conflicts with custom Jira design
- ❌ Custom mouse events: Browser inconsistencies, reinventing wheel

---

## 📝 Implementation Steps

### **Phase 1: Make TaskCard Draggable**

#### File: `Components/Shared/TaskCard.razor`
```razor
<div id="task-card-@Task.Id"
     class="jira-task-card"
     draggable="true"
     @ondragstart="HandleDragStart"
     @ondragend="HandleDragEnd"
     @onclick="HandleClick"
     role="button"
     tabindex="0"
     @onkeydown="HandleKeyDown">
    <!-- existing content -->
</div>

@code {
    [Parameter, EditorRequired]
    public TaskDto Task { get; set; } = null!;

    [Parameter]
    public EventCallback<Guid> OnClick { get; set; }

    [Parameter]
    public EventCallback<TaskDto> OnDragStart { get; set; }

    [Parameter]
    public EventCallback OnDragEnd { get; set; }

    private async Task HandleDragStart(DragEventArgs e)
    {
        await OnDragStart.InvokeAsync(Task);
    }

    private async Task HandleDragEnd(DragEventArgs e)
    {
        await OnDragEnd.InvokeAsync();
    }

    // ... existing methods
}
```

---

### **Phase 2: Update Index.razor (Board) for Drop Zones**

#### File: `Components/Pages/Index.razor`

**Add drop zone attributes to each column content:**

```razor
<!-- TO DO Column -->
<div id="kanban-column-content-todo"
     class="kanban-column-content @GetDropZoneClass(TaskStatus.ToDo)"
     @ondrop="@(e => HandleDrop(e, TaskStatus.ToDo))"
     @ondrop:preventDefault="true"
     @ondragover:preventDefault="true"
     @ondragenter="@(e => HandleDragEnter(TaskStatus.ToDo))"
     @ondragleave="@(e => HandleDragLeave(TaskStatus.ToDo))">
    @if (todoTasks.Count == 0)
    {
        <div class="empty-column">
            <p class="empty-text">No tasks</p>
        </div>
    }
    else
    {
        @foreach (var task in todoTasks)
        {
            <TaskCard Task="@task"
                      OnClick="@NavigateToTask"
                      OnDragStart="@HandleDragStart"
                      OnDragEnd="@HandleDragEnd" />
        }
    }
</div>

<!-- Repeat for IN PROGRESS and DONE columns -->
```

**Add code-behind logic:**

```csharp
@code {
    // ... existing fields
    private TaskDto? draggedTask = null;
    private TaskStatus? dragOverColumn = null;
    private bool isUpdating = false;

    private void HandleDragStart(TaskDto task)
    {
        draggedTask = task;
    }

    private void HandleDragEnd()
    {
        draggedTask = null;
        dragOverColumn = null;
    }

    private void HandleDragEnter(TaskStatus status)
    {
        dragOverColumn = status;
    }

    private void HandleDragLeave(TaskStatus status)
    {
        if (dragOverColumn == status)
        {
            dragOverColumn = null;
        }
    }

    private async Task HandleDrop(DragEventArgs e, TaskStatus newStatus)
    {
        if (draggedTask == null || isUpdating) return;

        // Don't do anything if dropped in same column
        if (draggedTask.Status == newStatus)
        {
            dragOverColumn = null;
            return;
        }

        isUpdating = true;
        var originalStatus = draggedTask.Status;

        try
        {
            // Optimistic UI update
            UpdateTaskListsOptimistically(draggedTask, newStatus);
            StateHasChanged();

            // Call backend API
            var result = await TaskService.UpdateTaskStatusAsync(draggedTask.Id, newStatus);

            if (!result.IsSuccess)
            {
                // Rollback on failure
                UpdateTaskListsOptimistically(draggedTask, originalStatus);
                errorMessage = $"Failed to update task: {result.ErrorMessage}";
            }
            else
            {
                // Update the task DTO with new status
                draggedTask.Status = newStatus;
            }
        }
        catch (Exception ex)
        {
            // Rollback on exception
            UpdateTaskListsOptimistically(draggedTask, originalStatus);
            errorMessage = $"Error updating task: {ex.Message}";
        }
        finally
        {
            isUpdating = false;
            dragOverColumn = null;
            draggedTask = null;
            StateHasChanged();
        }
    }

    private void UpdateTaskListsOptimistically(TaskDto task, TaskStatus newStatus)
    {
        // Remove from current list
        todoTasks.Remove(task);
        inProgressTasks.Remove(task);
        doneTasks.Remove(task);

        // Add to new list
        var targetList = newStatus switch
        {
            TaskStatus.ToDo => todoTasks,
            TaskStatus.InProgress => inProgressTasks,
            TaskStatus.Done => doneTasks,
            _ => todoTasks
        };
        targetList.Add(task);

        // Update counts
        todoCount = todoTasks.Count;
        inProgressCount = inProgressTasks.Count;
        doneCount = doneTasks.Count;
    }

    private string GetDropZoneClass(TaskStatus status)
    {
        if (draggedTask != null && dragOverColumn == status && draggedTask.Status != status)
        {
            return "drag-over";
        }
        return "";
    }

    // ... existing methods
}
```

---

### **Phase 3: Visual Feedback CSS**

#### File: `Components/Pages/Index.razor.css`

**Add drag-and-drop styles:**

```css
/* Drag and Drop Styles */

/* Dragging card ghost effect */
.jira-task-card[draggable="true"] {
    cursor: grab;
    transition: opacity 0.2s ease, transform 0.1s ease;
}

.jira-task-card[draggable="true"]:active {
    cursor: grabbing;
}

/* Card being dragged */
.jira-task-card.dragging {
    opacity: 0.5;
    transform: scale(0.95);
}

/* Drop zone highlight */
.kanban-column-content.drag-over {
    background-color: var(--jira-primary-light, rgba(0, 82, 204, 0.08));
    border: 2px dashed var(--jira-primary);
    border-radius: var(--jira-radius-small);
    transition: background-color 0.2s ease, border 0.2s ease;
}

/* Drop zone when empty */
.kanban-column-content.drag-over .empty-column {
    background-color: transparent;
    border: none;
}

/* Prevent text selection during drag */
.kanban-column-content {
    user-select: none;
    -webkit-user-select: none;
    -moz-user-select: none;
}

/* Drop animation */
@keyframes dropIn {
    0% {
        transform: scale(0.9);
        opacity: 0;
    }
    50% {
        transform: scale(1.05);
    }
    100% {
        transform: scale(1);
        opacity: 1;
    }
}

.jira-task-card.just-dropped {
    animation: dropIn 0.3s ease;
}
```

---

### **Phase 4: Accessibility & Keyboard Support**

#### File: `Components/Shared/TaskCard.razor` (Enhancement)

**Add keyboard navigation:**

```csharp
@code {
    [Parameter]
    public EventCallback<KeyboardMoveRequest> OnKeyboardMove { get; set; }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        // Existing Enter/Space handler
        if (e.Key == "Enter" || e.Key == " ")
        {
            await OnClick.InvokeAsync(Task.Id);
            return;
        }

        // Arrow key navigation for drag-drop
        if (e.CtrlKey)
        {
            var direction = e.Key switch
            {
                "ArrowLeft" => MoveDirection.Left,
                "ArrowRight" => MoveDirection.Right,
                _ => MoveDirection.None
            };

            if (direction != MoveDirection.None)
            {
                await OnKeyboardMove.InvokeAsync(new KeyboardMoveRequest
                {
                    TaskId = Task.Id,
                    Direction = direction
                });
            }
        }
    }
}

public enum MoveDirection { None, Left, Right }
public record KeyboardMoveRequest
{
    public Guid TaskId { get; init; }
    public MoveDirection Direction { get; init; }
}
```

---

## 🧪 Testing Checklist

### Functional Testing
- [ ] Drag card from TO DO to IN PROGRESS
- [ ] Drag card from IN PROGRESS to DONE
- [ ] Drag card from DONE back to TO DO
- [ ] Drop card in same column (should not update)
- [ ] Drag multiple different cards sequentially
- [ ] Network error handling (disconnect during drop)
- [ ] Rollback on failure

### Visual Testing
- [ ] Drag ghost appears correctly
- [ ] Drop zone highlights on drag over
- [ ] Drop zone un-highlights on drag leave
- [ ] Card animates into position on drop
- [ ] Empty column message shows/hides correctly
- [ ] Task counts update correctly

### Cross-Browser Testing
- [ ] Chrome/Edge (Chromium)
- [ ] Firefox
- [ ] Safari (if available)

### Responsive Testing
- [ ] Desktop (1920x1080)
- [ ] Tablet (768x1024)
- [ ] Mobile (touch drag - may need touch events)

### Accessibility Testing
- [ ] Keyboard navigation (Ctrl+Arrow keys)
- [ ] Screen reader announces state changes
- [ ] Focus management after drop
- [ ] ARIA live regions for status updates

---

## ⚠️ Edge Cases & Error Handling

### Scenarios to Handle:

1. **Network Failure During Drop**
   - Optimistic update shown immediately
   - API call fails → rollback to original state
   - Show error message to user

2. **Concurrent Updates**
   - Use `isUpdating` flag to prevent double-drops
   - Potentially add task version/timestamp checking

3. **Touch Devices**
   - HTML5 drag-drop doesn't work well on touch
   - May need to add touch event handlers
   - Alternative: Hold-to-drag on mobile

4. **Empty Columns**
   - Ensure drop zone is still large enough
   - Empty message doesn't interfere with drop

5. **Quick Successive Drags**
   - Debounce or lock during API call
   - Queue updates vs reject new drags

---

## 📊 Performance Considerations

1. **Minimal Re-renders**
   - Use `@key` directive on TaskCard loops
   - Only update affected columns

2. **Optimistic UI**
   - Update UI immediately, sync with backend async
   - Better perceived performance

3. **State Management**
   - Keep drag state minimal (just current dragged task)
   - Clean up on drag end

---

## 🔄 Future Enhancements

### Phase 2 Features (Future):
- **Card Reordering**: Drag to reorder within same column
- **Multi-select Drag**: Drag multiple cards at once
- **Undo/Redo**: Action history with Ctrl+Z
- **Drag Preview**: Custom drag image (card snapshot)
- **Touch Gestures**: Swipe to move on mobile
- **Animations**: Smooth transitions for other cards
- **Real-time Sync**: WebSocket updates when others move cards

---

## 📦 Files to Modify

### Required Changes:
1. ✏️ `Components/Shared/TaskCard.razor` - Add draggable attributes & handlers
2. ✏️ `Components/Shared/TaskCard.razor` (code) - Add drag event callbacks
3. ✏️ `Components/Pages/Index.razor` - Add drop zones to columns
4. ✏️ `Components/Pages/Index.razor` (code) - Add drag/drop logic & state
5. ✏️ `Components/Pages/Index.razor.css` - Add drag/drop visual styles

### Optional New Files:
6. 🆕 `Services/DragDropState.cs` - Centralized drag state (if needed)
7. 🆕 `Models/DragDropModels.cs` - DTOs for drag events (if needed)

---

## 🚀 Implementation Order

### Sprint 1: Basic Drag & Drop (MVP)
1. Make TaskCard draggable (Phase 1)
2. Add drop zones to Index.razor (Phase 2 - basic)
3. Implement HandleDrop logic with API call
4. Add basic visual feedback (Phase 3 - minimal)
5. Test happy path

### Sprint 2: Polish & Error Handling
1. Add optimistic updates & rollback
2. Enhanced visual feedback (animations)
3. Error handling & user notifications
4. Cross-browser testing

### Sprint 3: Accessibility
1. Keyboard navigation (Ctrl+Arrows)
2. ARIA labels and live regions
3. Focus management
4. Screen reader testing

### Sprint 4: Mobile Support (Optional)
1. Touch event handlers
2. Hold-to-drag on mobile
3. Mobile-specific UI adjustments

---

## 💡 Key Insights

### Why HTML5 Drag & Drop?
- **Blazor Server** architecture means we need minimal JS
- HTML5 API is well-supported in Blazor via event handlers
- Keeps implementation pure C# (no JS interop needed)

### Why Optimistic UI?
- Blazor Server has network round-trip latency
- Optimistic updates provide instant feedback
- Rollback ensures data consistency on errors

### Why Not External Libraries?
- Current design is custom Jira clone
- Adding heavy libraries (MudBlazor) would break visual consistency
- HTML5 API is sufficient for kanban use case

---

## 🎨 Visual Design Notes

Match Jira's drag-and-drop UX:
- **Grab cursor** on hover
- **Grabbing cursor** during drag
- **50% opacity** for dragged card
- **Light blue background** (#0052CC @ 8% opacity) for drop zone
- **Dashed border** on drop zone
- **Smooth animations** (0.2-0.3s transitions)

---

## ✅ Success Criteria

The implementation is complete when:
1. ✅ User can drag any task card
2. ✅ User can drop card in any column
3. ✅ Task status updates in database
4. ✅ UI updates immediately (optimistic)
5. ✅ Errors show user-friendly messages
6. ✅ Failed updates rollback visually
7. ✅ Visual feedback matches Jira UX
8. ✅ Works on desktop browsers
9. ✅ No console errors during drag/drop
10. ✅ Task counts update correctly

---

## 📚 References

- [MDN: HTML Drag and Drop API](https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API)
- [Blazor Event Handling](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling)
- [Atlassian Design System - Drag & Drop](https://atlassian.design/patterns/drag-and-drop)

---

**Created:** 2025-10-05
**Author:** Claude Code
**Status:** 📋 Planning Complete - Ready for Implementation
