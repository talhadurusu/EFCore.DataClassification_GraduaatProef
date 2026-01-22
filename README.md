# EFCore.DataClassification

Entity Framework Core 8 extension for SQL Server data classification (sensitivity labels) with automatic migration support.

## Quick Start

### 1. Add Reference

```bash
dotnet add reference ../EFCore.DataClassification/EFCore.DataClassification.csproj
```

### 2. Configure DbContext

```csharp
using EFCore.DataClassification.Extensions;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseSqlServer(connectionString)
        .UseDataClassificationSqlServer();
});
```

### 3. Enable Classification Scanning

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.UseDataClassification();
}
```

### 4. Add Attributes to Your Entities

```csharp
using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

public class Customer
{
    public int Id { get; set; }
    
    [DataClassification("Contact", "Email Address", SensitivityRank.High)]
    public string? Email { get; set; }
    
    [DataClassification("Contact", "Phone Number", SensitivityRank.High)]
    public string? PhoneNumber { get; set; }
}
```

**Attribute parameters:** `(label, informationType, rank)`

**Valid ranks:** `None`, `Low`, `Medium`, `High`, `Critical`

### 5. Create and Apply Migration

```bash
dotnet ef migrations add AddDataClassification
dotnet ef database update
```

**Done!** SQL Server sensitivity classification metadata is automatically generated.

## Usage

### Using Attributes

```csharp
[DataClassification("Label", "Information Type", SensitivityRank.High)]
public string Email { get; set; }
```

### Using Fluent API

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.UseDataClassification();
    
    modelBuilder.Entity<User>()
        .Property(u => u.PhoneNumber)
        .HasDataClassification("Internal", "Phone Number", SensitivityRank.High);
}
```

## How It Works

1. **Mark properties** with `[DataClassification]` attribute or Fluent API
2. **Model building** scans and applies annotations to EF Core metadata
3. **Migrations** automatically generate classification operations when columns are added, removed, renamed, or altered
4. **SQL generation** creates both:
   - Extended properties (works on all SQL Server versions)
   - Native sensitivity classification (SQL Server 2019+)

The library automatically handles:
- Operation ordering (removes before drops, creates after renames/alters)
- SQL Server version detection (adapts behavior for 2017 vs 2019+)
- Validation (rank values, label lengths)

## Requirements

- **.NET 8.0**
- **SQL Server 2017+**
  - SQL Server 2019 & Azure SQL: Full support (Native Classification + Extended Properties)
  - SQL Server 2017: Extended Properties only (classification commands safely skipped)
- **Entity Framework Core 8** (`Microsoft.EntityFrameworkCore.SqlServer` 8.0.22)

## Features

- ✅ **Declarative Configuration** - Attributes or Fluent API
- ✅ **Automatic SQL Generation** - Native classification + extended properties
- ✅ **Migration-Aware** - Syncs with schema changes automatically
- ✅ **Version Compatibility** - Auto-detects SQL Server version
- ✅ **Validation** - Built-in validation with clear error messages
- ✅ **Zero Configuration** - Works out of the box

## Migration Operations

The library generates custom migration operations:

- `AddDataClassification` - Adds classification to a column
- `DropDataClassification` - Removes classification from a column

These operations are automatically ordered correctly:
- Remove operations run **before** column drops/renames/alters
- Create operations run **after** column renames/alters

## Error Handling

Invalid configuration throws `DataClassificationException` with clear messages:
- Invalid rank values
- Label/information type too long (max 128 chars)

## Projects

- **`EFCore.DataClassification`** - Core library
- **`EFCore.DataClassification.Tests`** - Test suite (55 tests)
- **`EFCore.DataClassification.WebApi`** - Example Web API with sample entities

## Example Web API

The `EFCore.DataClassification.WebApi` project demonstrates:
- Configuration in `Program.cs` and `AppDbContext`
- Multiple entity examples (User, Admin, Customer, Game, Car, etc.)
- API endpoints for querying classification metadata
- Global exception handling

Run the example:
```bash
dotnet run --project EFCore.DataClassification.WebApi
```

## Testing

Run tests:
```bash
dotnet test
```

Test suite includes:
- Attribute and extension tests
- SQL generator tests
- Migration model differ tests (ordering, edge cases)
- Integration tests

## License

MIT License
