# Research: Minimal Blazor Project Management Application

**Feature**: 001-generate-a-minimal
**Date**: 2025-10-05

## Technology Stack Decisions

### 1. Blazor Server vs Blazor WebAssembly

**Decision**: Blazor Server

**Rationale**:
- Faster initial load time (no need to download .NET runtime to browser)
- Simpler deployment model (single server-side application)
- Better for CRUD applications with frequent server interactions
- Reduced client-side resource requirements
- Direct access to server resources and databases
- Minimal setup complexity aligns with MVP goals

**Alternatives Considered**:
- **Blazor WebAssembly**: Rejected because it adds complexity (client-side runtime, API separation required), larger initial download, and unnecessary for a simple task management tool
- **Blazor Hybrid (MAUI)**: Out of scope - targets desktop/mobile apps, not web

### 2. Data Persistence Strategy

**Decision**: Entity Framework Core with SQLite

**Rationale**:
- File-based database requires no separate database server setup
- EF Core provides clean abstraction over data access
- Built-in migration support for schema changes
- SQLite is included in .NET runtime, zero additional dependencies
- Excellent for development and small-scale deployments (<1000 tasks requirement)
- Easy to swap to SQL Server/PostgreSQL later if needed

**Alternatives Considered**:
- **In-Memory Collections**: Rejected - doesn't meet FR-004 (persist data across sessions)
- **JSON File Storage**: Rejected - lacks query capabilities, no transaction support, manual serialization
- **SQL Server/PostgreSQL**: Rejected for MVP - unnecessary complexity, requires separate database server

### 3. Testing Framework

**Decision**: xUnit + bUnit + Moq

**Rationale**:
- **xUnit**: Industry standard for .NET testing, excellent async support
- **bUnit**: Specialized library for testing Blazor components, renders components in test context
- **Moq**: Mature mocking framework for isolating dependencies in unit tests
- All three integrate seamlessly with .NET test infrastructure

**Alternatives Considered**:
- **NUnit/MSTest**: Both viable but xUnit has better support for modern async patterns
- **Manual component testing**: Not feasible - Blazor components require rendering context

### 4. State Management

**Decision**: Scoped Services with Dependency Injection (built-in)

**Rationale**:
- Blazor Server sessions are scoped per user connection
- .NET's built-in DI container is sufficient for simple state
- Service lifetime management (Scoped) provides isolation between users
- No additional state management library needed for MVP scope
- Aligns with FR-009 (last-write-wins) - no complex state synchronization

**Alternatives Considered**:
- **Fluxor (Redux pattern)**: Rejected - overkill for 3-page CRUD app, adds complexity
- **Blazor State Management libraries**: Rejected - unnecessary for simple service-based state

### 5. UI Component Approach

**Decision**: Custom Razor components with Bootstrap 5

**Rationale**:
- Bootstrap 5 included in Blazor templates by default
- Building custom components provides learning opportunity and control
- No external UI library licensing/version concerns
- Sufficient for MVP requirements (task cards, forms, status badges)

**Alternatives Considered**:
- **MudBlazor/Radzen**: Rejected - adds dependency, steeper learning curve, unnecessary for basic UI
- **Telerik/Syncfusion**: Rejected - commercial licenses, excessive features for MVP

## Architecture Patterns

### 6. Service Layer Pattern

**Decision**: Repository pattern via service abstraction (ITaskService)

**Rationale**:
- Separates business logic from data access
- Makes services easily mockable for testing
- Provides flexibility to change data access strategy
- Standard pattern in .NET applications

**Implementation**:
```csharp
ITaskService (interface) → TaskService (implementation) → DbContext
```

### 7. Validation Strategy

**Decision**: Data Annotations + FluentValidation for complex rules

**Rationale**:
- Data Annotations handle simple required/length validations (FR-006)
- Blazor EditForm has built-in support for Data Annotations
- FluentValidation for any complex business rules if needed
- Client and server-side validation support

### 8. Error Handling Approach

**Decision**: Try-catch in services + User-facing error messages

**Rationale**:
- Meets FR-009 (graceful error handling with clear messages)
- Service layer catches exceptions and returns Result<T> pattern
- UI displays user-friendly messages
- Background logging to console/file for debugging

**Implementation Pattern**:
```csharp
public async Task<Result<Task>> CreateTask(Task task)
{
    try
    {
        // business logic
        return Result.Success(task);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create task");
        return Result.Failure("Unable to create task. Please try again.");
    }
}
```

## Performance Considerations

### 9. Task List Rendering (1000 tasks requirement)

**Decision**: Virtualization with Blazor's Virtualize component

**Rationale**:
- Built-in Blazor component for efficient large list rendering
- Only renders visible items + buffer
- Handles 1000+ items efficiently (FR requirement: 1000 tasks)
- No additional dependencies

**Alternatives Considered**:
- **Pagination**: Considered but virtualization provides better UX for scrolling
- **Load on scroll**: Built into Virtualize component

### 10. Database Query Optimization

**Decision**: EF Core with explicit loading and indexes

**Rationale**:
- Add index on Status field for filtering
- Use AsNoTracking() for read-only queries
- Explicit Select() projections to minimize data transfer
- Sufficient for <1000 task scale

## Development Workflow

### 11. Migration Strategy

**Decision**: EF Core Migrations with version control

**Rationale**:
- Code-first approach aligns with development workflow
- Migrations tracked in git for reproducibility
- `dotnet ef migrations add` generates migration code
- `dotnet ef database update` applies migrations

### 12. Initial Project Setup

**Decision**: .NET CLI templates with modifications

**Commands**:
```bash
dotnet new blazorserver -n MiniJira
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet new xunit -n MiniJira.Tests
cd MiniJira.Tests && dotnet add package bUnit
```

## Security & Constraints

### 13. Authentication/Authorization

**Decision**: None for MVP

**Rationale**:
- Explicitly out of scope (per Technical Context constraints)
- Single-user focused application
- Can be added in future iteration using ASP.NET Core Identity

### 14. Concurrency Handling

**Decision**: Last-write-wins (no optimistic concurrency)

**Rationale**:
- Per spec resolution: "last successful update overwrites previous ones"
- Simplified for MVP, no conflict resolution UI needed
- EF Core default behavior (no concurrency tokens)

## Open Questions Resolved

All technical context items have been clarified:
- ✅ Language: C# with .NET 8.0/9.0
- ✅ Framework: Blazor Server
- ✅ Database: SQLite with EF Core
- ✅ Testing: xUnit + bUnit + Moq
- ✅ Performance: Virtualization for 1000 tasks
- ✅ Architecture: Service layer with DI
- ✅ Validation: Data Annotations + FluentValidation
- ✅ Error handling: Service-level with user messages

## Next Steps

Phase 0 complete. Ready for Phase 1:
1. Generate data model from Task and Status entities
2. Create service contracts (ITaskService interface)
3. Define component contracts
4. Write failing tests for all contracts
5. Create quickstart guide
