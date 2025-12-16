### EFCore.DataClassification

## Overview

**EFCore.DataClassification** is a small extension library for **Entity Framework Core 8 (EF Core 8)** that adds **SQL Server data classification** support on top of the standard migrations pipeline.

It lets you:

- **Annotate properties** in your entity classes with a `[DataClassification]` attribute.
- Or configure classification via **Fluent API** (`HasDataClassification`).
- Automatically **generate SQL Server metadata** for:
  - `ADD SENSITIVITY CLASSIFICATION` (native SQL Server feature),
  - and `sp_addextendedproperty` / `sp_dropextendedproperty` calls for label, information type and rank.
- Keep the classification metadata **in sync with EF Core migrations** (add, remove, or change columns → classification migrations are generated accordingly).
- Validate **rank values and label lengths** at migration time with clear error messages.

The solution also includes:

- **`EFCore.DataClassification.Tests`** – unit and integration tests for the library.
- **`EFCore.DataClassification.WebApi`** – a minimal ASP.NET Core Web API sample that demonstrates how to use the library in a real application.

---

## Projects

- **`EFCore.DataClassification`**
  - Core library.
  - Contains attributes, annotations, extensions, SQL generator, custom migration operations and model differ.
- **`EFCore.DataClassification.Tests`**
  - xUnit tests for attributes, extensions, SQL generator, and migration model differ.
- **`EFCore.DataClassification.WebApi`**
  - Example ASP.NET Core 8 Web API application.
  - Uses SQL Server + this library to demonstrate classification on a `User` and other sample entities.

---

## Requirements

- **.NET 8.0**
- **SQL Server** (sensitivity classification and extended properties are SQL Server-specific)
- **Entity Framework Core 8** (`Microsoft.EntityFrameworkCore.SqlServer` 8.0.22)

---

## Core Concepts

### DataClassification constants and ranks

The library defines a central set of constants and valid rank values in `DataClassificationConstants`:

- **Annotations (EF Core metadata keys)**:
  - `DataClassification:Label`
  - `DataClassification:InformationType`
  - `DataClassification:Rank`
- **Max lengths**:
  - `MaxLabelLength = 128`
  - `MaxInformationTypeLength = 128`
- **Default schema**: `dbo`
- **Allowed ranks** (maps to SQL Server ranks):
  - `None`, `Low`, `Medium`, `High`, `Critical`

It also exposes:

- `IsValidRank(string? rank)` – checks if a rank is allowed.
- `GetAllowedRanksString()` – returns allowed ranks as comma-separated string for error messages.

### SensitivityRank enum

The **`SensitivityRank`** enum lives in the `Models` folder:

```csharp
public enum SensitivityRank
{
    None,
    Low,
    Medium,
    High,
    Critical
}
```

This is the rank you use in attributes and Fluent API.

---

## How It Works (High-Level)

1. **You mark entity properties** with:

   - `[DataClassification(...)]` attribute, or
   - `HasDataClassification(...)` Fluent API.

2. **Model building**:

   - `ModelBuilder.UseDataClassification()` scans all entity types and their properties.
   - For each property with `[DataClassification]`, it writes EF Core **annotations** using `DataClassificationConstants.Label`, `InformationType`, and `Rank`.

3. **Migrations model differ**:

   - During migrations diffing, `DataClassificationMigrationsModelDiffer` looks at column mappings and checks whether they have classification annotations.
   - It adds custom migration operations:
     - `CreateDataClassificationOperation`
     - `RemoveDataClassificationOperation`
   - It also manages change detection and ordering, so when a column is dropped the classification remove operation is executed before the column drop.

4. **SQL generation**:

   - `DataClassificationSqlGenerator` intercepts these custom operations and:
     - Writes **extended properties** with `sp_addextendedproperty` / `sp_dropextendedproperty`.
     - Writes **SQL Server sensitivity classification** using `ADD SENSITIVITY CLASSIFICATION ... WITH (LABEL = ..., INFORMATION_TYPE = ..., RANK = ...)`.
   - It validates:
     - Rank values (must be one of the allowed ranks).
     - Label length (max 128 chars).
   - On invalid configuration, it throws **`DataClassificationException`** with a helpful message.

5. **Design-time services**:
   - `DataClassificationDesignTimeServices` (library) and `Design` (WebApi) register:
     - `IMigrationsCodeGenerator` as `DataClassificationMigrationsGenerator` (adds needed namespaces to generated migrations).
     - `ICSharpMigrationOperationGenerator` as `DataClassificationMigrationOperationGenerator` (writes C# code for custom operations).

---

## Installation & Setup

### 1. Add the library to your project

You can either:

- **Reference the project directly** from your solution:
  - Add the existing `EFCore.DataClassification` project to your solution.
  - Add a Project Reference to it from your application (Web, API, etc.).

Or:

- **(If packaged)** add a NuGet package reference (not shown in this repo, but conceptually it would be something like `EFCore.DataClassification`).

### 2. Configure DbContext options

In your application (for example in `Program.cs`):

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseSqlServer(connectionString)
        .UseDataClassificationSqlServer(); // <-- enables library services
});
```

`UseDataClassificationSqlServer()`:

- Registers `DataClassificationDbContextOptionsExtension`.
- That extension wires:
  - `IMigrationsSqlGenerator` → `DataClassificationSqlGenerator`
  - `IMigrationsModelDiffer` → `DataClassificationMigrationsModelDiffer`

### 3. Enable classification scanning in your DbContext

Inside your `DbContext`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // 1. Scan for [DataClassification] attributes
    modelBuilder.UseDataClassification();

    // 2. Optional Fluent API example
    modelBuilder.Entity<User>()
        .Property(u => u.PhoneNumber)
        .HasDataClassification("Internal", "Phone Number", SensitivityRank.High);
}
```

---

## Using the Attribute

### DataClassificationAttribute

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DataClassificationAttribute : Attribute
{
    public string Label { get; }
    public string InformationType { get; }
    public SensitivityRank Rank { get; }

    public DataClassificationAttribute(string label, string informationType, SensitivityRank rank)
    {
        Label = label;
        InformationType = informationType;
        Rank = rank;
    }
}
```

### Example – `User` entity

```csharp
public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    // Attribute-based classification
    [DataClassification("Private", "Home Address", SensitivityRank.Medium)]
    public string Adress { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Fluent API example is applied in OnModelCreating (PhoneNumber)

    [DataClassification("Confidential", "Financial Information", SensitivityRank.High)]
    public int Salary { get; set; }

    [DataClassification("Confidential", "Admin Reference", SensitivityRank.High)]
    public int? AdminId { get; set; }

    public Admin? Admin { get; set; }
}
```

Another example – `Customer`:

```csharp
public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [DataClassification("Contact", "Email Address", SensitivityRank.High)]
    public string Email { get; set; } = string.Empty;

    [DataClassification("Address", "Mailing Address", SensitivityRank.None)]
    public string Address { get; set; } = string.Empty;
}
```

After running migrations, these properties will have SQL Server sensitivity classification and extended properties attached.

---

## Using Fluent API

You can also set classification via extensions on `PropertyBuilder`:

```csharp
public static class PropertyBuilderExtensions
{
    public static PropertyBuilder HasDataClassification(
        this PropertyBuilder propertyBuilder,
        string label,
        string informationType,
        SensitivityRank rank)
    { ... }

    public static PropertyBuilder<TProperty> HasDataClassification<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string label,
        string informationType,
        SensitivityRank rank)
    { ... }
}
```

### Example – from `AppDbContext`

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

---

## Migrations Support

### Custom operations

The library introduces two custom migration operations:

- **`CreateDataClassificationOperation`**
  - Used when a column with classification is added or when classification is changed.
- **`RemoveDataClassificationOperation`**
  - Used when classification should be removed (for example when a column is dropped, unmapped, or its classification is removed/changed).

### MigrationBuilder extensions

You can also **explicitly** add/remove classification in your migration code using `MigrationBuilderExtensions`:

```csharp
public static class MigrationBuilderExtensions
{
    public static OperationBuilder<CreateDataClassificationOperation> AddDataClassification(
        this MigrationBuilder migrationBuilder,
        string table,
        string column,
        string? schema = null,
        string? label = null,
        string? informationType = null,
        string? rank = null);

    public static OperationBuilder<RemoveDataClassificationOperation> DropDataClassification(
        this MigrationBuilder migrationBuilder,
        string table,
        string column,
        string? schema = null);
}
```

This is helpful if you want **manual control** over classification operations inside a particular migration.

### SQL generation details

`DataClassificationSqlGenerator`:

- Validates incoming data:
  - Allowed ranks (`None`, `Low`, `Medium`, `High`, `Critical`).
  - Label length (`<= 128`).
- Writes **extended properties**:
  - Uses `sys.sp_addextendedproperty` and `sys.sp_dropextendedproperty`.
  - Properties:
    - `DataClassification:Label`
    - `DataClassification:InformationType`
    - `DataClassification:Rank`
- Writes **sensitivity classification**:
  - Uses `ADD SENSITIVITY CLASSIFICATION TO [schema].[table].[column] WITH (...)`.
  - Maps rank string values to SQL Server ranks:
    - `"Low" → "LOW"`, `"Medium" → "MEDIUM"`, `"High" → "HIGH"`, `"Critical" → "CRITICAL"`.
  - `"None"` is treated as no sensitivity classification (no rank sent).

If configuration is invalid, it throws `DataClassificationException` so you get a **clear, early failure**.

---

## Web API Sample

The `EFCore.DataClassification.WebApi` project demonstrates an ASP.NET Core Web API using this library.

### Key parts

- **`Program`**

  - Configures `AppDbContext` with `.UseDataClassificationSqlServer()`.
  - Registers a global exception handler (`GlobalExceptionHandler`).
  - Adds AutoMapper with `UserMappingProfile`.
  - Enables Swagger/OpenAPI.

- **`AppDbContext`**

  - Defines multiple DbSets: `Users`, `Admins`, `Games`, `Car`, `Bikes`, `Homes`, `Customers`, `Documents`, etc.
  - Calls `modelBuilder.UseDataClassification()` in `OnModelCreating`.
  - Configures one property (`User.PhoneNumber`) via Fluent API `HasDataClassification`.

- **Models**

  - `User`, `Customer`, `Admin`, `Game`, `Car`, `Bike`, `Home`, `Document`, etc.
  - Several properties are decorated with `[DataClassification]` to show different scenarios.

- **Controllers**

  - `UsersController`:
    - Basic CRUD endpoints (`GET`, `POST`, `PUT`, `DELETE`).
    - Additional queries (search, filter by admin).
    - Uses DTOs and AutoMapper (`UserDtos`, `UserMappingProfile`).

- **Middleware**

  - `GlobalExceptionHandler`:
    - Implements `.NET 8 IExceptionHandler` pattern.
    - Maps:
      - `DataClassificationException` → 400 Bad Request with specific error details.
      - `ArgumentNullException` → 400 Bad Request.
      - `InvalidOperationException` → 409 Conflict.
      - Any other exception → 500 Internal Server Error.

- **Design-time**
  - `Design` class registers the design-time services for migrations generator and operation generator so that `dotnet ef` commands produce migrations that understand the custom operations.

### Running the Web API

1. Configure a valid **SQL Server connection string** in `appsettings.json` (e.g. `DefaultConnection`).
2. Run migrations, for example:
   ```bash
   dotnet ef database update --project EFCore.DataClassification.WebApi
   ```
3. Start the API:
   ```bash
   dotnet run --project EFCore.DataClassification.WebApi
   ```
4. Open Swagger UI (usually at `https://localhost:{port}/swagger`) and test endpoints like:
   - `GET /api/users`
   - `POST /api/users`
   - `GET /api/users/search?query=...`

---

## Tests

The `EFCore.DataClassification.Tests` project includes tests for:

- **Attributes** – e.g. `DataClassificationAttributeTests` confirms:
  - Properties set correctly.
  - Attribute is applicable to properties only.
  - `AllowMultiple = false`.
- **Extensions** – tests for:
  - `ModelBuilderExtensions.UseDataClassification()` – annotations applied correctly.
  - `PropertyBuilderExtensions.HasDataClassification()` – annotation-based configuration works.
  - `MigrationBuilderExtensions` – custom operations are added correctly.
- **Infrastructure**:
  - `DataClassificationMigrationsModelDiffer`:
    - Adds `CreateDataClassificationOperation` for new classified columns.
    - Adds `RemoveDataClassificationOperation` for removed/changed classification.
    - Sorts operations correctly when dropping columns.
  - `DataClassificationSqlGeneratorTests`:
    - Generated SQL for extended properties and sensitivity classification.
- **Integration tests**:
  - Verify end-to-end behavior from model configuration to generated migrations and SQL.

To run tests:

```bash
dotnet test
```

---

## Error Handling and Validation

When classification configuration is invalid, the library throws **`DataClassificationException`**:

- Used in `DataClassificationSqlGenerator.ValidateDataClassification(...)`.
- Scenarios:
  - Rank not in allowed set.
  - Label too long.

In the Web API sample:

- `GlobalExceptionHandler` catches `DataClassificationException` and returns a 400 Bad Request with a clear error message.
- In development, additional details may also be exposed for other exception types.

---

## Summary

**EFCore.DataClassification** is a focused EF Core 8 extension that:

- Adds an intuitive attribute + Fluent API for **data classification**.
- Integrates tightly with **EF Core migrations** and **SQL Server sensitivity classification**.
- Includes a ready-to-run **Web API example** and a rich **test suite**.
- Provides **validation and clear error messages** through `DataClassificationException`.
