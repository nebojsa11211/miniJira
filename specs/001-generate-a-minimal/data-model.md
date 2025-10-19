# Data Model: Minimal Blazor Project Management Application

**Feature**: 001-generate-a-minimal
**Date**: 2025-10-05

## Entity Definitions

### Task Entity

Represents a work item in the project management system.

**Properties**:
- `Id` (Guid): Unique identifier, primary key
- `Title` (string): Short title/summary of the task
  - Required: Yes
  - Max length: 200 characters
  - Validation: Cannot be empty or whitespace
- `Description` (string): Full detailed description of the task
  - Required: Yes
  - Max length: 5000 characters
  - Validation: Cannot be empty or whitespace
- `Status` (TaskStatus enum): Current state of the task
  - Required: Yes
  - Default: ToDo
  - Values: ToDo, InProgress, Done
- `Priority` (TaskPriority enum): Importance level
  - Required: Yes
  - Default: Medium
  - Values: Low, Medium, High
- `CreatedAt` (DateTime): Timestamp when task was created
  - Required: Yes
  - Default: UTC now on creation
  - Immutable after creation
- `UpdatedAt` (DateTime): Timestamp of last modification
  - Required: Yes
  - Default: UTC now on creation
  - Updated automatically on any change

**Relationships**: None (single entity system for MVP)

**Indexes**:
- Primary key on `Id` (default)
- Non-clustered index on `Status` (for filtering by status)
- Non-clustered index on `CreatedAt` (for sorting by date)

### TaskStatus Enum

Represents the lifecycle state of a task.

**Values**:
- `ToDo = 0`: Task is planned but not started
- `InProgress = 1`: Task is actively being worked on
- `Done = 2`: Task is completed

**Valid Transitions**:
- ToDo → InProgress
- ToDo → Done (allow skipping InProgress)
- InProgress → Done
- InProgress → ToDo (allow moving back)
- Done → InProgress (allow reopening)
- Done → ToDo (allow reopening)

Note: All transitions are permitted for MVP (no workflow restrictions).

### TaskPriority Enum

Represents the importance/urgency level of a task.

**Values**:
- `Low = 0`: Can be deferred
- `Medium = 1`: Normal priority
- `High = 2`: Needs immediate attention

**Usage**: Display only, no business logic implications in MVP.

## Database Schema

### Table: Tasks

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | UNIQUEIDENTIFIER (Guid) | PRIMARY KEY | Auto-generated |
| Title | NVARCHAR(200) | NOT NULL | Indexed for search (future) |
| Description | NVARCHAR(5000) | NOT NULL | Full text |
| Status | INT | NOT NULL, DEFAULT 0 | 0=ToDo, 1=InProgress, 2=Done |
| Priority | INT | NOT NULL, DEFAULT 1 | 0=Low, 1=Medium, 2=High |
| CreatedAt | DATETIME2 | NOT NULL | UTC timestamp |
| UpdatedAt | DATETIME2 | NOT NULL | UTC timestamp |

**Indexes**:
```sql
CREATE INDEX IX_Tasks_Status ON Tasks(Status);
CREATE INDEX IX_Tasks_CreatedAt ON Tasks(CreatedAt DESC);
```

## Validation Rules

### Task Entity Validation

**Title Validation**:
- Required field
- Min length: 1 character (after trim)
- Max length: 200 characters
- Error message: "Title is required and must be between 1 and 200 characters"

**Description Validation**:
- Required field
- Min length: 1 character (after trim)
- Max length: 5000 characters
- Error message: "Description is required and must be between 1 and 5000 characters"

**Status Validation**:
- Must be valid enum value (0, 1, or 2)
- Error message: "Invalid status value"

**Priority Validation**:
- Must be valid enum value (0, 1, or 2)
- Error message: "Invalid priority value"

**Date Validation**:
- CreatedAt must be <= current UTC time
- UpdatedAt must be >= CreatedAt
- UpdatedAt must be <= current UTC time
- Error messages: Built-in EF Core validation

## State Transitions

### Task Status Lifecycle

```
┌─────────┐
│  ToDo   │────────┐
└─────────┘        │
     ↑ ↓           ↓
┌─────────┐    ┌──────┐
│InProgress│ ←──┤ Done │
└─────────┘    └──────┘
     ↓ ↑           ↑
     └─────────────┘
```

**Rules** (MVP - all transitions allowed):
- Any status can transition to any other status
- No validation of transition logic in MVP
- Future: May add workflow rules (e.g., Done → InProgress requires reason)

## Query Patterns

### Common Queries

**Get All Tasks** (FR-002):
```csharp
context.Tasks
    .AsNoTracking()
    .OrderByDescending(t => t.CreatedAt)
    .ToListAsync()
```

**Get Tasks by Status**:
```csharp
context.Tasks
    .AsNoTracking()
    .Where(t => t.Status == status)
    .OrderByDescending(t => t.CreatedAt)
    .ToListAsync()
```

**Get Single Task** (FR-010):
```csharp
context.Tasks
    .AsNoTracking()
    .FirstOrDefaultAsync(t => t.Id == id)
```

**Create Task** (FR-001):
```csharp
var task = new Task
{
    Id = Guid.NewGuid(),
    Title = dto.Title,
    Description = dto.Description,
    Status = TaskStatus.ToDo,
    Priority = dto.Priority,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};
context.Tasks.Add(task);
await context.SaveChangesAsync();
```

**Update Task Status** (FR-003):
```csharp
var task = await context.Tasks.FindAsync(id);
task.Status = newStatus;
task.UpdatedAt = DateTime.UtcNow;
await context.SaveChangesAsync();
```

**Update Task Details** (FR-010):
```csharp
var task = await context.Tasks.FindAsync(id);
task.Title = dto.Title;
task.Description = dto.Description;
task.Status = dto.Status;
task.Priority = dto.Priority;
task.UpdatedAt = DateTime.UtcNow;
await context.SaveChangesAsync();
```

## Performance Considerations

**For 1000 Tasks** (per spec requirement):

1. **Virtualization**: Use Blazor `Virtualize` component for list rendering
2. **Projection**: Select only needed fields in queries
3. **Pagination**: Optional for future (use skip/take with index on CreatedAt)
4. **Caching**: Not needed for MVP (<1000 records, SQLite is fast enough)

**Estimated Query Performance** (SQLite on SSD):
- Get all tasks: <50ms for 1000 records
- Get by status: <10ms (indexed)
- Get single task: <5ms (primary key)
- Insert/Update: <10ms

## Migration Strategy

### Initial Migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Migration File (EF Core generated):

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Title = table.Column<string>(maxLength: 200, nullable: false),
                Description = table.Column<string>(maxLength: 5000, nullable: false),
                Status = table.Column<int>(nullable: false, defaultValue: 0),
                Priority = table.Column<int>(nullable: false, defaultValue: 1),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tasks", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_Status",
            table: "Tasks",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_CreatedAt",
            table: "Tasks",
            column: "CreatedAt",
            descending: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Tasks");
    }
}
```

## EF Core Configuration

### DbContext Configuration

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(TaskStatus.ToDo);

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasDefaultValue(TaskPriority.Medium);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
```

## Data Seeding (Optional)

For development/testing, seed sample data:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... entity configuration ...

    modelBuilder.Entity<Task>().HasData(
        new Task
        {
            Id = Guid.NewGuid(),
            Title = "Setup project structure",
            Description = "Initialize Blazor project with necessary packages",
            Status = TaskStatus.Done,
            Priority = TaskPriority.High,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        },
        new Task
        {
            Id = Guid.NewGuid(),
            Title = "Implement task creation",
            Description = "Build UI and service for creating new tasks",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.High,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        },
        new Task
        {
            Id = Guid.NewGuid(),
            Title = "Add task filtering",
            Description = "Allow filtering tasks by status and priority",
            Status = TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    );
}
```
