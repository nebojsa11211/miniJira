# MiniJira - Blazor Task Management Application

A modern, feature-rich task management application built with ASP.NET Core Blazor Server, inspired by Jira.

## ⚠️ Development Status

**This is a development/demo application with MOCK authentication.**

**DO NOT deploy to production without implementing real authentication.**

## Features

### Core Task Management
- ✅ **Kanban Board** - Drag-and-drop task management across columns
- ✅ **Custom Columns** - Create and manage your own workflow columns
- ✅ **Task CRUD** - Create, read, update, and delete tasks
- ✅ **Priority Levels** - High, Medium, Low priority tracking
- ✅ **Task Status** - To Do, In Progress, Done (and custom statuses)
- ✅ **Customer Field** - Track customer information per task
- ✅ **Task Assignment** - Assign tasks to users
- ✅ **Task Hiding** - Archive completed tasks

### Advanced Features
- ✅ **Work Orders (Radni Nalog)** - Specialized manufacturing/construction work order management
  - 30 editable rows with automatic calculations
  - Excel export/import with professional formatting
  - CSV export
  - Automatic calculations: Square meters, Linear meters, Cubic meters, Weight
  - Running totals and grouping by thickness
- ✅ **Comments** - Add comments to tasks
- ✅ **File Attachments** - Upload and manage task attachments
- ✅ **History Tracking** - Track column movements and assignment changes
- ✅ **Filtering & Sorting** - Filter by priority/assignee, sort by multiple criteria
- ✅ **Localization** - English and Croatian language support

### UI/UX
- ✅ **Modern Design** - Professional Atlassian-inspired design system
- ✅ **Dark Mode** - Full dark theme support
- ✅ **Responsive** - Mobile-first design, works on all devices
- ✅ **Accessibility** - WCAG 2.1 AA compliant
- ✅ **Font Size Adjustment** - Accessibility feature for better readability

## Technology Stack

- **Framework**: ASP.NET Core 8.0 (LTS)
- **UI**: Blazor Server
- **Database**: SQLite (Entity Framework Core 9.0)
- **Excel**: ClosedXML 0.105.0
- **Logging**: Serilog 9.0
- **Styling**: Custom CSS + Bootstrap 5

## Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- (Optional) Visual Studio 2022 or VS Code

### Installation

1. **Clone or download the repository**
   ```bash
   cd MiniJira
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update --project MiniJira
   ```

   This creates the SQLite database (`minijira.db`) with the required schema.

4. **Run the application**
   ```bash
   cd MiniJira
   dotnet run
   ```

5. **Open your browser**
   - Navigate to: `https://localhost:5001` or `http://localhost:5000`
   - Select a mock user from the user selection screen

## Project Structure

```
MiniJira/                    (Main Blazor Server project)
├── Components/             (Blazor components)
│   ├── Pages/             (Page components)
│   └── Shared/            (Reusable components)
├── Controllers/            (API endpoints)
├── Data/                   (EF Core DbContext)
├── Models/                 (Domain models)
├── Services/               (Business logic)
│   ├── DTOs/              (Data Transfer Objects)
│   ├── ITaskService.cs
│   ├── TaskService.cs
│   ├── IWorkOrderService.cs
│   └── WorkOrderService.cs
├── Shared/                 (Layout components)
├── wwwroot/                (Static files - CSS, JS)
├── Resources/              (Localization)
└── Migrations/             (Database migrations)

MiniJira.Tests/            (Test project - empty)
```

## Configuration

### Database

The application uses SQLite by default. The database file `minijira.db` is created in the project root.

Connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=minijira.db"
  }
}
```

### Localization

Supported languages:
- English (en-US) - Default
- Croatian (hr-HR)

Change language using the language selector in the navigation menu.

### File Storage

Attachments are stored in the `uploads/` directory (created automatically).

## Mock Authentication

**WARNING**: This application uses mock authentication for development.

Mock users are hardcoded in `Services/MockUserService.cs`:
- John Doe
- Jane Smith
- Bob Johnson
- Alice Williams

**For Production**: Replace mock authentication with:
- ASP.NET Core Identity
- OAuth 2.0 / OpenID Connect
- Azure AD, Auth0, or IdentityServer

## Documentation

- `CLAUDE.md` - Development guidelines and project structure
- `DESIGN_SYSTEM.md` - Comprehensive UI design system
- `QUICK_START_GUIDE.md` - UI component usage guide
- `LOCALIZATION_README.md` - Localization implementation
- `RESPONSIVE_DESIGN_SUMMARY.md` - Responsive design details
- `WORK_ORDER_IMPLEMENTATION_STATUS.md` - Work order feature docs
- `DOCUMENTATION_ANALYSIS_REPORT.md` - Documentation analysis

## Development

### Adding a Migration

```bash
dotnet ef migrations add YourMigrationName --project MiniJira
dotnet ef database update --project MiniJira
```

### Running in Development

```bash
cd MiniJira
dotnet watch run
```

The application will hot-reload on code changes.

### Logging

Logs are written to:
- Console (during development)
- `logs/minijira-<date>.log` (rotating daily, 30-day retention)

## Known Limitations

- ❌ **No automated tests** - Test project exists but is empty
- ❌ **Mock authentication** - Not suitable for production
- ❌ **Single-user concurrency** - No multi-user real-time updates
- ❌ **SQLite database** - Consider PostgreSQL/SQL Server for production
- ❌ **File storage in uploads/** - Consider blob storage for production

## Future Enhancements

Potential improvements:
- Real authentication system
- Real-time collaboration (SignalR)
- Advanced reporting and analytics
- Automated test suite
- Docker containerization
- Production database support
- Cloud file storage integration

## Screenshots

(Add screenshots of your application here)

## License

(Add your license information here)

## Contributing

(Add contribution guidelines here)

## Support

For issues or questions:
- Review the documentation in the project
- Check the codebase for inline comments
- See DOCUMENTATION_ANALYSIS_REPORT.md for detailed project information

---

**Version**: 1.0
**Last Updated**: 2025-10-19
