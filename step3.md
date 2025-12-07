# SafeBoda Project - Step 3: Data Persistence with EF Core
## Comprehensive Line-by-Line Explanation

---

## Table of Contents
1. [Introduction](#introduction)
2. [What is Data Persistence?](#what-is-data-persistence)
3. [What is Entity Framework Core?](#what-is-entity-framework-core)
4. [Step 1: Add EF Core NuGet Packages](#step-1-add-ef-core-nuget-packages)
5. [Step 2: Create Infrastructure Class Library](#step-2-create-infrastructure-class-library)
6. [Step 3: Create the DbContext Class](#step-3-create-the-dbcontext-class)
7. [Step 4: Define DbSet Properties](#step-4-define-dbset-properties)
8. [Step 5: Configure Database Connection String](#step-5-configure-database-connection-string)
9. [Step 6: Register the DbContext](#step-6-register-the-dbcontext)
10. [Step 7: Create and Apply Migrations](#step-7-create-and-apply-migrations)
11. [Step 8: Implement EfTripRepository](#step-8-implement-eftriprepository)
12. [Step 9: Update Dependency Injection](#step-9-update-dependency-injection)
13. [Step 10: Verify API Functionality](#step-10-verify-api-functionality)
14. [Understanding Database Entities](#understanding-database-entities)
15. [Understanding Migrations](#understanding-migrations)
16. [Summary](#summary)

---

## Introduction

Welcome to Step 3! In this module, we're solving a critical problem: **our data disappears every time we restart the application!** 

Currently, we're using `InMemoryTripRepository`, which stores data in RAM. When the application stops, all data is lost. This is fine for testing, but not for a real application. We need **persistent storage** - data that survives restarts, server crashes, and power outages.

**What we'll build:**
- A database-backed storage solution
- Entity Framework Core integration
- Database migrations for schema management
- A repository that saves data permanently

**The transformation:**
- **Before**: Data in memory → Lost on restart ❌
- **After**: Data in database → Persists forever ✅

---

## What is Data Persistence?

### The Problem with In-Memory Storage

**In-Memory Storage (Current):**
```
Application Starts
    ↓
Data stored in RAM (List<Trip>)
    ↓
Application Stops
    ↓
RAM is cleared
    ↓
All data is LOST! 💥
```

**Database Storage (What we're building):**
```
Application Starts
    ↓
Data stored in Database (on disk)
    ↓
Application Stops
    ↓
Database file remains on disk
    ↓
Data is SAFE! ✅
    ↓
Application Starts Again
    ↓
Data is still there! 🎉
```

### What is Persistence?

**Persistence** means data **survives** after the program ends. Think of it like:
- **Not persistent**: Writing on a whiteboard (erased when power goes off)
- **Persistent**: Writing in a notebook (survives power outages)

**Why do we need it?**
- Users expect their data to be saved
- Business data must not be lost
- Historical records need to be preserved
- Compliance and auditing requirements

### Types of Storage

| Type | Speed | Persistence | Use Case |
|------|-------|-------------|----------|
| **RAM (Memory)** | Very Fast | ❌ Temporary | Caching, temporary data |
| **Database** | Fast | ✅ Permanent | Application data |
| **File System** | Medium | ✅ Permanent | Documents, images |
| **Cloud Storage** | Variable | ✅ Permanent | Large files, backups |

For our application, we need a **database** - it's fast enough and provides persistence.

---

## What is Entity Framework Core?

### The Problem: Object-Relational Mismatch

**C# Objects (What we write):**
```csharp
public record Trip(
    Guid Id,
    Guid RiderId,
    Location Start,
    decimal Fare
);
```

**Database Tables (What databases understand):**
```sql
CREATE TABLE Trips (
    Id UNIQUEIDENTIFIER,
    RiderId UNIQUEIDENTIFIER,
    StartLatitude FLOAT,
    StartLongitude FLOAT,
    Fare DECIMAL(18,2)
);
```

**The Mismatch:**
- C# has **objects** with properties
- Databases have **tables** with columns
- C# has **nested objects** (Location inside Trip)
- Databases have **flat structures** (columns only)
- C# has **types** (Guid, DateTime)
- Databases have **SQL types** (UNIQUEIDENTIFIER, DATETIME)

### What is an ORM?

**ORM** = **Object-Relational Mapper**

An ORM is a tool that **translates** between:
- C# objects ↔ Database tables
- C# properties ↔ Database columns
- C# LINQ queries ↔ SQL queries

**Think of it as a translator:**
```
You (C# Code)          ORM (EF Core)          Database (SQL)
"Get all trips"   →   Translates to SQL   →   SELECT * FROM Trips
```

### What is Entity Framework Core?

**Entity Framework Core (EF Core)** is Microsoft's ORM for .NET. It:
- Maps C# classes to database tables
- Converts LINQ queries to SQL
- Tracks changes to objects
- Saves changes to the database
- Handles database connections

**Benefits:**
- ✅ Write C# code, not SQL
- ✅ Type-safe queries (compiler catches errors)
- ✅ Automatic change tracking
- ✅ Database-agnostic (works with SQL Server, PostgreSQL, MySQL, etc.)
- ✅ Migrations (version control for database schema)

**How it works:**
```
Your C# Code
    ↓
EF Core (ORM)
    ↓
Database Provider (SQL Server, PostgreSQL, etc.)
    ↓
Database
```

---

## Step 1: Add EF Core NuGet Packages

### What are NuGet Packages?

**NuGet** is the package manager for .NET. It's like an app store for code libraries. Packages contain:
- Pre-written code you can use
- Dependencies (other packages they need)
- Documentation

### Why Do We Need These Packages?

**Microsoft.EntityFrameworkCore.SqlServer:**
- Provides EF Core functionality for SQL Server
- Contains the database provider
- Handles SQL Server-specific features

**Microsoft.EntityFrameworkCore.Design:**
- Provides design-time tools
- Enables migrations (creating database schema)
- Required for `dotnet ef` commands

### Adding Packages via Command Line

**For SQL Server:**
```bash
dotnet add SafeBoda.Api/SafeBoda.Api.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add SafeBoda.Api/SafeBoda.Api.csproj package Microsoft.EntityFrameworkCore.Design
```

**For PostgreSQL (Alternative):**
```bash
dotnet add SafeBoda.Api/SafeBoda.Api.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add SafeBoda.Api/SafeBoda.Api.csproj package Microsoft.EntityFrameworkCore.Design
```

**Breaking down the command:**
- **`dotnet add`**: Add a package to a project
- **`SafeBoda.Api/SafeBoda.Api.csproj`**: Target project
- **`package`**: We're adding a package
- **`Microsoft.EntityFrameworkCore.SqlServer`**: Package name

### What Gets Added to Project File

After adding packages, your `SafeBoda.Api.csproj` will have:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.10" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.10" />
</ItemGroup>
```

**Explanation:**
- **`<ItemGroup>`**: Container for package references
- **`PackageReference`**: A NuGet package dependency
- **`Include`**: Package name
- **`Version`**: Package version (automatically resolved)

**Note**: The actual project uses PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`), but the concepts are the same for SQL Server.

---

## Step 2: Create Infrastructure Class Library

### Why a Separate Project?

**Separation of Concerns:**
- **Core**: Domain models (pure business logic)
- **Application**: Business logic interfaces
- **Infrastructure**: Data access, external services
- **Api**: Web API endpoints

**Benefits:**
- ✅ Easy to swap databases (change Infrastructure, not Core)
- ✅ Testable (can mock Infrastructure)
- ✅ Organized (clear where data access code lives)
- ✅ Reusable (other projects can use Infrastructure)

### The Command

```bash
dotnet new classlib -n SafeBoda.Infrastructure
```

**What this creates:**
- `SafeBoda.Infrastructure/` folder
- `SafeBoda.Infrastructure.csproj` project file
- Default `Class1.cs` (we'll delete this)

### Adding to Solution

```bash
dotnet sln add SafeBoda.Infrastructure/SafeBoda.Infrastructure.csproj
```

### Adding Project References

The Infrastructure project needs:
- **SafeBoda.Core**: To use domain models (Trip, Rider, Driver)
- **SafeBoda.Application**: To implement ITripRepository

```bash
dotnet add SafeBoda.Infrastructure/SafeBoda.Infrastructure.csproj reference SafeBoda.Core/SafeBoda.Core.csproj
dotnet add SafeBoda.Infrastructure/SafeBoda.Infrastructure.csproj reference SafeBoda.Application/SafeBoda.Application.csproj
```

### Adding EF Core to Infrastructure

```bash
dotnet add SafeBoda.Infrastructure/SafeBoda.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add SafeBoda.Infrastructure/SafeBoda.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
```

**For SQL Server:**
```bash
dotnet add SafeBoda.Infrastructure/SafeBoda.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
```

**For PostgreSQL:**
```bash
dotnet add SafeBoda.Infrastructure/SafeBoda.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
```

### Project Structure

After setup, your Infrastructure project should have:
```
SafeBoda.Infrastructure/
├── SafeBoda.Infrastructure.csproj
├── Data/
│   └── SafeBodaDbContext.cs
├── Entities/
│   ├── TripEntity.cs
│   ├── RiderEntity.cs
│   └── DriverEntity.cs
└── Repositories/
    └── EfTripRepository.cs
```

---

## Step 3: Create the DbContext Class

### What is a DbContext?

**DbContext** is the **bridge** between your C# code and the database. Think of it as:
- A **session** with the database
- A **workspace** where you work with data
- A **translator** between objects and tables

**Responsibilities:**
- Manages database connection
- Tracks changes to entities
- Executes queries
- Saves changes to database
- Handles transactions

### Creating SafeBodaDbContext

Create `SafeBoda.Infrastructure/Data/SafeBodaDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using SafeBoda.Infrastructure.Entities;

namespace SafeBoda.Infrastructure.Data;

public class SafeBodaDbContext : DbContext
{
    public SafeBodaDbContext(DbContextOptions<SafeBodaDbContext> options) : base(options)
    {
    }

    public DbSet<TripEntity> Trips { get; set; }
    public DbSet<RiderEntity> Riders { get; set; }
    public DbSet<DriverEntity> Drivers { get; set; }
}
```

**Line-by-line explanation:**

**Line 1-2: Using Statements**
```csharp
using Microsoft.EntityFrameworkCore;
using SafeBoda.Infrastructure.Entities;
```
- **`Microsoft.EntityFrameworkCore`**: Provides `DbContext`, `DbSet`
- **`SafeBoda.Infrastructure.Entities`**: Our entity classes (TripEntity, etc.)

**Line 4: Namespace**
```csharp
namespace SafeBoda.Infrastructure.Data;
```
- Groups related classes together
- `Data` folder = data access code

**Line 6: Class Declaration**
```csharp
public class SafeBodaDbContext : DbContext
```
- **`public class`**: Public class (accessible from other projects)
- **`SafeBodaDbContext`**: Our custom context name
- **`: DbContext`**: Inherits from EF Core's `DbContext`
  - Gets all database functionality
  - Manages connections, queries, changes

**Line 8-10: Constructor**
```csharp
public SafeBodaDbContext(DbContextOptions<SafeBodaDbContext> options) : base(options)
{
}
```
- **`DbContextOptions<SafeBodaDbContext>`**: Configuration for this context
  - Contains connection string
  - Contains database provider (SQL Server, PostgreSQL, etc.)
- **`: base(options)`**: Passes options to parent `DbContext`
  - Parent class needs these options to connect to database

**Why this constructor?**
- EF Core uses **dependency injection**
- Framework creates `DbContextOptions` with connection string
- Framework passes it to constructor
- We pass it to base class

**Line 12-14: DbSet Properties**
```csharp
public DbSet<TripEntity> Trips { get; set; }
public DbSet<RiderEntity> Riders { get; set; }
public DbSet<DriverEntity> Drivers { get; set; }
```
- **`DbSet<T>`**: Represents a database table
  - `T` is the entity type (TripEntity, RiderEntity, etc.)
- **`Trips`**: Property name (becomes table name "Trips")
- **`{ get; set; }`**: Auto-property (getter and setter)

**What DbSet does:**
- Represents a table in the database
- Allows querying: `context.Trips.Where(...)`
- Allows adding: `context.Trips.Add(trip)`
- Allows removing: `context.Trips.Remove(trip)`

**The Mapping:**
```
DbSet<TripEntity> Trips  →  Database Table "Trips"
DbSet<RiderEntity> Riders →  Database Table "Riders"
DbSet<DriverEntity> Drivers → Database Table "Drivers"
```

### Advanced: OnModelCreating Method

For more control, you can override `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<TripEntity>(entity =>
    {
        entity.ToTable("Trips");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Fare).HasColumnType("decimal(18,2)");
    });
}
```

**What this does:**
- **`ToTable("Trips")`**: Explicitly set table name
- **`HasKey(e => e.Id)`**: Set primary key
- **`HasColumnType("decimal(18,2)")`**: Set column type and precision
- **`IsRequired()`**: Make column NOT NULL
- **`HasMaxLength(100)`**: Set maximum string length

**Why use it?**
- Fine-tune database schema
- Set constraints (required, max length)
- Configure relationships
- Set default values

---

## Step 4: Define DbSet Properties

### What are Entities?

**Entities** are C# classes that represent database tables. They're different from our domain models:

**Domain Model (Core):**
```csharp
public record Trip(Guid Id, Guid RiderId, Location Start, Location End, ...);
```
- Pure business logic
- No database concerns
- Immutable (record)

**Entity (Infrastructure):**
```csharp
public class TripEntity
{
    public Guid Id { get; set; }
    public Guid RiderId { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    // ...
}
```
- Database representation
- Flattened structure (no nested objects)
- Mutable (class with setters)

### Why Separate Entities?

**Separation of Concerns:**
- **Domain models**: Business logic (Core project)
- **Entities**: Database structure (Infrastructure project)

**Benefits:**
- Domain models stay pure (no database attributes)
- Can change database without changing domain
- Can have different structures (Location → Latitude/Longitude)

### Creating Entity Classes

**TripEntity.cs:**
```csharp
namespace SafeBoda.Infrastructure.Entities;

public class TripEntity
{
    public Guid Id { get; set; }
    public Guid RiderId { get; set; }
    public Guid DriverId { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
    public decimal Fare { get; set; }
    public DateTime RequestTime { get; set; }
}
```

**Explanation:**
- **`class`** (not `record`): Entities are mutable (EF Core needs to set properties)
- **`Id`**: Primary key (EF Core convention: property named "Id" = primary key)
- **`RiderId`, `DriverId`**: Foreign keys (references to other tables)
- **`StartLatitude`, `StartLongitude`**: Flattened Location (database can't store nested objects)
- **`Fare`**: `decimal` for money (precision important)
- **`RequestTime`**: `DateTime` for timestamps

**Why flatten Location?**
- Domain model: `Location Start` (nested object)
- Database: `StartLatitude`, `StartLongitude` (columns)
- Databases don't support nested objects directly
- We map between them in the repository

**RiderEntity.cs:**
```csharp
namespace SafeBoda.Infrastructure.Entities;

public class RiderEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
```

**DriverEntity.cs:**
```csharp
namespace SafeBoda.Infrastructure.Entities;

public class DriverEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string MotoPlateNumber { get; set; } = string.Empty;
}
```

### DbSet Properties in DbContext

```csharp
public class SafeBodaDbContext : DbContext
{
    public DbSet<TripEntity> Trips { get; set; }
    public DbSet<RiderEntity> Riders { get; set; }
    public DbSet<DriverEntity> Drivers { get; set; }
}
```

**What each DbSet does:**
- **`Trips`**: Represents the "Trips" table
  - Query: `context.Trips.Where(t => t.Fare > 1000)`
  - Add: `context.Trips.Add(newTrip)`
  - Remove: `context.Trips.Remove(trip)`

- **`Riders`**: Represents the "Riders" table
- **`Drivers`**: Represents the "Drivers" table

**The Mapping:**
```
C# Code                    Database
─────────────────────────────────────────
context.Trips              SELECT * FROM Trips
context.Trips.Add(trip)    INSERT INTO Trips ...
context.Trips.Remove(trip) DELETE FROM Trips ...
```

---

## Step 5: Configure Database Connection String

### What is a Connection String?

A **connection string** is a text that tells EF Core:
- Which database server to connect to
- Which database to use
- How to authenticate
- Other connection settings

**Format:**
```
Server=server_name;Database=database_name;User=username;Password=password;
```

### Adding to appsettings.json

Open `SafeBoda.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SafeBodaDb": "Server=localhost;Database=SafeBoda;Trusted_Connection=True;"
  }
}
```

**For SQL Server (Windows Authentication):**
```json
{
  "ConnectionStrings": {
    "SafeBodaDb": "Server=localhost;Database=SafeBoda;Trusted_Connection=True;"
  }
}
```

**For SQL Server (SQL Authentication):**
```json
{
  "ConnectionStrings": {
    "SafeBodaDb": "Server=localhost;Database=SafeBoda;User Id=sa;Password=YourPassword;"
  }
}
```

**For PostgreSQL:**
```json
{
  "ConnectionStrings": {
    "SafeBodaDb": "Host=localhost;Port=5432;Database=SafeBodaDb;Username=postgres;Password=YourPassword;"
  }
}
```

**Breaking down SQL Server connection string:**
- **`Server=localhost`**: Database server address
  - `localhost` = this computer
  - Could be `192.168.1.100` or `myserver.database.windows.net`
- **`Database=SafeBoda`**: Database name
  - Database will be created if it doesn't exist (with migrations)
- **`Trusted_Connection=True`**: Use Windows Authentication
  - Uses your Windows login
  - No username/password needed

**Breaking down PostgreSQL connection string:**
- **`Host=localhost`**: Database server
- **`Port=5432`**: PostgreSQL default port
- **`Database=SafeBodaDb`**: Database name
- **`Username=postgres`**: Database user
- **`Password=YourPassword`**: User password

### Security Note

**⚠️ Never commit connection strings with passwords to Git!**

**Use User Secrets (Development):**
```bash
dotnet user-secrets set "ConnectionStrings:SafeBodaDb" "Server=...;Password=..."
```

**Use Environment Variables (Production):**
```bash
export ConnectionStrings__SafeBodaDb="Server=...;Password=..."
```

**Or use appsettings.Development.json** (not committed to Git):
```json
{
  "ConnectionStrings": {
    "SafeBodaDb": "Your actual connection string here"
  }
}
```

---

## Step 6: Register the DbContext

### Why Register DbContext?

**Dependency Injection** needs to know:
- What type to create (`SafeBodaDbContext`)
- How to create it (with connection string)
- What lifetime to use (Scoped = one per request)

### Registering in Program.cs

Open `SafeBoda.Api/Program.cs` and add:

**For SQL Server:**
```csharp
builder.Services.AddDbContext<SafeBodaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SafeBodaDb")));
```

**For PostgreSQL:**
```csharp
builder.Services.AddDbContext<SafeBodaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SafeBodaDb")));
```

**Line-by-line explanation:**

**Line 1: AddDbContext**
```csharp
builder.Services.AddDbContext<SafeBodaDbContext>(...)
```
- **`AddDbContext<T>`**: Registers DbContext with DI container
- **`<SafeBodaDbContext>`**: The type to register
- **Generic method**: Type parameter in `<>`

**Line 2: Options Configuration**
```csharp
options => options.UseSqlServer(...)
```
- **`options`**: `DbContextOptionsBuilder` object
  - Used to configure the DbContext
- **`UseSqlServer(...)`**: Tells EF Core to use SQL Server
  - Alternative: `UseNpgsql(...)` for PostgreSQL
  - Alternative: `UseInMemoryDatabase(...)` for testing

**Line 3: Connection String**
```csharp
builder.Configuration.GetConnectionString("SafeBodaDb")
```
- **`builder.Configuration`**: Access to configuration (appsettings.json)
- **`GetConnectionString("SafeBodaDb")`**: Gets connection string by key
  - Looks in `ConnectionStrings:SafeBodaDb` in appsettings.json
- Returns the connection string we defined earlier

**What happens:**
1. Framework reads `appsettings.json`
2. Finds `ConnectionStrings:SafeBodaDb`
3. Creates `DbContextOptions` with connection string
4. Registers `SafeBodaDbContext` in DI container
5. When controller needs it, framework creates instance with connection

### Complete Registration Example

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<SafeBodaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SafeBodaDb")));

// Register repository (we'll update this in Step 9)
builder.Services.AddScoped<ITripRepository, EfTripRepository>();

var app = builder.Build();
// ...
```

**Lifetime: Scoped**
- `AddDbContext` uses **Scoped** lifetime by default
- One `DbContext` per HTTP request
- Safe for concurrent requests
- Automatically disposed after request

---

## Step 7: Create and Apply Migrations

### What are Migrations?

**Migrations** are like **version control for your database schema**. They:
- Track changes to database structure
- Can be applied to create/update database
- Can be rolled back if needed
- Are code files (can be reviewed, tested)

**Think of it like:**
- **Git commits** for database structure
- Each migration = one change (add table, add column, etc.)
- Can apply migrations in order
- Can rollback if something goes wrong

### Why Use Migrations?

**Without Migrations:**
- Manual SQL scripts
- Hard to track changes
- Hard to apply to multiple environments
- Risk of forgetting changes

**With Migrations:**
- ✅ Automatic SQL generation
- ✅ Version controlled (in Git)
- ✅ Easy to apply to dev/staging/production
- ✅ Can rollback if needed

### Installing EF Core Tools

First, install the EF Core command-line tools:

```bash
dotnet tool install --global dotnet-ef
```

**What this does:**
- Installs `dotnet ef` command globally
- Required for migration commands

### Creating a Migration

Navigate to your project directory and run:

```bash
dotnet ef migrations add InitialCreate --project SafeBoda.Infrastructure --startup-project SafeBoda.Api
```

**Breaking it down:**
- **`dotnet ef`**: EF Core command-line tool
- **`migrations add`**: Create a new migration
- **`InitialCreate`**: Migration name (descriptive)
- **`--project SafeBoda.Infrastructure`**: Where migrations are stored
- **`--startup-project SafeBoda.Api`**: Where to read configuration (appsettings.json)

**What this does:**
1. Scans `SafeBodaDbContext` for `DbSet` properties
2. Compares with current database (or empty if first time)
3. Generates SQL to create tables
4. Creates migration files in `Migrations/` folder

### Migration Files Created

After running the command, you'll see:

```
SafeBoda.Infrastructure/Migrations/
├── 20251125233939_InitialCreate.cs
├── 20251125233939_InitialCreate.Designer.cs
└── SafeBodaDbContextModelSnapshot.cs
```

**File names:**
- **`20251125233939_InitialCreate.cs`**: 
  - Timestamp: `20251125233939` (YYYYMMDDHHMMSS)
  - Name: `InitialCreate`
  - Contains `Up()` and `Down()` methods

**Up() Method:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Trips",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            RiderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            // ... more columns
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Trips", x => x.Id);
        });
}
```

**What Up() does:**
- Applies the migration (creates tables, columns, etc.)
- Run when you execute `Update-Database`

**Down() Method:**
```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "Trips");
}
```

**What Down() does:**
- Rolls back the migration (removes tables, columns, etc.)
- Run when you rollback migrations

**ModelSnapshot.cs:**
- Current state of your database model
- Used to generate next migration
- Compares current model with snapshot to detect changes

### Applying the Migration

Run this command to create/update the database:

```bash
dotnet ef database update --project SafeBoda.Infrastructure --startup-project SafeBoda.Api
```

**What this does:**
1. Connects to database (using connection string)
2. Checks which migrations have been applied
3. Applies any pending migrations
4. Creates database if it doesn't exist
5. Creates tables based on `DbSet` properties

**Result:**
- Database is created (if it doesn't exist)
- Tables are created: `Trips`, `Riders`, `Drivers`
- Columns match your entity properties
- Primary keys and constraints are set

### Verifying the Migration

**Check database:**
- Open SQL Server Management Studio (or pgAdmin for PostgreSQL)
- Connect to your database
- You should see:
  - Database: `SafeBoda` (or your database name)
  - Tables: `Trips`, `Riders`, `Drivers`
  - Columns matching your entities

**Check migration history:**
- EF Core creates `__EFMigrationsHistory` table
- Tracks which migrations have been applied
- Prevents applying same migration twice

### Common Migration Commands

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project SafeBoda.Infrastructure --startup-project SafeBoda.Api

# Apply migrations
dotnet ef database update --project SafeBoda.Infrastructure --startup-project SafeBoda.Api

# Rollback to previous migration
dotnet ef database update PreviousMigrationName --project SafeBoda.Infrastructure --startup-project SafeBoda.Api

# Remove last migration (if not applied)
dotnet ef migrations remove --project SafeBoda.Infrastructure --startup-project SafeBoda.Api

# List migrations
dotnet ef migrations list --project SafeBoda.Infrastructure --startup-project SafeBoda.Api
```

---

## Step 8: Implement EfTripRepository

### Why a New Repository?

We need to replace `InMemoryTripRepository` with a database-backed version. The interface stays the same, but the implementation changes:

**Before (InMemory):**
```csharp
private readonly List<Trip> _trips = new();
// Data in RAM
```

**After (Database):**
```csharp
private readonly SafeBodaDbContext _context;
// Data in database
```

### Creating EfTripRepository

Create `SafeBoda.Infrastructure/Repositories/EfTripRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using SafeBoda.Application;
using SafeBoda.Core;
using SafeBoda.Infrastructure.Data;
using SafeBoda.Infrastructure.Entities;

namespace SafeBoda.Infrastructure.Repositories;

public class EfTripRepository : ITripRepository
{
    private readonly SafeBodaDbContext _context;

    public EfTripRepository(SafeBodaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Trip>> GetActiveTripsAsync()
    {
        var entities = await _context.Trips.ToListAsync();
        return entities.Select(MapToTripDomain);
    }

    public async Task<Trip?> GetTripByIdAsync(Guid id)
    {
        var entity = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
        return entity == null ? null : MapToTripDomain(entity);
    }

    public async Task<Trip> CreateTripAsync(Trip trip)
    {
        var entity = MapToTripEntity(trip);
        _context.Trips.Add(entity);
        await _context.SaveChangesAsync();
        return trip;
    }

    // ... more methods
}
```

**Line-by-line explanation:**

**Line 1-5: Using Statements**
```csharp
using Microsoft.EntityFrameworkCore;
using SafeBoda.Application;
using SafeBoda.Core;
using SafeBoda.Infrastructure.Data;
using SafeBoda.Infrastructure.Entities;
```
- **`Microsoft.EntityFrameworkCore`**: For `ToListAsync()`, `FirstOrDefaultAsync()`, etc.
- **`SafeBoda.Application`**: For `ITripRepository` interface
- **`SafeBoda.Core`**: For domain models (`Trip`, `Rider`, `Driver`)
- **`SafeBoda.Infrastructure.Data`**: For `SafeBodaDbContext`
- **`SafeBoda.Infrastructure.Entities`**: For entity classes

**Line 9: Class Declaration**
```csharp
public class EfTripRepository : ITripRepository
```
- **`EfTripRepository`**: Entity Framework Trip Repository
- **`: ITripRepository`**: Implements the interface
  - Must implement all interface methods

**Line 11-12: DbContext Field**
```csharp
private readonly SafeBodaDbContext _context;

public EfTripRepository(SafeBodaDbContext context)
{
    _context = context;
}
```
- **`_context`**: Database context (injected via DI)
- **Constructor**: Receives `SafeBodaDbContext` from DI container
- Framework provides it automatically

**Line 18-21: GetActiveTripsAsync**
```csharp
public async Task<IEnumerable<Trip>> GetActiveTripsAsync()
{
    var entities = await _context.Trips.ToListAsync();
    return entities.Select(MapToTripDomain);
}
```
- **`_context.Trips`**: Access to Trips table
- **`ToListAsync()`**: Executes SQL query, returns list
  - SQL: `SELECT * FROM Trips`
  - Async: Doesn't block thread
- **`Select(MapToTripDomain)`**: Converts entities to domain models
  - Entity → Domain model mapping

**Line 24-27: GetTripByIdAsync**
```csharp
public async Task<Trip?> GetTripByIdAsync(Guid id)
{
    var entity = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
    return entity == null ? null : MapToTripDomain(entity);
}
```
- **`FirstOrDefaultAsync(t => t.Id == id)`**: Finds first matching trip
  - SQL: `SELECT * FROM Trips WHERE Id = @id`
  - Returns `null` if not found
- **`?`**: Nullable return type (trip might not exist)

**Line 30-35: CreateTripAsync**
```csharp
public async Task<Trip> CreateTripAsync(Trip trip)
{
    var entity = MapToTripEntity(trip);
    _context.Trips.Add(entity);
    await _context.SaveChangesAsync();
    return trip;
}
```
- **`MapToTripEntity(trip)`**: Converts domain model to entity
- **`_context.Trips.Add(entity)`**: Marks entity for insertion
  - Doesn't save yet (just tracks the change)
- **`SaveChangesAsync()`**: Executes SQL INSERT
  - SQL: `INSERT INTO Trips (...) VALUES (...)`
- **Returns**: Original domain model

**Line 38-59: UpdateTripAsync**
```csharp
public async Task<Trip> UpdateTripAsync(Trip trip)
{
    var existingEntity = await _context.Trips.FindAsync(trip.Id);
    if (existingEntity == null)
    {
        // Create if doesn't exist
        var entity = MapToTripEntity(trip);
        _context.Trips.Add(entity);
    }
    else
    {
        // Update existing
        existingEntity.RiderId = trip.RiderId;
        existingEntity.DriverId = trip.DriverId;
        existingEntity.StartLatitude = trip.Start.Latitude;
        existingEntity.StartLongitude = trip.Start.Longitude;
        existingEntity.EndLatitude = trip.End.Latitude;
        existingEntity.EndLongitude = trip.End.Longitude;
        existingEntity.Fare = trip.Fare;
        existingEntity.RequestTime = trip.RequestTime;
    }
    await _context.SaveChangesAsync();
    return trip;
}
```
- **`FindAsync(trip.Id)`**: Finds entity by primary key
  - Fast lookup (uses primary key index)
- **If not found**: Creates new entity
- **If found**: Updates properties
  - EF Core tracks changes automatically
- **`SaveChangesAsync()`**: Executes SQL UPDATE

**Line 91-100: DeleteTripAsync**
```csharp
public async Task<bool> DeleteTripAsync(Guid id)
{
    var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
    if (trip == null)
        return false;

    _context.Trips.Remove(trip);
    await _context.SaveChangesAsync();
    return true;
}
```
- **`FirstOrDefaultAsync`**: Finds trip
- **`Remove(trip)`**: Marks for deletion
- **`SaveChangesAsync()`**: Executes SQL DELETE
- **Returns**: `true` if deleted, `false` if not found

### Mapping Methods

**MapToTripDomain (Entity → Domain):**
```csharp
private static Trip MapToTripDomain(TripEntity entity)
{
    return new Trip(
        entity.Id,
        entity.RiderId,
        entity.DriverId,
        new Location(entity.StartLatitude, entity.StartLongitude),
        new Location(entity.EndLatitude, entity.EndLongitude),
        entity.Fare,
        entity.RequestTime
    );
}
```
- Converts flat entity to domain model
- Reconstructs `Location` objects from latitude/longitude

**MapToTripEntity (Domain → Entity):**
```csharp
private static TripEntity MapToTripEntity(Trip trip)
{
    return new TripEntity
    {
        Id = trip.Id,
        RiderId = trip.RiderId,
        DriverId = trip.DriverId,
        StartLatitude = trip.Start.Latitude,
        StartLongitude = trip.Start.Longitude,
        EndLatitude = trip.End.Latitude,
        EndLongitude = trip.End.Longitude,
        Fare = trip.Fare,
        RequestTime = trip.RequestTime
    };
}
```
- Converts domain model to flat entity
- Flattens `Location` objects to latitude/longitude

**Why mapping?**
- Keeps domain models pure (no database concerns)
- Allows different structures (Location vs Lat/Long)
- Separates business logic from data access

---

## Step 9: Update Dependency Injection

### Replacing InMemory with Database

In `SafeBoda.Api/Program.cs`, change:

**Before:**
```csharp
builder.Services.AddScoped<ITripRepository, InMemoryTripRepository>();
```

**After:**
```csharp
builder.Services.AddScoped<ITripRepository, EfTripRepository>();
```

**That's it!** The interface stays the same, so the rest of the code doesn't need to change.

### Complete Program.cs Example

```csharp
using SafeBoda.Application;
using SafeBoda.Infrastructure.Data;
using SafeBoda.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<SafeBodaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SafeBodaDb")));

// Register repository (now using database!)
builder.Services.AddScoped<ITripRepository, EfTripRepository>();

builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();
app.Run();
```

**What changed:**
- **Repository**: `InMemoryTripRepository` → `EfTripRepository`
- **Added**: `AddDbContext` registration
- **Everything else**: Stays the same!

**Why this works:**
- Both repositories implement `ITripRepository`
- Controllers use the interface, not concrete class
- DI container provides the implementation
- We just swapped implementations

---

## Step 10: Verify API Functionality

### Testing the API

**1. Start the API:**
```bash
cd SafeBoda.Api
dotnet run
```

**2. Test GET endpoint:**
```bash
curl https://localhost:7244/api/trips
```

**Expected**: Empty array `[]` (database is empty)

**3. Test POST endpoint:**
```bash
curl -X POST https://localhost:7244/api/trips/request \
  -H "Content-Type: application/json" \
  -d '{
    "riderId": "550e8400-e29b-41d4-a716-446655440000",
    "start": { "latitude": -1.9441, "longitude": 30.0619 },
    "end": { "latitude": -1.9706, "longitude": 30.1044 }
  }'
```

**Expected**: Returns created trip with ID

**4. Test GET again:**
```bash
curl https://localhost:7244/api/trips
```

**Expected**: Array with one trip (the one you just created)

**5. Restart the API:**
- Stop the API (Ctrl+C)
- Start it again (`dotnet run`)
- Test GET again

**Expected**: Trip is still there! ✅ (Data persisted!)

### Verifying in Database

**SQL Server:**
```sql
USE SafeBoda;
SELECT * FROM Trips;
```

**PostgreSQL:**
```sql
\c SafeBodaDb
SELECT * FROM "Trips";
```

**Expected**: You should see the trip you created via API

### Common Issues

**Issue: "Cannot open database"**
- Check connection string
- Ensure database server is running
- Check authentication (username/password)

**Issue: "Table doesn't exist"**
- Run migrations: `dotnet ef database update`
- Check if migration was applied

**Issue: "Invalid column name"**
- Entity properties don't match database columns
- Check mapping in `OnModelCreating`
- Create new migration if entity changed

**Issue: "Timeout expired"**
- Database server not accessible
- Check firewall settings
- Verify connection string

---

## Understanding Database Entities

### Entity vs Domain Model

**Domain Model (Core):**
```csharp
public record Trip(
    Guid Id,
    Guid RiderId,
    Location Start,  // Nested object
    Location End,
    decimal Fare
);
```
- Business logic representation
- Immutable (record)
- Can have nested objects
- No database attributes

**Entity (Infrastructure):**
```csharp
public class TripEntity
{
    public Guid Id { get; set; }
    public Guid RiderId { get; set; }
    public double StartLatitude { get; set; }  // Flattened
    public double StartLongitude { get; set; }
    public decimal Fare { get; set; }
}
```
- Database representation
- Mutable (class with setters)
- Flat structure (no nested objects)
- Can have database attributes

### Why Two Representations?

**Separation of Concerns:**
- **Domain**: Business rules, logic
- **Infrastructure**: Database structure, persistence

**Benefits:**
- Domain stays pure (no database dependencies)
- Can change database without changing domain
- Can have different structures (Location → Lat/Long)

**Mapping:**
- Repository converts between them
- `MapToTripDomain`: Entity → Domain
- `MapToTripEntity`: Domain → Entity

---

## Understanding Migrations

### What Migrations Do

**Migrations track database schema changes:**
1. You change entity classes
2. Create migration: `dotnet ef migrations add AddStatusColumn`
3. Migration file generated with SQL changes
4. Apply migration: `dotnet ef database update`
5. Database schema updated

### Migration Workflow

```
1. Change Entity
   ↓
2. Create Migration
   dotnet ef migrations add MigrationName
   ↓
3. Review Migration File
   (Check generated SQL)
   ↓
4. Apply Migration
   dotnet ef database update
   ↓
5. Database Updated!
```

### Migration Best Practices

**✅ Do:**
- Give migrations descriptive names
- Review migration files before applying
- Test migrations on development first
- Commit migration files to Git
- Apply migrations in order

**❌ Don't:**
- Edit applied migrations (create new ones instead)
- Delete migration files (breaks history)
- Apply migrations directly to production (use deployment process)

### Rolling Back Migrations

**Rollback to previous migration:**
```bash
dotnet ef database update PreviousMigrationName
```

**Remove last migration (if not applied):**
```bash
dotnet ef migrations remove
```

**⚠️ Warning**: Rolling back can cause data loss if schema changes removed columns/tables!

---

## Summary

In Step 3, we:

1. ✅ Added EF Core NuGet packages
2. ✅ Created `SafeBoda.Infrastructure` project
3. ✅ Created `SafeBodaDbContext` class
4. ✅ Defined `DbSet` properties for entities
5. ✅ Configured database connection string
6. ✅ Registered `DbContext` with DI
7. ✅ Created and applied migrations
8. ✅ Implemented `EfTripRepository`
9. ✅ Updated DI to use database repository
10. ✅ Verified API saves data permanently

**Key Concepts Learned:**
- **Data Persistence**: Data survives application restarts
- **Entity Framework Core**: ORM that maps objects to database
- **DbContext**: Bridge between code and database
- **DbSet**: Represents a database table
- **Migrations**: Version control for database schema
- **Entities**: Database representation of domain models
- **Repository Pattern**: Abstraction for data access

**The Transformation:**
- **Before**: Data in memory → Lost on restart ❌
- **After**: Data in database → Persists forever ✅

**What We Built:**
- Database-backed storage
- Automatic schema management (migrations)
- Type-safe database queries
- Foundation for production-ready application

**What's Next?**
In future steps, we'll:
- Add more entities and relationships
- Add indexes for performance
- Add data validation
- Add authentication and authorization
- Deploy to cloud with managed database

---

## Common Questions

**Q: Why use EF Core instead of writing SQL directly?**
A: EF Core provides type safety, automatic change tracking, migrations, and database-agnostic code. Writing SQL directly is error-prone and database-specific.

**Q: What's the difference between `ToListAsync()` and `ToList()`?**
A: `ToListAsync()` is asynchronous (doesn't block), better for web APIs. `ToList()` is synchronous (blocks thread).

**Q: Why do we need entities if we have domain models?**
A: Entities represent database structure (flat, mutable). Domain models represent business logic (can be nested, immutable). Separation of concerns.

**Q: What happens if I change an entity after migration?**
A: Create a new migration. EF Core will detect changes and generate SQL to update the database schema.

**Q: Can I use the same DbContext in multiple threads?**
A: No! DbContext is not thread-safe. Use one per HTTP request (Scoped lifetime).

**Q: What's the difference between `Add()` and `Update()`?**
A: `Add()` marks entity as new (INSERT). `Update()` marks all properties as modified (UPDATE). Use `FindAsync()` + modify properties for better performance.

**Q: Why use `SaveChangesAsync()` instead of `SaveChanges()`?**
A: Async version doesn't block the thread, better for scalability in web applications.

**Q: What if my connection string has a password?**
A: Use User Secrets (development) or Environment Variables (production). Never commit passwords to Git!

---

## Conclusion

Congratulations! You've transformed SafeBoda from a temporary in-memory application to a **persistent, database-backed system**! 🎉

**What you've achieved:**
- Data now persists across restarts
- Professional database structure
- Automatic schema management
- Type-safe database access
- Foundation for scaling

**The journey:**
- **Step 1**: Domain models and interfaces
- **Step 2**: Web API endpoints
- **Step 3**: Database persistence ✅

Your application is now **production-ready** in terms of data storage! Users' data will be safe, even if the server restarts or crashes.

**Remember**: A good database design is the foundation of a reliable application. The work you've done here will support all future features! 🚀

