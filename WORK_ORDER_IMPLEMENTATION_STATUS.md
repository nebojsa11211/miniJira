# Work Order Implementation Status

## Completed Tasks ✅

### 1. Analysis & Design
- ✅ Analyzed RadniNalog.xlsx structure (48 rows × 21 columns)
- ✅ Identified header section, data table, and summary sections
- ✅ Documented all fields and their purposes
- ✅ Designed database schema (see RADNI_NALOG_ANALYSIS.md)

### 2. Database Models
- ✅ Created `WorkOrderHeader` model with all header fields:
  - Customer, Object, Product
  - Work Order Number, Order/Delivery references and dates
  - Material, Surface, Cut, Processing, Packing specifications
  - Notes
  - Created/Updated metadata

- ✅ Created `WorkOrderRow` model with:
  - Position markers (PZ, P1, R, O, P2)
  - Dimensions (Length, Width, Thickness)
  - Quantities (Pieces, FromPieces)
  - Text fields (Description, ProcessingNotes)
  - Calculated fields (SquareMeters, SquareMetersTot, LinearMeters, CubicMetersTot, WeightKg)
  - `RecalculateFields()` method for automatic calculations

- ✅ Updated Task model with `WorkOrder` navigation property

### 3. Database Integration
- ✅ Added `WorkOrderHeaders` and `WorkOrderRows` DbSets to ApplicationDbContext
- ✅ Configured one-to-one relationship: Task → WorkOrderHeader
- ✅ Configured one-to-many relationship: WorkOrderHeader → WorkOrderRows
- ✅ Added proper indexes for performance:
  - Unique index on TaskId in WorkOrderHeader
  - Unique composite index on (WorkOrderHeaderId, RowNumber)
  - Indexes on WorkOrderNumber, CreatedAt, UpdatedAt
- ✅ Created and applied EF Core migration `AddWorkOrderTables`
- ✅ Database tables created successfully

## Remaining Tasks 📋

### 4. Service Layer (Next: In Progress)
- ⏳ Create DTOs:
  - `WorkOrderHeaderDto`
  - `WorkOrderRowDto`
  - `CreateWorkOrderRequest`
  - `UpdateWorkOrderRequest`
  - `UpdateWorkOrderRowRequest`

- ⏳ Create `IWorkOrderService` interface:
  - `GetByTaskIdAsync(Guid taskId)`
  - `CreateAsync(Guid taskId, CreateWorkOrderRequest request)`
  - `UpdateHeaderAsync(Guid headerId, UpdateWorkOrderRequest request)`
  - `UpdateRowAsync(Guid rowId, UpdateWorkOrderRowRequest request)`
  - `UpdateMultipleRowsAsync(Guid headerId, List<UpdateWorkOrderRowRequest> rows)`
  - `DeleteAsync(Guid headerId)`
  - `ExportToExcelAsync(Guid headerId)`
  - `ExportToCsvAsync(Guid headerId)`

- ⏳ Create `WorkOrderService` implementation with:
  - CRUD operations
  - Automatic calculation logic
  - Running total calculations (M² TOT)
  - Summary calculations

### 5. Blazor Components
- 📝 Create `WorkOrderView.razor`:
  - Header section with input fields
  - Data table (30 rows) with inline editing
  - Automatic calculation on field changes
  - Summary section with totals
  - Excel/CSV export buttons

- 📝 Create `WorkOrderCell.razor`:
  - Reusable cell component
  - Edit/display mode toggle
  - Type-specific input controls (text, number, date)
  - Validation

### 6. Excel Export
- 📝 Add ClosedXML package to main project
- 📝 Implement Excel export in `WorkOrderService`:
  - Match exact Excel layout
  - Merge cells as in original
  - Apply formatting (bold, borders, colors)
  - Set column widths

- 📝 Implement CSV export:
  - Simple flat export for data rows
  - Include headers

### 7. Integration
- 📝 Add Work Order tab to `TaskDetail.razor`:
  - Tab component with "Work Order" tab
  - Load/create work order on demand
  - Save changes automatically or with button

### 8. Testing
- 📝 Write unit tests for:
  - `WorkOrderRow.RecalculateFields()` method
  - Service layer calculations
  - Summary totals

- 📝 End-to-end testing:
  - Create work order from UI
  - Enter data and verify calculations
  - Export to Excel and verify format
  - Re-import and verify data integrity

## Key Features Implemented

### Automatic Calculations
The `WorkOrderRow.RecalculateFields()` method implements:

```csharp
// Square Meters: (Length × Width × Pieces) / 1,000,000
SquareMeters = (Length * Width * Pieces) / 1_000_000

// Linear Meters: (Perimeter × Pieces) / 1,000
LinearMeters = (2 * (Length + Width) * Pieces) / 1000

// Cubic Meters: (Length × Width × Thickness × Pieces) / 1,000,000,000
CubicMetersTot = (Length * Width * Thickness * Pieces) / 1_000_000_000

// Weight: Volume × Density
WeightKg = CubicMetersTot * MaterialDensity
```

### Database Schema
- **WorkOrderHeaders** table: Stores header/metadata (1 per task)
- **WorkOrderRows** table: Stores data rows (up to 30 per work order)
- One-to-one relationship between Task and WorkOrderHeader
- One-to-many relationship between WorkOrderHeader and WorkOrderRows

## Next Steps

1. **Complete Service Layer** (Current Priority):
   - Create DTOs for data transfer
   - Create IWorkOrderService interface
   - Implement WorkOrderService with calculations

2. **Create Blazor Components**:
   - Interactive work order view
   - Inline cell editing
   - Real-time calculations

3. **Implement Excel Export**:
   - High-fidelity export matching original layout
   - CSV export for data analysis

4. **Integration & Testing**:
   - Add to TaskDetail page
   - Comprehensive testing

## Files Created
- `MiniJira/Models/WorkOrderHeader.cs`
- `MiniJira/Models/WorkOrderRow.cs`
- `MiniJira/Data/ApplicationDbContext.cs` (updated)
- `MiniJira/Models/Task.cs` (updated)
- `MiniJira/Migrations/[timestamp]_AddWorkOrderTables.cs`
- `RADNI_NALOG_ANALYSIS.md` (analysis document)
- `SPREADSHEET_IMPLEMENTATION_PLAN.md` (design document)

## Architecture Decisions

1. **Specialized vs Generic**: Chose specialized Work Order system over generic spreadsheet for:
   - Better performance
   - Type safety
   - Easier maintenance
   - Better business logic enforcement

2. **Calculated Fields Storage**: Storing calculated fields in database for:
   - Performance (no recalculation on every read)
   - Historical accuracy
   - Summary calculations
   - Fields are recalculated on update

3. **One-to-One with Task**: Each task has at most one work order:
   - Simpler model
   - Matches business requirement
   - Easy to extend to one-to-many if needed

4. **Row Number Sequencing**: Using integer RowNumber (1-30) for:
   - Deterministic ordering
   - Easy insertion/deletion
   - User-friendly display

## Estimated Remaining Time
- Service Layer: ~2 hours
- Blazor Components: ~4 hours
- Excel Export: ~2 hours
- Integration & Testing: ~2 hours
- **Total**: ~10 hours

## Notes
- The Excel file uses Croatian labels (KUPAC, OBJEKT, etc.)
- Consider adding localization for UI labels
- Material density can be configured per row or from a lookup table
- Summary section calculations need aggregate queries across all rows
