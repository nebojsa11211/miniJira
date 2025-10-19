# Tasks: Minimal Blazor Project Management Application

**Feature**: 001-generate-a-minimal
**Input**: Design documents from `/specs/001-generate-a-minimal/`
**Prerequisites**: plan.md, research.md, data-model.md, contracts/, quickstart.md

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → Extract: C# .NET 8.0/9.0, Blazor Server, EF Core + SQLite, xUnit + bUnit
2. Load design documents:
   → data-model.md: Task entity, TaskStatus, TaskPriority enums
   → contracts/ITaskService.md: Service layer contract with 7 methods
   → contracts/Components.md: 3 pages + 2 shared components
   → quickstart.md: Setup and validation scenarios
3. Generate tasks by category:
   → Foundation: Project setup, models, DbContext
   → Service Tests: Contract tests for ITaskService (TDD)
   → Service Implementation: DTOs, interface, implementation
   → Component Tests: bUnit tests for all 5 components (TDD)
   → Component Implementation: Razor components
   → Integration & Polish: E2E tests, performance, validation
4. Apply TDD ordering: Tests before implementation
5. Mark parallel tasks with [P] (different files, no dependencies)
6. Number tasks sequentially (T001-T038)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- All paths relative to project root: `MiniJira/` and `MiniJira.Tests/`

---

## Phase 3.1: Foundation & Setup

### Project Initialization
- [x] **T001** Create Blazor Server project structure: `dotnet new blazorserver -n MiniJira -o MiniJira`
- [x] **T002** Create test project: `dotnet new xunit -n MiniJira.Tests -o MiniJira.Tests` and add reference to main project
- [x] **T003** Create solution file and add both projects: `dotnet new sln -n MiniJira && dotnet sln add MiniJira/MiniJira.csproj MiniJira.Tests/MiniJira.Tests.csproj`
- [x] **T004** Add NuGet packages to MiniJira: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`
- [x] **T005** Add NuGet packages to MiniJira.Tests: `bUnit`, `bUnit.web`, `NSubstitute`, `FluentAssertions`

### Data Model Foundation
- [x] **T006** [P] Create TaskStatus enum in `MiniJira/Models/TaskStatus.cs` with values: ToDo=0, InProgress=1, Done=2
- [x] **T007** [P] Create TaskPriority enum in `MiniJira/Models/TaskPriority.cs` with values: Low=0, Medium=1, High=2
- [x] **T008** Create Task entity model in `MiniJira/Models/Task.cs` with properties: Id (Guid), Title (string, max 200), Description (string, max 5000), Status (TaskStatus), Priority (TaskPriority), CreatedAt (DateTime), UpdatedAt (DateTime), and DataAnnotations validation
- [x] **T009** Create ApplicationDbContext in `MiniJira/Data/ApplicationDbContext.cs` with DbSet<Task>, configure entity with indexes on Status and CreatedAt

### Database Setup
- [x] **T010** Configure DbContext in `MiniJira/Program.cs`: Add SQLite connection string "Data Source=minijira.db", register ApplicationDbContext as scoped service
- [x] **T011** Add database initialization in Program.cs: Create scope, get DbContext, call EnsureCreated() on startup
- [x] **T012** Create initial EF Core migration: `dotnet ef migrations add InitialCreate --project MiniJira`
- [x] **T013** Apply database migration: `dotnet ef database update --project MiniJira` (creates minijira.db)

---

## Phase 3.2: Service Layer - Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3

**CRITICAL: These tests MUST be written and MUST FAIL before ANY service implementation**

### DTOs (Required for Tests)
- [x] **T014** [P] Create TaskDto in `MiniJira/Services/DTOs/TaskDto.cs` with all 7 properties from Task entity
- [x] **T015** [P] Create CreateTaskRequest in `MiniJira/Services/DTOs/CreateTaskRequest.cs` with Title, Description, Priority and DataAnnotations validation
- [x] **T016** [P] Create UpdateTaskRequest in `MiniJira/Services/DTOs/UpdateTaskRequest.cs` with Title, Description, Status, Priority and validation
- [x] **T017** [P] Create Result<T> pattern classes in `MiniJira/Services/DTOs/Result.cs`: Result base class with IsSuccess and ErrorMessage, Result<T> with Value property, static Success/Failure factory methods

### Service Contract Tests (All Must Fail Initially)
- [x] **T018** [P] Create contract test for GetAllTasksAsync in `MiniJira.Tests/Unit/Services/TaskServiceTests_GetAll.cs`: Test empty list, test ordering by CreatedAt descending
- [x] **T019** [P] Create contract test for GetTasksByStatusAsync in `MiniJira.Tests/Unit/Services/TaskServiceTests_GetByStatus.cs`: Test filtering by each status
- [x] **T020** [P] Create contract test for GetTaskByIdAsync in `MiniJira.Tests/Unit/Services/TaskServiceTests_GetById.cs`: Test found task, test null when not found
- [x] **T021** [P] Create contract test for CreateTaskAsync in `MiniJira.Tests/Unit/Services/TaskServiceTests_Create.cs`: Test valid creation, test title empty validation failure, test title >200 chars failure, test description empty failure
- [x] **T022** [P] Create contract test for UpdateTaskAsync in `MiniJira.Tests/Unit/Services/TaskServiceTests_Update.cs`: Test successful update, test not found error, test validation failures
- [x] **T023** [P] Create contract test for UpdateTaskStatusAsync in `MiniJira.Tests/Unit/Services/TaskServiceTests_UpdateStatus.cs`: Test status change updates UpdatedAt, test not found error
- [x] **T024** [P] Create contract test for DeleteTaskAsync in `MiniJira.Tests/Unit/Services/TaskServiceTests_Delete.cs`: Test successful deletion, test not found error

### Verify Tests Fail
- [x] **T025** Run all service tests and confirm they fail: `dotnet test --filter TaskServiceTests` (expected: 15+ failing tests)

---

## Phase 3.3: Service Layer - Implementation (ONLY after tests are failing)

### Service Interface & Implementation
- [ ] **T026** Create ITaskService interface in `MiniJira/Services/ITaskService.cs` with 7 method signatures: GetAllTasksAsync, GetTasksByStatusAsync, GetTaskByIdAsync, CreateTaskAsync, UpdateTaskAsync, UpdateTaskStatusAsync, DeleteTaskAsync
- [ ] **T027** Create TaskService implementation in `MiniJira/Services/TaskService.cs`: Implement all 7 methods with EF Core queries, AsNoTracking for reads, proper error handling with Result<T> pattern, MapToDto helper method
- [ ] **T028** Register ITaskService in Program.cs: `builder.Services.AddScoped<ITaskService, TaskService>()`
- [ ] **T029** Verify all service tests pass: `dotnet test --filter TaskServiceTests` (expected: all 15+ tests pass)

---

## Phase 3.4: UI Components - Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.5

**CRITICAL: Component tests MUST be written and MUST FAIL before component implementation**

### Shared Component Tests
- [ ] **T030** [P] Create StatusBadge component tests in `MiniJira.Tests/Components/StatusBadgeTests.cs`: Test rendering for each TaskStatus (ToDo→gray, InProgress→blue, Done→green), test correct CSS classes applied
- [ ] **T031** [P] Create TaskCard component tests in `MiniJira.Tests/Components/TaskCardTests.cs`: Test task details rendering, test description truncation (>100 chars), test OnClick event callback invocation

### Page Component Tests
- [ ] **T032** Create Index page tests in `MiniJira.Tests/Components/Pages/IndexTests.cs`: Test loading spinner displays initially, test tasks render after load, test filter by status functionality, test navigation to create page, test navigation to detail page on card click
- [ ] **T033** Create CreateTask page tests in `MiniJira.Tests/Components/Pages/CreateTaskTests.cs`: Test empty form initial state, test validation errors (empty title/description), test successful task creation navigates to index, test cancel button navigation
- [ ] **T034** Create TaskDetail page tests in `MiniJira.Tests/Components/Pages/TaskDetailTests.cs`: Test task loads and displays, test task not found error, test edit mode toggle, test status quick-change buttons, test save/cancel edit, test delete with confirmation

### Verify Component Tests Fail
- [ ] **T035** Run all component tests and confirm they fail: `dotnet test --filter "Components"` (expected: 20+ failing tests)

---

## Phase 3.5: UI Components - Implementation (ONLY after tests are failing)

### Shared Components
- [ ] **T036** [P] Implement StatusBadge component in `MiniJira/Components/Shared/StatusBadge.razor`: Accept TaskStatus parameter, render badge with color (ToDo=bg-secondary, InProgress=bg-primary, Done=bg-success), enum to display text conversion
- [ ] **T037** [P] Implement TaskCard component in `MiniJira/Components/Shared/TaskCard.razor`: Accept TaskDto and OnClick parameters, render card with title/description (truncate at 100 chars)/status badge/priority badge/created date, make clickable

### Page Components
- [ ] **T038** Implement Index page in `MiniJira/Components/Pages/Index.razor`: Inject ITaskService, load tasks in OnInitializedAsync, implement status filter buttons, use Virtualize component for task list (1000 tasks support), render TaskCard for each task with click navigation, add Create Task button
- [ ] **T039** Implement CreateTask page in `MiniJira/Components/Pages/CreateTask.razor`: Inject ITaskService and NavigationManager, create EditForm with CreateTaskRequest model, add input validation (DataAnnotations), implement OnValidSubmit to create task and navigate to index, implement OnInvalidSubmit to show errors, add Cancel button
- [ ] **T040** Implement TaskDetail page in `MiniJira/Components/Pages/TaskDetail.razor`: Inject ITaskService and NavigationManager, accept Id parameter, load task in OnInitializedAsync, implement display mode (read-only) and edit mode toggle, implement status quick-change buttons, implement edit form with UpdateTaskRequest, implement delete with confirmation dialog, add back to list navigation

### Verify Component Tests Pass
- [ ] **T041** Run all component tests and confirm they pass: `dotnet test --filter "Components"` (expected: all 20+ tests pass)

---

## Phase 3.6: Integration & Polish

### Integration Tests
- [ ] **T042** [P] Create end-to-end task creation workflow test in `MiniJira.Tests/Integration/TaskWorkflowTests_Create.cs`: Test complete flow from Index → CreateTask → save → back to Index with new task visible
- [ ] **T043** [P] Create end-to-end task update workflow test in `MiniJira.Tests/Integration/TaskWorkflowTests_Update.cs`: Test flow from Index → TaskDetail → edit → save → verify changes
- [ ] **T044** [P] Create end-to-end status change workflow test in `MiniJira.Tests/Integration/TaskWorkflowTests_Status.cs`: Test status quick-change from TaskDetail, verify status updates in list

### Performance & Validation
- [ ] **T045** Create performance test with 1000 tasks in `MiniJira.Tests/Integration/PerformanceTests.cs`: Seed 1000 tasks, measure Index page load time (must be <2 seconds), verify Virtualize component working, test smooth scrolling
- [ ] **T046** Run quickstart validation checklist from `specs/001-generate-a-minimal/quickstart.md`: Execute all 8 manual validation tests (empty list, create task, view details, update status, edit task, form validation, multiple tasks, persistence), document results

### Final Touches
- [ ] **T047** [P] Add delete task functionality: Implement delete button in TaskDetail with confirmation dialog, update ITaskService with DeleteTaskAsync (already exists from contracts), add delete test
- [ ] **T048** [P] Add status filtering to Index page UI: Add filter buttons (All/ToDo/InProgress/Done) above task list, update filteredTasks based on selection
- [ ] **T049** Verify all tests pass: `dotnet test` (expected: 40+ tests all passing)
- [ ] **T050** Build and run application: `dotnet build && dotnet run --project MiniJira`, verify app runs on https://localhost:5001, perform smoke test of all features

---

## Dependencies

### Layer Dependencies
```
Foundation (T001-T013)
    ↓
Service DTOs (T014-T017) → Service Tests (T018-T025)
    ↓                            ↓
Service Implementation (T026-T029) ← Tests must fail first
    ↓
Component Tests (T030-T035)
    ↓                     ↓
Component Implementation (T036-T041) ← Tests must fail first
    ↓
Integration Tests (T042-T046)
    ↓
Polish & Validation (T047-T050)
```

### Critical Blockers
- **T013** (DB setup) blocks all subsequent tasks
- **T014-T017** (DTOs) required before T018-T025 (service tests)
- **T025** (verify tests fail) blocks T026-T029 (implementation)
- **T029** (service tests pass) blocks T030-T035 (component tests)
- **T035** (verify tests fail) blocks T036-T041 (implementation)
- **T041** (component tests pass) blocks T042-T046 (integration tests)

---

## Parallel Execution Examples

### Phase 3.1 Parallel: Model Files (After T005)
```bash
# Can run T006, T007 together (different files):
Task: "Create TaskStatus enum in MiniJira/Models/TaskStatus.cs"
Task: "Create TaskPriority enum in MiniJira/Models/TaskPriority.cs"
```

### Phase 3.2 Parallel: DTOs (After T013)
```bash
# Can run T014-T017 together (different files):
Task: "Create TaskDto in MiniJira/Services/DTOs/TaskDto.cs"
Task: "Create CreateTaskRequest in MiniJira/Services/DTOs/CreateTaskRequest.cs"
Task: "Create UpdateTaskRequest in MiniJira/Services/DTOs/UpdateTaskRequest.cs"
Task: "Create Result<T> in MiniJira/Services/DTOs/Result.cs"
```

### Phase 3.2 Parallel: Service Tests (After T017)
```bash
# Can run T018-T024 together (different test files):
Task: "Contract test GetAllTasksAsync in MiniJira.Tests/Unit/Services/TaskServiceTests_GetAll.cs"
Task: "Contract test GetTasksByStatusAsync in MiniJira.Tests/Unit/Services/TaskServiceTests_GetByStatus.cs"
Task: "Contract test GetTaskByIdAsync in MiniJira.Tests/Unit/Services/TaskServiceTests_GetById.cs"
Task: "Contract test CreateTaskAsync in MiniJira.Tests/Unit/Services/TaskServiceTests_Create.cs"
Task: "Contract test UpdateTaskAsync in MiniJira.Tests/Unit/Services/TaskServiceTests_Update.cs"
Task: "Contract test UpdateTaskStatusAsync in MiniJira.Tests/Unit/Services/TaskServiceTests_UpdateStatus.cs"
Task: "Contract test DeleteTaskAsync in MiniJira.Tests/Unit/Services/TaskServiceTests_Delete.cs"
```

### Phase 3.4 Parallel: Component Tests (After T029)
```bash
# Can run T030-T031 together (different files):
Task: "Create StatusBadge tests in MiniJira.Tests/Components/StatusBadgeTests.cs"
Task: "Create TaskCard tests in MiniJira.Tests/Components/TaskCardTests.cs"
```

### Phase 3.5 Parallel: Shared Components (After T035)
```bash
# Can run T036-T037 together (different files):
Task: "Implement StatusBadge in MiniJira/Components/Shared/StatusBadge.razor"
Task: "Implement TaskCard in MiniJira/Components/Shared/TaskCard.razor"
```

### Phase 3.6 Parallel: Integration Tests (After T041)
```bash
# Can run T042-T044 together (different files):
Task: "E2E create workflow test in MiniJira.Tests/Integration/TaskWorkflowTests_Create.cs"
Task: "E2E update workflow test in MiniJira.Tests/Integration/TaskWorkflowTests_Update.cs"
Task: "E2E status change test in MiniJira.Tests/Integration/TaskWorkflowTests_Status.cs"
```

### Phase 3.6 Parallel: Final Polish (After T046)
```bash
# Can run T047-T048 together (different concerns):
Task: "Add delete functionality in TaskDetail.razor"
Task: "Add status filtering UI in Index.razor"
```

---

## Notes

### TDD Enforcement
- ✅ All service tests (T018-T024) before implementation (T026-T027)
- ✅ All component tests (T030-T034) before implementation (T036-T040)
- ✅ Verify tests FAIL (T025, T035) before proceeding
- ✅ Verify tests PASS (T029, T041) before next phase

### Parallel Execution Rules
- [P] tasks = different files, no shared state
- Same component/service = sequential (no [P])
- Tests can be parallel if testing different contracts
- Implementation sequential within same layer

### File Path Conventions
- Models: `MiniJira/Models/`
- Data: `MiniJira/Data/`
- Services: `MiniJira/Services/` and `MiniJira/Services/DTOs/`
- Components: `MiniJira/Components/Pages/` and `MiniJira/Components/Shared/`
- Tests: `MiniJira.Tests/Unit/`, `MiniJira.Tests/Components/`, `MiniJira.Tests/Integration/`

### Success Criteria
- All 40+ tests passing
- Application builds without errors
- Manual validation checklist complete
- Performance requirement met (1000 tasks, <2s load)
- All 10 functional requirements from spec.md satisfied

---

## Validation Checklist
*GATE: Check before marking tasks complete*

- [x] All ITaskService methods have contract tests (T018-T024)
- [x] All entities have model tasks (Task entity in T008)
- [x] All tests come before implementation (T018-T025 → T026-T029, T030-T035 → T036-T041)
- [x] Parallel tasks are truly independent (verified: different files)
- [x] Each task specifies exact file path
- [x] No [P] task modifies same file as another [P] task
- [x] TDD workflow enforced with verification gates (T025, T035)
- [x] All 5 components (3 pages + 2 shared) have tests and implementation
- [x] Integration tests cover main user workflows
- [x] Performance validation included (T045)
- [x] Quickstart validation included (T046)
