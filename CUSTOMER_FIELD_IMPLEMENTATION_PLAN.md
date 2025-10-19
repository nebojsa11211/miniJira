# Customer Field Implementation Plan

## Overview
Add customer tracking to all tasks, allowing each task to be associated with a customer. This aligns with the business requirement that tasks represent work to be done or sold to specific customers.

## Business Requirements
- Every task can be linked to a customer (optional field)
- Customer field should be prominent and easy to fill
- Users should be able to filter/search tasks by customer
- Customer data should sync with Work Orders when applicable
- Support autocomplete for existing customer names
- Display customer prominently on task cards and detail views

---

## Architecture Analysis

### Current State
1. **Task Model**: No customer field exists
2. **WorkOrderHeader Model**: Already has `Customer` field (nullable string, 200 chars)
3. **DTOs**: CreateTaskRequest and UpdateTaskRequest need updates
4. **UI Components**: TaskCard, CreateTask, TaskDetail need customer support

### Design Decision: Simple String Field (MVP Approach)

**Rationale:**
- ✅ Quick to implement and test
- ✅ Consistent with existing WorkOrderHeader.Customer design
- ✅ Flexible - allows free-form customer names
- ✅ Can be normalized to separate Customer entity in future if needed
- ✅ No complex foreign key relationships initially

**Alternative Considered:**
- Separate Customer entity with normalization (deferred to Phase 2 if needed)

---

## Implementation Plan

### Phase 1: Database & Model Layer
**Goal:** Add customer field to core data model

#### 1.1 Update Task Model
**File:** `MiniJira/Models/Task.cs`

```csharp
// Add after line 22 (after Priority)
/// <summary>
/// Customer name - who this work is for
/// </summary>
[StringLength(200)]
public string? Customer { get; set; }
```

**Impact:**
- Database schema change required
- Migration needed

#### 1.2 Create Database Migration
**Command:**
```bash
dotnet ef migrations add AddCustomerToTask --project MiniJira
```

**Expected Changes:**
- Add nullable `Customer` column to `Tasks` table
- MaxLength: 200 characters

#### 1.3 Update DTOs

**File:** `MiniJira/Services/DTOs/CreateTaskRequest.cs`
```csharp
// Add after Priority property
[StringLength(200)]
public string? Customer { get; set; }
```

**File:** `MiniJira/Services/DTOs/UpdateTaskRequest.cs`
```csharp
// Add after Priority property
[StringLength(200)]
public string? Customer { get; set; }
```

**File:** Check if TaskDto exists and update similarly

---

### Phase 2: Service Layer
**Goal:** Update business logic to handle customer field

#### 2.1 Update TaskService
**File:** `MiniJira/Services/TaskService.cs`

**Changes needed:**
- In `CreateTaskAsync`: Map `request.Customer` to `task.Customer`
- In `UpdateTaskAsync`: Map `request.Customer` to `task.Customer`
- No additional validation needed (nullable field)

#### 2.2 Add Customer Autocomplete Service
**File:** `MiniJira/Services/ITaskService.cs` (new method)

```csharp
Task<List<string>> GetDistinctCustomersAsync();
```

**File:** `MiniJira/Services/TaskService.cs` (implementation)

```csharp
public async Task<List<string>> GetDistinctCustomersAsync()
{
    return await _context.Tasks
        .Where(t => !t.IsDeleted && !string.IsNullOrEmpty(t.Customer))
        .Select(t => t.Customer!)
        .Distinct()
        .OrderBy(c => c)
        .Take(100) // Limit for performance
        .ToListAsync();
}
```

---

### Phase 3: UI Components
**Goal:** Add customer field to all user-facing interfaces

#### 3.1 Create Task Page
**File:** `MiniJira/Components/Pages/CreateTask.razor`

**Add after priority field (around line 61):**

```razor
<div id="form-group-customer" class="form-group">
    <label for="Customer" class="form-label">
        @Localizer["CreateTask.Customer"]
    </label>
    <InputText
        id="Customer"
        name="Customer"
        class="form-control"
        @bind-Value="model.Customer"
        placeholder="@Localizer["CreateTask.CustomerPlaceholder"]"
        title="@Localizer["Tooltip.CustomerInput"]"
        list="customer-suggestions" />

    @* Autocomplete datalist *@
    <datalist id="customer-suggestions">
        @foreach (var customer in existingCustomers)
        {
            <option value="@customer" />
        }
    </datalist>

    <ValidationMessage For="@(() => model.Customer)" class="validation-message" />
</div>
```

**File:** `MiniJira/Components/Pages/CreateTask.razor.cs`

```csharp
private List<string> existingCustomers = new();

protected override async Task OnInitializedAsync()
{
    existingCustomers = await TaskService.GetDistinctCustomersAsync();
}
```

#### 3.2 Task Detail Page
**File:** `MiniJira/Components/Pages/TaskDetail.razor`

**Add customer field in edit mode** (find form-row divs and add):

```razor
<div class="form-row">
    <div class="form-group">
        <label for="customer" class="form-label">
            @Localizer["TaskDetail.Customer"]
        </label>
        <InputText
            id="customer"
            class="form-control"
            @bind-Value="editModel.Customer"
            placeholder="@Localizer["TaskDetail.CustomerPlaceholder"]"
            list="customer-suggestions-detail" />

        <datalist id="customer-suggestions-detail">
            @foreach (var customer in existingCustomers)
            {
                <option value="@customer" />
            }
        </datalist>
    </div>
</div>
```

**Add in view mode** (display customer read-only):

```razor
@if (!string.IsNullOrEmpty(task.Customer))
{
    <div class="task-detail-field">
        <div class="field-label">@Localizer["TaskDetail.Customer"]</div>
        <div class="field-value">
            <svg class="field-icon" width="16" height="16" viewBox="0 0 16 16" fill="currentColor">
                <!-- Customer icon -->
                <path d="M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm0 1c-3.315 0-6 2.015-6 4.5V15h12v-1.5c0-2.485-2.685-4.5-6-4.5z"/>
            </svg>
            @task.Customer
        </div>
    </div>
}
```

#### 3.3 Task Card Component
**File:** `MiniJira/Components/Shared/TaskCard.razor`

**Add customer display under title:**

```razor
@if (!string.IsNullOrEmpty(Task.Customer))
{
    <div id="task-card-customer-@Task.Id" class="task-card-customer">
        <svg class="customer-icon" width="14" height="14" viewBox="0 0 16 16" fill="currentColor">
            <path d="M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm0 1c-3.315 0-6 2.015-6 4.5V15h12v-1.5c0-2.485-2.685-4.5-6-4.5z"/>
        </svg>
        <span class="customer-name">@Task.Customer</span>
    </div>
}
```

**File:** `MiniJira/Components/Shared/TaskCard.razor.css`

```css
.task-card-customer {
    display: flex;
    align-items: center;
    gap: 4px;
    font-size: calc(11px * var(--font-size-multiplier));
    color: var(--jira-text-secondary);
    margin-top: 4px;
    padding: 2px 0;
}

.customer-icon {
    flex-shrink: 0;
    opacity: 0.7;
}

.customer-name {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

[data-theme="dark"] .task-card-customer {
    color: var(--jira-dn200);
}
```

---

### Phase 4: Localization
**Goal:** Add translations for customer field

#### 4.1 English Localization
**File:** `MiniJira/Resources/Localization.resx`

Add entries:
```xml
CreateTask.Customer = Customer
CreateTask.CustomerPlaceholder = Enter customer name (optional)
TaskDetail.Customer = Customer
TaskDetail.CustomerPlaceholder = Customer name
Tooltip.CustomerInput = Enter the customer name for this task
```

#### 4.2 Croatian Localization
**File:** `MiniJira/Resources/Localization.hr-HR.resx`

Add entries:
```xml
CreateTask.Customer = Kupac
CreateTask.CustomerPlaceholder = Unesite ime kupca (opcionalno)
TaskDetail.Customer = Kupac
TaskDetail.CustomerPlaceholder = Ime kupca
Tooltip.CustomerInput = Unesite ime kupca za ovaj zadatak
```

---

### Phase 5: Enhanced Features (Optional)

#### 5.1 Customer Filter on Board
**File:** `MiniJira/Components/Pages/Board.razor`

Add customer filter dropdown:
```razor
<div class="board-filters">
    <!-- Existing filters -->

    <select @bind="selectedCustomer" @bind:after="FilterTasksByCustomer">
        <option value="">@Localizer["Board.AllCustomers"]</option>
        @foreach (var customer in distinctCustomers)
        {
            <option value="@customer">@customer</option>
        }
    </select>
</div>
```

#### 5.2 WorkOrder Integration
**When creating WorkOrder from Task:**
- Pre-populate WorkOrder.Customer from Task.Customer
- Add option to update Task.Customer when WorkOrder.Customer changes

**File:** `MiniJira/Services/WorkOrderService.cs`

```csharp
// In CreateWorkOrderAsync
if (task.Customer != null && string.IsNullOrEmpty(workOrder.Customer))
{
    workOrder.Customer = task.Customer;
}
```

#### 5.3 Reports Enhancement
Add customer column to reports/task lists

---

## Testing Plan

### Unit Tests
1. Test Task creation with customer
2. Test Task update with customer
3. Test GetDistinctCustomersAsync returns unique values
4. Test customer field validation (max length 200)

### Integration Tests
1. Test migration applies successfully
2. Test customer persists to database
3. Test autocomplete loads existing customers

### UI Tests
1. Verify customer field appears on create task form
2. Verify customer displays on task card
3. Verify customer shows in task detail view
4. Verify autocomplete suggestions work
5. Test dark mode styling for customer elements

### Manual Testing Checklist
- [ ] Create task with customer
- [ ] Create task without customer
- [ ] Edit task to add customer
- [ ] Edit task to change customer
- [ ] Edit task to remove customer
- [ ] Verify autocomplete shows previous customers
- [ ] Check customer displays on task card
- [ ] Verify customer in task detail (view mode)
- [ ] Test customer field in dark mode
- [ ] Create work order - verify customer sync
- [ ] Test with very long customer names (200 chars)
- [ ] Test with special characters in customer name

---

## Database Migration Strategy

### Development
```bash
cd MiniJira
dotnet ef migrations add AddCustomerToTask
dotnet ef database update
```

### Production
- Migration is non-breaking (adds nullable column)
- No data migration needed
- Safe to deploy with zero downtime

---

## Rollback Plan

If issues arise:
1. Remove customer field from UI components
2. Keep database column (nullable, no impact)
3. Revert DTO changes
4. Or: Create down migration to remove column

---

## Success Metrics

1. ✅ All existing tasks remain functional
2. ✅ New tasks can be created with/without customer
3. ✅ Customer autocomplete suggests previous customers
4. ✅ Customer visible on task cards and detail views
5. ✅ No performance degradation
6. ✅ Dark mode compatibility
7. ✅ Localization in English and Croatian

---

## Timeline Estimate

| Phase | Estimated Time | Dependencies |
|-------|---------------|--------------|
| Phase 1: Database & Model | 30 minutes | None |
| Phase 2: Service Layer | 30 minutes | Phase 1 |
| Phase 3: UI Components | 2 hours | Phase 2 |
| Phase 4: Localization | 30 minutes | Phase 3 |
| Phase 5: Enhanced Features | 2 hours | Phase 4 (Optional) |
| Testing | 1 hour | All phases |
| **Total (Core)** | **4.5 hours** | |
| **Total (with enhancements)** | **6.5 hours** | |

---

## Future Enhancements (Post-MVP)

1. **Customer Management Module**
   - Separate Customer entity with normalization
   - Customer details page (contact info, history)
   - Customer search/browse interface

2. **Customer Analytics**
   - Tasks per customer report
   - Customer value metrics
   - Timeline of customer engagement

3. **Advanced Autocomplete**
   - Fuzzy search
   - Recently used customers prioritized
   - Customer aliases support

4. **Integration**
   - Import customers from external systems
   - Export customer task data
   - API endpoints for customer operations

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Migration fails | Low | High | Test in dev environment first |
| Performance impact from autocomplete | Low | Medium | Limit results to 100, add caching |
| Customer name conflicts | Low | Low | Free-form text allows duplicates (acceptable for MVP) |
| Dark mode styling issues | Low | Low | Test thoroughly in dark mode |
| Localization missing keys | Medium | Low | Add fallback to English |

---

## Notes

- Customer field is **optional** - tasks without customers are valid
- Customer is **free-form text** - no validation beyond max length
- **Autocomplete** improves UX but doesn't enforce strict values
- **Case-sensitive** storage (can be changed to case-insensitive later)
- **WorkOrder sync** is one-way initially (Task → WorkOrder on creation)

---

## Approval & Sign-off

- [ ] Database schema approved
- [ ] UI/UX design approved
- [ ] Localization strings approved
- [ ] Testing plan approved
- [ ] Ready for implementation

---

**Document Version:** 1.0
**Last Updated:** 2025-10-15
**Author:** Claude Code
**Status:** Ready for Review
