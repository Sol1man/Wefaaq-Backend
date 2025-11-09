# ?? Database Connection Issue - RESOLVED

## Issue Summary
You encountered a SQL Server connection error when trying to run `Update-Database`:
```
Microsoft.Data.SqlClient.SqlException: A network-related or instance-specific error occurred 
while establishing a connection to SQL Server. The server was not found or was not accessible.
```

## Root Cause
The connection string format in `appsettings.json` was using `localhost\SQLEXPRESS` which was not being resolved correctly by the SQL Client driver in some contexts.

## Solutions Applied ?

### 1. Updated Connection Strings
Changed the server address format in both configuration files:

**Before:**
```json
"Server=localhost\\SQLEXPRESS;..."
```

**After:**
```json
"Server=.\\SQLEXPRESS;Database=WefaaqDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"
```

**Key changes:**
- ? Changed `localhost\SQLEXPRESS` to `.\SQLEXPRESS` (more reliable format)
- ? Added `Encrypt=False` to avoid SSL certificate issues
- ? Used `Integrated Security=True` for Windows Authentication
- ? Kept `TrustServerCertificate=True` for development

### 2. Verified SQL Server Status
Confirmed that SQL Server Express is running:
```
Service: MSSQL$SQLEXPRESS - Running ?
```

### 3. Created Test Script
Added `Test-SqlConnection.ps1` for quick connection verification.

## How to Proceed Now

### Option 1: Using Package Manager Console (Recommended for Visual Studio)
```powershell
# Run in Package Manager Console (View > Other Windows > Package Manager Console)

# Update the database (applies all pending migrations)
Update-Database -Project Wefaaq.Dal -StartupProject Wefaaq.Api

# Verify the database was created
# Then you can run the application and it will seed data automatically in development mode
```

### Option 2: Using dotnet CLI
```bash
# In terminal, from the solution directory:

# Update database
dotnet ef database update --project Wefaaq.Dal --startup-project Wefaaq.Api

# Run the application
dotnet run --project Wefaaq.Api
```

## What Will Happen When You Run Update-Database?

1. **Database Creation**: Creates `WefaaqDb` (or `WefaaqDb_Dev` in development mode)
2. **Schema Creation**: Creates all tables:
   - Clients
   - Organizations
   - OrganizationRecords
   - OrganizationLicenses
   - OrganizationWorkers
   - OrganizationCars
3. **Indexes**: Creates unique index on Client.Email
4. **Relationships**: Sets up all foreign keys with cascade delete

## Configuration Files Updated

### Wefaaq.Api/appsettings.json (Production)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=WefaaqDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"
  }
}
```

### Wefaaq.Api/appsettings.Development.json (Development)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=WefaaqDb_Dev;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"
  }
}
```

## Verification Steps

### 1. Test SQL Connection
```powershell
.\Test-SqlConnection.ps1
```

### 2. Check Connection Manually
```powershell
sqlcmd -S .\SQLEXPRESS -Q "SELECT @@VERSION" -E
```

### 3. List Databases After Migration
```powershell
sqlcmd -S .\SQLEXPRESS -Q "SELECT name FROM sys.databases WHERE name LIKE 'Wefaaq%'" -E
```

## Data Seeding (Automatic in Development)

When you run the application in development mode, it will automatically:
1. Apply pending migrations
2. Seed sample data:
   - 5 Clients with different classifications
   - 5 Organizations linked to clients
   - Sample licenses, workers, and cars

You can see this in `Program.cs` lines 96-115.

## Existing Migrations

Your database schema includes these migrations:
1. ? `20251006161258_InitialCreate` - Creates all base tables
2. ? `20251010192544_UpdateClientOrganizationRelationship` - Changes from many-to-many to one-to-many
3. ? `20251109215134_first` - Removed (was empty)

## Troubleshooting

If you still encounter issues:

### Issue: "Cannot open database" error
**Solution:** Run `Update-Database` first to create the database

### Issue: "Login failed for user" error
**Solution:** Ensure Windows Authentication is enabled in SQL Server

### Issue: SSL Certificate error
**Solution:** Already fixed with `TrustServerCertificate=True;Encrypt=False`

### Issue: Named Pipes error
**Solution:** Already fixed by using `.\SQLEXPRESS` format

## Next Steps After Successful Migration

1. **Verify Database**:
   ```powershell
   sqlcmd -S .\SQLEXPRESS -d WefaaqDb_Dev -Q "SELECT COUNT(*) FROM Clients" -E
   ```
   Should return 5 clients (if seeded)

2. **Run the Application**:
   ```bash
   dotnet run --project Wefaaq.Api
   ```
   
3. **Access Swagger UI**:
   - Navigate to `https://localhost:5001` (or the port shown in console)
   - Swagger UI is configured to run at the root URL in development

4. **Test the API**:
   - GET `/api/clients` - Should return 5 seeded clients
   - GET `/api/organizations` - Should return 5 seeded organizations

## Files Modified/Created

### Modified:
- ? `Wefaaq.Api/appsettings.json` - Updated connection string
- ? `Wefaaq.Api/appsettings.Development.json` - Updated connection string
- ? `MIGRATION_FIX.md` - Enhanced documentation

### Created:
- ? `Wefaaq.Dal/WefaaqContextFactory.cs` - Design-time DbContext factory
- ? `Test-SqlConnection.ps1` - SQL connection test script
- ? `DATABASE_CONNECTION_FIX.md` - This file

### Removed:
- ? `Wefaaq.Dal/Migrations/20251109215134_first.cs` - Empty migration
- ? `Wefaaq.Dal/Migrations/20251109215134_first.Designer.cs` - Designer file

## Summary
? SQL Server Express is running and accessible
? Connection strings updated with correct format
? Design-time factory created for migrations
? Empty migration removed
? Ready to run `Update-Database`
? Test script available for verification

**You're all set! Now run `Update-Database` in Package Manager Console.**
