# MiniJira Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-10-26

## Active Technologies
- C# with .NET 8.0 (LTS) or .NET 9.0 + ASP.NET Core Blazor Server (for interactive UI components) (001-generate-a-minimal)

## Project Structure
```
MiniJira/                    (Main Blazor Server project)
├── Components/             (Blazor components - Pages and Shared)
│   ├── Pages/             (Page components)
│   └── Shared/            (Reusable components: TaskCard, Tooltip, etc.)
├── Controllers/            (API endpoints)
│   ├── CultureController.cs     (Language switching)
│   └── TestDataController.cs    (Test data seeding API)
├── Data/                   (EF Core DbContext)
├── Models/                 (Domain models and enums)
├── Services/               (Business logic and DTOs)
│   ├── DataResetService.cs      (Data reset and test seeding)
│   ├── MobileMenuStateService.cs (Mobile menu state management)
│   ├── SidebarStateService.cs   (Sidebar collapse state)
│   ├── LocalizationService.cs   (Localization support)
│   └── ...                      (Task, Column, WorkOrder services)
├── Shared/                 (Layout components)
├── wwwroot/                (Static files - CSS, JS, images)
├── Resources/              (Localization .resx files)
├── Migrations/             (EF Core database migrations)
└── Properties/             (Project configuration)

MiniJira.Tests/            (Test project - currently empty)
```

## Commands

### Build and Run
```bash
# Build the project
dotnet build

# Run the application (from MiniJira directory)
cd MiniJira
dotnet run

# The app will be available at https://localhost:5001 or http://localhost:5000
```

### Database Migrations
```bash
# Apply pending migrations
dotnet ef database update --project MiniJira

# Create a new migration
dotnet ef migrations add <MigrationName> --project MiniJira

# Remove last migration (if not applied)
dotnet ef migrations remove --project MiniJira
```

### Testing
```bash
# Run tests (note: test project is currently empty)
dotnet test
```

## Code Style
- **C# Conventions**: Follow Microsoft C# coding conventions
- **Async/Await**: Use async methods for all I/O operations
- **Naming**: PascalCase for public members, camelCase for private fields
- **Components**: One component per file, use code-behind (.razor + .razor.cs) when logic is complex
- **CSS**: Use scoped styles (.razor.css) for component-specific styling
- **Services**: Register as Scoped for per-request lifetime

## Recent Changes
- 2025-10-26: Added test data seeding API and services (DataResetService, TestDataController)
- 2025-10-26: Added UI state management services (MobileMenuStateService, SidebarStateService)
- 2025-10-26: Added CultureController for language switching in Blazor Server
- 2025-10-26: Enhanced localization support and fixed font size persistence with culture-specific formatting
- 2025-10-26: Completed comprehensive Croatian localization
- 2025-10-19: Added comprehensive bilingual user guide with screenshots
- 001-generate-a-minimal: Added C# with .NET 8.0 (LTS) or .NET 9.0 + ASP.NET Core Blazor Server (for interactive UI components)

<!-- MANUAL ADDITIONS START -->
## Development Rules

### Background Process Management
- **ALWAYS** kill background processes after testing/debugging is complete
- When running `dotnet run` in background mode for testing, use `KillShell` tool to terminate the process when done
- Never leave development servers running in the background after completing tasks

### ⚠️ CRITICAL: Mock Authentication Notice
**This application uses MOCK authentication for development only.**

**DO NOT deploy to production without implementing real authentication.**

Mock components:
- `Services/MockUserService.cs` - Returns hardcoded test users
- `Services/CurrentUserService.cs` - Uses mock data
- `Components/Pages/UserSelection.razor` - Mock login screen

For production deployment, implement:
- ASP.NET Core Identity, OR
- OAuth 2.0 / OpenID Connect, OR
- Azure AD / Auth0 / IdentityServer

### Database
- **Database Provider**: SQLite (file-based)
- **Database File**: `minijira.db` (created automatically in project root)
- **Default Data**: System columns (ToDo, InProgress, Done) seeded with hardcoded GUIDs

### Localization
- **Supported Cultures**: en-US (English), hr-HR (Croatian)
- **Default Culture**: hr-HR (Croatian)
- **Resource Files**: `Resources/Localization.resx` and `Resources/Localization.hr-HR.resx`

### Testing Status
- **Automated Tests**: None currently implemented
- **Test Project**: Exists but empty (`MiniJira.Tests/`)
- **Testing**: Manual testing only at this time
<!-- MANUAL ADDITIONS END -->