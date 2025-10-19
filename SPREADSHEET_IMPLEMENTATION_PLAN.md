# Dynamic Spreadsheet Implementation Plan

## Overview
Convert Excel spreadsheets into dynamic, database-backed web components that allow data entry, modification, and export. Each task in MiniJira will have its own associated spreadsheet.

## Architecture Design

### 1. Database Schema

#### SpreadsheetTemplate
Defines the structure/template for spreadsheets (reusable across multiple tasks).
```csharp
- Guid Id (PK)
- string Name
- string Description
- int RowCount
- int ColumnCount
- bool IsActive
- DateTime CreatedAt
- DateTime UpdatedAt
- Navigation: ICollection<SpreadsheetCell> CellTemplates
- Navigation: ICollection<TaskSpreadsheet> TaskSpreadsheets
```

#### SpreadsheetCell (Template Definition)
Defines cell properties in a template.
```csharp
- Guid Id (PK)
- Guid SpreadsheetTemplateId (FK)
- int RowIndex
- int ColumnIndex
- string? Label (e.g., "Employee Name", "Hours")
- string? DefaultValue
- string DataType (Text, Number, Date, Formula, Dropdown)
- bool IsReadOnly
- bool IsMerged
- int? MergeRowSpan
- int? MergeColSpan
- string? CssClasses (for styling: bold, header, etc.)
- string? ValidationRules (JSON)
- string? DropdownOptions (JSON for dropdown values)
```

#### TaskSpreadsheet
Links a task to its spreadsheet instance.
```csharp
- Guid Id (PK)
- Guid TaskId (FK) - UNIQUE
- Guid? SpreadsheetTemplateId (FK, nullable for custom sheets)
- string? CustomName
- DateTime CreatedAt
- DateTime LastModifiedAt
- Guid? LastModifiedBy
- Navigation: ICollection<SpreadsheetCellValue> CellValues
- Navigation: Task Task
```

#### SpreadsheetCellValue
Stores actual cell data for a task's spreadsheet.
```csharp
- Guid Id (PK)
- Guid TaskSpreadsheetId (FK)
- int RowIndex
- int ColumnIndex
- string? Value
- string? FormattedValue (calculated/formatted display value)
- string? Formula (if cell contains formula)
- DateTime? LastModifiedAt
- Guid? ModifiedBy
- Unique Index: (TaskSpreadsheetId, RowIndex, ColumnIndex)
```

### 2. Service Layer

#### ISpreadsheetService
```csharp
// Template Management
Task<Result<SpreadsheetTemplateDto>> CreateTemplateAsync(CreateTemplateRequest request);
Task<Result<SpreadsheetTemplateDto>> GetTemplateAsync(Guid templateId);
Task<Result<List<SpreadsheetTemplateDto>>> GetAllTemplatesAsync();
Task<Result> UpdateTemplateAsync(Guid templateId, UpdateTemplateRequest request);
Task<Result> DeleteTemplateAsync(Guid templateId);

// Task Spreadsheet Operations
Task<Result<TaskSpreadsheetDto>> CreateTaskSpreadsheetAsync(Guid taskId, Guid? templateId);
Task<Result<TaskSpreadsheetDto>> GetTaskSpreadsheetAsync(Guid taskId);
Task<Result> UpdateCellValueAsync(Guid taskSpreadsheetId, int row, int col, string value);
Task<Result> UpdateMultipleCellsAsync(Guid taskSpreadsheetId, List<CellUpdateDto> updates);
Task<Result<byte[]>> ExportToExcelAsync(Guid taskSpreadsheetId);
Task<Result<string>> ExportToCsvAsync(Guid taskSpreadsheetId);
Task<Result> ImportFromExcelAsync(Guid taskSpreadsheetId, Stream fileStream);
```

### 3. DTOs

```csharp
// SpreadsheetTemplateDto
public class SpreadsheetTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public List<SpreadsheetCellDto> CellDefinitions { get; set; }
}

// TaskSpreadsheetDto
public class TaskSpreadsheetDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid? SpreadsheetTemplateId { get; set; }
    public string? CustomName { get; set; }
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public List<SpreadsheetCellValueDto> CellValues { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

// SpreadsheetCellDto
public class SpreadsheetCellDto
{
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public string? Label { get; set; }
    public string? Value { get; set; }
    public string DataType { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsMerged { get; set; }
    public int? MergeRowSpan { get; set; }
    public int? MergeColSpan { get; set; }
    public string? CssClasses { get; set; }
    public List<string>? DropdownOptions { get; set; }
}

// CellUpdateDto
public class CellUpdateDto
{
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public string Value { get; set; }
}
```

### 4. Blazor Components

#### SpreadsheetView.razor
Main component for displaying and editing spreadsheet.

**Features:**
- Renders HTML table based on template/data
- Inline cell editing (double-click to edit)
- Real-time updates to database
- Formula evaluation (basic)
- Cell styling based on template
- Dropdown support for enumerated values
- Merged cell support

**Parameters:**
```csharp
[Parameter] public Guid TaskId { get; set; }
[Parameter] public bool IsReadOnly { get; set; } = false;
[Parameter] public EventCallback OnDataChanged { get; set; }
```

#### SpreadsheetCell.razor
Individual cell component (reusable).

**Features:**
- Display/Edit mode toggle
- Data type-specific input controls
- Validation
- Styling

#### SpreadsheetToolbar.razor
Toolbar with actions.

**Features:**
- Export to Excel (.xlsx)
- Export to CSV
- Import from Excel
- Print view
- Toggle read-only mode
- Save changes button

#### TemplateManager.razor (Admin)
Admin component for managing spreadsheet templates.

**Features:**
- Create new templates
- Edit existing templates
- Define cell properties
- Preview templates
- Duplicate templates

### 5. Required NuGet Packages

```xml
<!-- For Excel export/import -->
<PackageReference Include="EPPlus" Version="7.0.0" />
<!-- or -->
<PackageReference Include="ClosedXML" Version="0.102.2" />

<!-- For CSV handling -->
<PackageReference Include="CsvHelper" Version="30.0.1" />

<!-- Already have EF Core for database -->
```

### 6. Integration Points

#### TaskDetail.razor Enhancement
Add tab or section for spreadsheet:
```razor
<TabControl>
    <Tab Title="Details">...</Tab>
    <Tab Title="Comments">...</Tab>
    <Tab Title="Attachments">...</Tab>
    <Tab Title="Spreadsheet">
        <SpreadsheetView TaskId="@TaskId" />
    </Tab>
</TabControl>
```

#### Task Model Enhancement
Add navigation property:
```csharp
public class Task
{
    // Existing properties...
    public virtual TaskSpreadsheet? Spreadsheet { get; set; }
}
```

### 7. CSS Styling

Create `spreadsheet.css`:
```css
.spreadsheet-container {
    overflow: auto;
    max-height: 70vh;
    border: 1px solid #ddd;
}

.spreadsheet-table {
    border-collapse: collapse;
    width: 100%;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    font-size: 12px;
}

.spreadsheet-cell {
    border: 1px solid #ccc;
    padding: 4px 8px;
    min-width: 80px;
    height: 24px;
    position: relative;
}

.spreadsheet-cell.editable:hover {
    background-color: #f0f8ff;
    cursor: cell;
}

.spreadsheet-cell.editing {
    padding: 0;
}

.spreadsheet-cell input,
.spreadsheet-cell select {
    width: 100%;
    height: 100%;
    border: 2px solid #4CAF50;
    padding: 2px 4px;
    font-size: 12px;
}

.spreadsheet-cell.header {
    background-color: #f2f2f2;
    font-weight: bold;
    text-align: center;
}

.spreadsheet-cell.readonly {
    background-color: #f9f9f9;
    color: #666;
}

.spreadsheet-cell.number {
    text-align: right;
}

.spreadsheet-cell.formula {
    font-style: italic;
    color: #0066cc;
}

.spreadsheet-toolbar {
    padding: 10px;
    background-color: #f5f5f5;
    border-bottom: 1px solid #ddd;
    display: flex;
    gap: 10px;
    align-items: center;
}

.spreadsheet-toolbar button {
    padding: 6px 12px;
    border: 1px solid #ccc;
    background-color: white;
    border-radius: 4px;
    cursor: pointer;
}

.spreadsheet-toolbar button:hover {
    background-color: #e9e9e9;
}

/* Dark mode support */
.dark-mode .spreadsheet-cell {
    border-color: #555;
    background-color: #2a2a2a;
    color: #e0e0e0;
}

.dark-mode .spreadsheet-cell.header {
    background-color: #1a1a1a;
}
```

### 8. JavaScript Interop (for Excel Export)

Create `spreadsheet.js`:
```javascript
// Handle keyboard navigation
window.spreadsheetHelper = {
    focusCell: function(cellId) {
        const cell = document.getElementById(cellId);
        if (cell) cell.focus();
    },

    selectCellText: function(cellId) {
        const cell = document.getElementById(cellId);
        if (cell && cell.tagName === 'INPUT') {
            cell.select();
        }
    },

    downloadFile: function(filename, contentType, content) {
        const blob = new Blob([content], { type: contentType });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
    }
};
```

### 9. Implementation Phases

**Phase 1: Core Infrastructure (Tasks 1-3)**
- Database models and migrations
- Service layer with basic CRUD
- Unit tests for services

**Phase 2: UI Components (Tasks 4-5)**
- SpreadsheetView component
- Cell editing functionality
- Basic styling

**Phase 3: Export/Import (Task 6)**
- Excel export (.xlsx)
- CSV export
- Excel import

**Phase 4: Integration (Tasks 7-8)**
- Task entity integration
- TaskDetail page enhancement
- Navigation

**Phase 5: Advanced Features (Task 9)**
- Template manager
- Formula support
- Cell validation

**Phase 6: Testing & Polish (Tasks 10-12)**
- Comprehensive testing
- Error handling
- Performance optimization

### 10. Key Design Decisions

1. **Template-Based Approach**: Separate templates from data for reusability
2. **Cell-by-Cell Storage**: Each cell stored separately for flexibility and efficient updates
3. **Lazy Loading**: Only load spreadsheet data when needed
4. **Optimistic Updates**: Update UI immediately, sync to database asynchronously
5. **No Real-Time Collaboration**: Keep it simple initially, add SignalR later if needed
6. **Formula Support**: Basic formulas only (SUM, AVERAGE, simple arithmetic)
7. **Export Strategy**: Use EPPlus or ClosedXML for high-fidelity Excel export

### 11. Performance Considerations

- Use AsNoTracking() for read operations
- Implement pagination for large spreadsheets
- Debounce cell updates (500ms delay)
- Use virtual scrolling for very large sheets
- Index on (TaskSpreadsheetId, RowIndex, ColumnIndex)
- Cache template definitions in memory

### 12. Security Considerations

- Validate cell values server-side
- Sanitize formulas to prevent injection
- Check user permissions before allowing edits
- Audit trail for cell modifications
- Rate limiting on cell updates

## Next Steps

1. Analyze the specific structure of RadniNalog.xlsx
2. Create database models based on that structure
3. Begin implementation following the phases above

## Questions to Answer

1. What is the exact structure of RadniNalog.xlsx?
   - Number of rows and columns
   - Which cells are headers/labels
   - Which cells are editable vs. read-only
   - Any merged cells?
   - Data types for each cell
   - Any formulas or calculations?

2. Should all tasks use the same spreadsheet template, or different templates per task type?

3. Do we need version history for spreadsheet changes?

4. Should changes be auto-saved or require explicit save action?
