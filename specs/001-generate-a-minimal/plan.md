
# Implementation Plan: Minimal Blazor Project Management Application

**Branch**: `001-generate-a-minimal` | **Date**: 2025-10-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-generate-a-minimal/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code, or `AGENTS.md` for all other agents).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
Build a minimal project management application using .NET Core Blazor that provides basic Jira-like functionality. The system enables users to create tasks with title, description, priority, and status, display a list of all tasks, and update task statuses through an intuitive web interface. The application will persist data across sessions and support up to 1,000 tasks efficiently.

## Technical Context
**Language/Version**: C# with .NET 8.0 (LTS) or .NET 9.0
**Primary Dependencies**: ASP.NET Core Blazor Server (for interactive UI components)
**Storage**: Entity Framework Core with SQLite (for simple file-based persistence)
**Testing**: xUnit with bUnit (for Blazor component testing)
**Target Platform**: Web browser (modern browsers supporting WebSockets for Blazor Server)
**Project Type**: web (Blazor Server application with backend services)
**Performance Goals**: Support 1,000 tasks displayed efficiently, page load < 2 seconds
**Constraints**: Minimal setup, single-user focused (no authentication/authorization in MVP), last-write-wins for concurrent updates
**Scale/Scope**: MVP with 3 core pages (task list, create task, task detail), ~10-15 components, basic CRUD operations

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: ✅ PASS (Constitution template is not yet populated with project-specific principles)

Note: The constitution file at `.specify/memory/constitution.md` contains only placeholder templates. Once project principles are established, this plan should be reviewed against them. Current design follows general best practices:
- Blazor components will be self-contained and testable
- Services will use dependency injection for testability
- Data persistence through EF Core provides abstraction
- Test-first approach will be followed (xUnit + bUnit)

## Project Structure

### Documentation (this feature)
```
specs/[###-feature]/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
MiniJira/                          # Blazor Server project root
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core DbContext
│   └── Migrations/               # Database migrations
├── Models/
│   ├── Task.cs                   # Task entity
│   └── TaskStatus.cs             # Status enum
├── Services/
│   ├── ITaskService.cs           # Task service interface
│   └── TaskService.cs            # Task service implementation
├── Components/
│   ├── Pages/
│   │   ├── Index.razor           # Task list page
│   │   ├── CreateTask.razor      # Create task page
│   │   └── TaskDetail.razor      # Task detail/edit page
│   ├── Shared/
│   │   ├── TaskCard.razor        # Task display component
│   │   └── StatusBadge.razor     # Status display component
│   └── Layout/
│       └── MainLayout.razor      # Main layout
├── Program.cs                     # Application entry point
└── appsettings.json              # Configuration

MiniJira.Tests/
├── Unit/
│   ├── Services/
│   │   └── TaskServiceTests.cs  # Unit tests for TaskService
│   └── Models/
│       └── TaskTests.cs          # Model validation tests
├── Integration/
│   └── TaskWorkflowTests.cs      # End-to-end workflow tests
└── Components/
    ├── TaskCardTests.cs           # Component tests using bUnit
    └── Pages/
        ├── IndexTests.cs          # Task list page tests
        └── CreateTaskTests.cs     # Create task page tests
```

**Structure Decision**: Blazor Server single-project structure with integrated backend. This is optimal for the minimal MVP as it:
- Avoids unnecessary separation between frontend/backend (both are C# in Blazor Server)
- Simplifies deployment (single application)
- Reduces complexity while maintaining testability
- Follows standard Blazor Server conventions

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

2. **Generate and dispatch research agents**:
   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Follow Test-Driven Development (TDD) approach:
  1. Contract tests first (service interface tests)
  2. Model/Entity creation tasks
  3. Service implementation to make contract tests pass
  4. Component tests
  5. Component implementation to make tests pass
  6. Integration tests for user workflows
  7. Documentation and refinement

**Task Categories**:

1. **Foundation Tasks** (1-5):
   - Setup project structure
   - Configure EF Core and SQLite
   - Create enums (TaskStatus, TaskPriority)
   - Create Task entity model
   - Create ApplicationDbContext

2. **Service Layer Tasks** (6-15):
   - Create DTOs (TaskDto, CreateTaskRequest, UpdateTaskRequest, Result<T>)
   - Write ITaskService contract tests (failing) [P]
   - Implement ITaskService interface
   - Implement TaskService class
   - Verify all service tests pass

3. **UI Component Tasks** (16-30):
   - Create StatusBadge component + tests [P]
   - Create TaskCard component + tests [P]
   - Create Index page (task list) + tests
   - Create CreateTask page + tests
   - Create TaskDetail page + tests
   - Implement Virtualize for task list (1000 tasks)
   - Add form validation and error handling

4. **Integration & Polish Tasks** (31-35):
   - Write end-to-end workflow tests
   - Implement delete functionality with confirmation
   - Add status filtering UI
   - Performance testing with 1000 tasks
   - Final validation per quickstart.md

**Ordering Strategy**:
- TDD strict: All tests written before implementation
- Dependency order: Data layer → Service layer → UI layer
- Mark [P] for parallel execution (independent components)
- Tests for each layer must pass before moving to next layer

**Task Dependencies**:
```
Foundation (1-5)
    ↓
Service Tests (6-10) → Service Implementation (11-15)
    ↓
Component Tests (16-25) → Component Implementation (26-30)
    ↓
Integration Tests & Polish (31-35)
```

**Estimated Output**: 35-40 numbered, dependency-ordered tasks in tasks.md

**Success Criteria for Phase 2 Completion**:
- All contract tests written and failing
- All tasks numbered and ordered by dependency
- Each task has clear acceptance criteria
- Parallel tasks marked with [P]
- TDD workflow enforced (test before implementation)

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [x] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented (none required)

**Deliverables**:
- [x] research.md - Technical decisions and rationale
- [x] data-model.md - Entity and database schema design
- [x] contracts/ITaskService.md - Service layer contract with tests
- [x] contracts/Components.md - UI component contracts with tests
- [x] quickstart.md - Step-by-step setup and validation guide
- [x] CLAUDE.md - Agent context file with project tech stack
- [x] plan.md - This file (implementation plan)
- [x] tasks.md - 50 numbered, dependency-ordered implementation tasks

---
*Based on Constitution v2.1.1 - See `/memory/constitution.md`*
