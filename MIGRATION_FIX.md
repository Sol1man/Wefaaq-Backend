# Entity Framework Migration Fix

## Problem
The error "Unable to resolve service for type 'Microsoft.EntityFrameworkCore.DbContextOptions`1[Wefaaq.Dal.WefaaqContext]'" occurred because EF Core tools couldn't create an instance of `WefaaqContext` at design time.

## Solution
Created a `WefaaqContextFactory` class that implements `IDesignTimeDbContextFactory<WefaaqContext>`. This factory:

1. Reads the connection string from `Wefaaq.Api/appsettings.json`
2. Creates a `DbContextOptionsBuilder` with SQL Server configuration
3. Returns a properly configured `WefaaqContext` instance

## What was added:

### File: `Wefaaq.Dal/WefaaqContextFactory.cs`
- Implements `IDesignTimeDbContextFactory<WefaaqContext>`
- Reads configuration from the API project's appsettings.json
- Provides EF Core tools with a way to instantiate the DbContext

### Package: `Microsoft.Extensions.Configuration.Json`
- Added to `Wefaaq.Dal` project
- Required for reading JSON configuration files

### Updated Connection Strings
Fixed connection string format in both appsettings files:
- Changed from `Server=localhost\SQLEXPRESS` to `Server=.\SQLEXPRESS`
- Added `Encrypt=False` to avoid SSL certificate issues
- Changed `Trusted_Connection=True` to `Integrated Security=True` (more explicit)

## How to use:

### Using Package Manager Console (Visual Studio):
```powershell
# Add a new migration
Add-Migration MigrationName -Project Wefaaq.Dal -StartupProject Wefaaq.Api

# Update database
Update-Database -Project Wefaaq.Dal -StartupProject Wefaaq.Api

# Remove last migration
Remove-Migration -Project Wefaaq.Dal -StartupProject Wefaaq.Api
```

### Using dotnet CLI:
```bash
# Install EF Core tools globally (one-time)
dotnet tool install --global dotnet-ef

# Add a new migration
dotnet ef migrations add MigrationName --project Wefaaq.Dal --startup-project Wefaaq.Api

# Update database
dotnet ef database update --project Wefaaq.Dal --startup-project Wefaaq.Api

# Remove last migration
dotnet ef migrations remove --project Wefaaq.Dal --startup-project Wefaaq.Api
```

## Connection String Troubleshooting

If you encounter connection errors, try these connection string variations:

### SQL Server Express (Named Instance)
```json
"Server=.\\SQLEXPRESS;Database=WefaaqDb;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
```

### SQL Server LocalDB
```json
"Server=(localdb)\\MSSQLLocalDB;Database=WefaaqDb;Integrated Security=True;TrustServerCertificate=True"
```

### SQL Server Default Instance
```json
"Server=localhost;Database=WefaaqDb;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
```

### Check SQL Server Status
```powershell
# Check if SQL Server is running
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Test connection
sqlcmd -S .\SQLEXPRESS -Q "SELECT @@VERSION" -E
```

## Why this works:
When you run EF Core commands, the tools look for:
1. A startup project with a `Program.cs` that configures the DbContext (Wefaaq.Api)
2. OR an `IDesignTimeDbContextFactory<T>` implementation in the DAL project

Since we created the factory, EF Core tools can now create the DbContext even without running the full application.

## Notes:
- The factory reads from `../Wefaaq.Api/appsettings.json` relative to the Wefaaq.Dal project directory
- Both `appsettings.json` and `appsettings.Development.json` are checked
- The same connection string from your API project is used
- SQL Server Express instance must be running
- Windows Authentication (Integrated Security) is used

## Verification:
? Solution builds successfully
? WefaaqContextFactory has no compilation errors
? SQL Server Express is running
? Connection string format updated
? Ready to create and apply migrations

## Current Connection String:
- **Production**: `Server=.\SQLEXPRESS;Database=WefaaqDb;...`
- **Development**: `Server=.\SQLEXPRESS;Database=WefaaqDb_Dev;...`
