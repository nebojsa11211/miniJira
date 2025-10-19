# Radni Nalog (Work Order) Excel Analysis

## File Structure

**Worksheet Name**: RADNI NALOG
**Dimensions**: A2:U49 (21 columns × 48 rows)

## Layout Breakdown

### 1. Header Section (Rows 2-8)

**Row 2** - Main Headers (Merged cells):
- **B2:K2** - "KUPAC" (Customer) - Text input field
- **L2:N2** - "OBJEKT" (Object/Project) - Text input field
- **O2:T2** - "PROIZVOD" (Product) - Text input field
- **U3** - "X" checkbox marker

**Row 4** - Document Info:
- **B4:C4** - "RN.BR." (Work Order Number)
- **D4:F4** - Work order number input (empty)
- **G4** - "NARUDŽBA:" (Order)
- **H4:K4** - Order input field (empty)
- **L4:N4** - "ISPORUKA:" (Delivery)
- **O4:Q4** - Delivery input field (empty)
- **R4** - "STR." (Page)
- **S4:T4** - "1 - 1" (Page number)

**Row 5** - Material Info:
- **B5** - "MAT. I OBRADA:" (Material and Processing)
- **G5:N5** - Material input field
- **O5:T5** - Processing input field

**Row 6** - Processing Details:
- **B6:E6** - "PLOHA:" (Surface/Panel)
- **F6:G6** - Surface input
- **H6** - "REZ:" (Cut)
- **I6:K6** - Cut input
- **L6:M6** - "OBRADA:" (Processing)
- **N6:O6** - Processing input
- **P6:Q6** - "PAKIRANJE:" (Packing)
- **R6:T6** - Packing input

**Row 7-8** - Notes:
- **B7:E8** - "BILJEŠKA:" (Note)
- **F7:T7** - Note text area (Row 7)
- **F8:T8** - Note text area (Row 8)

### 2. Data Table Section (Rows 9-40)

**Row 9** - Column Headers:
- **B9** - "PZ" (Position)
- **C9** - "P" (?)
- **D9** - "R" (?)
- **E9** - "O" (?)
- **F9** - "P" (?)
- **G9** - "DUŽINA" (Length)
- **H9** - "ŠIRINA" (Width)
- **I9** - "DEB." (Thickness)
- **J9** - "KOM" (Pieces/Quantity)
- **K9** - "IZ KOM" (From Pieces)
- **L9:M9** - "OPIS" (Description) [Merged]
- **N9:O9** - "OBRADA" (Processing) [Merged]
- **P9** - "M 2" (Square meters)
- **Q9** - "M 2 TOT" (Total Square meters)
- **R9** - "M 1" (Linear meters)
- **S9** - "M 3 TOT" (Total Cubic meters)
- **T9** - "KG" (Kilograms)

**Rows 11-40** - Data Entry Rows:
- 30 rows for data entry
- Each row currently contains "0" in columns P-T (calculated fields)
- Rows 10 appears to be empty (spacing)

### 3. Summary Section (Rows 41-49)

**Row 41** - Summary Header:
- **B41** - "UKUPNO KOMADA PODIJELJENO" (Total Pieces Divided)

**Row 42** - Summary Column Headers:
- **B42** - "DEB." (Thickness)
- **D42** - "KOM" (Pieces)
- **G42** - "M2" (Square meters)
- **H42** - "M 1" (Linear meters)
- **I42** - "M 3" (Cubic meters)
- **J42** - "KG" (Kilograms)
- **M42** - "UKUPNO" (Total)
- **P42** - "KOM" (Pieces)
- **Q42** - "M 2" (Square meters)
- **R42** - "M 1" (Linear meters)
- **S42** - "M 3" (Cubic meters)
- **T42** - "KG" (Kilograms)

**Rows 43-49** - Summary Data:
- Multiple summary rows with calculations
- All currently showing "0" values
- Row 44 includes date fields: "NARUDŽBA:" and "ISPORUKA:" with dates "31/12/1899 00:00:00"
- Row 46 includes: "MATERIJAL I OBRADA:" with value "0"

## Cell Types Identified

### Input Fields (Editable):
1. **Text fields**:
   - Customer (KUPAC)
   - Object/Project (OBJEKT)
   - Product (PROIZVOD)
   - Work Order Number (RN.BR.)
   - Order field (NARUDŽBA)
   - Material and Processing
   - Surface (PLOHA)
   - Cut (REZ)
   - Processing (OBRADA)
   - Packing (PAKIRANJE)
   - Notes (BILJEŠKA)
   - Description (OPIS) per row
   - Processing (OBRADA) per row

2. **Numeric fields**:
   - Length (DUŽINA)
   - Width (ŠIRINA)
   - Thickness (DEB.)
   - Pieces (KOM)
   - From Pieces (IZ KOM)

3. **Date fields**:
   - Order date (NARUDŽBA)
   - Delivery date (ISPORUKA)

4. **Checkbox**:
   - U3 - "X" marker

### Calculated/Read-Only Fields:
- **M 2** (Square meters) - Likely calculated from Length × Width
- **M 2 TOT** (Total Square meters) - Running total
- **M 1** (Linear meters) - Calculated
- **M 3 TOT** (Total Cubic meters) - Calculated from dimensions
- **KG** (Weight) - Calculated
- All summary section fields (Rows 42-49)

## Merged Cells

Extensive use of merged cells for:
- Headers spanning multiple columns
- Multi-line input areas (Notes)
- Description and Processing columns in data table
- Summary section labels

Total merged ranges: 100+ (including empty merged areas for layout)

## Data Model Implications

### Template Structure:
1. **Header/Metadata** (8 cells):
   - Customer, Object, Product
   - Work Order Number, Order, Delivery
   - Material info, Processing details
   - Notes (2 rows)

2. **Data Grid** (30 rows × 15 columns):
   - 5 position markers (PZ, P, R, O, P)
   - 3 dimension fields (Length, Width, Thickness)
   - 2 quantity fields (KOM, IZ KOM)
   - 2 text fields (Description, Processing)
   - 5 calculated fields (M2, M2 TOT, M1, M3 TOT, KG)

3. **Summary** (Multiple summary rows):
   - Grouped by thickness
   - Totals for quantities, areas, volumes, weights

### Formulas to Implement:
- M2 = DUŽINA × ŠIRINA (Length × Width)
- M3 = DUŽINA × ŠIRINA × DEB. / 1000000 (Volume)
- KG = M3 × Material Density
- M2 TOT = Running sum of M2
- M3 TOT = Running sum of M3

## Database Design Recommendations

### Approach 1: Flexible Cell-Based Storage
Store each cell independently with row/column coordinates.

**Pros**:
- Handles merged cells easily
- Flexible for different templates
- Easy to render

**Cons**:
- Complex queries for data analysis
- Difficult to validate business rules
- Less type-safe

### Approach 2: Structured Storage
Store header data separately from data rows.

**Pros**:
- Strongly typed
- Easy to query and report
- Better validation
- Normalized data structure

**Cons**:
- Less flexible for layout changes
- Need separate rendering logic

## Recommended Implementation: Hybrid Approach

1. **WorkOrderHeader** table:
   - TaskId, Customer, Object, Product
   - WorkOrderNumber, OrderDate, DeliveryDate
   - Material, Surface, Cut, Processing, Packing
   - Notes

2. **WorkOrderRow** table:
   - WorkOrderHeaderId, RowNumber
   - Position markers (PZ, P, R, O, P)
   - Length, Width, Thickness
   - Pieces, FromPieces
   - Description, Processing
   - M2 (calculated), M2Tot, M1, M3Tot, KG

3. **SpreadsheetTemplate** table (for rendering):
   - Template metadata
   - Cell definitions for precise Excel-like rendering

This approach:
- Maintains structured data for business logic
- Preserves layout information for rendering
- Enables both type-safe operations and flexible display
