# SQL Server Connection Test Script
# Run this in PowerShell to test your SQL Server connection

Write-Host "=== SQL Server Connection Test ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check SQL Server Services
Write-Host "1. Checking SQL Server Services..." -ForegroundColor Yellow
$sqlServices = Get-Service | Where-Object {$_.Name -like "*SQL*"} | Select-Object Name, DisplayName, Status
$sqlServices | Format-Table -AutoSize
Write-Host ""

# Test 2: Test SQL Server Express Connection
Write-Host "2. Testing SQL Server Express Connection..." -ForegroundColor Yellow
try {
    $result = sqlcmd -S .\SQLEXPRESS -Q "SELECT @@VERSION AS 'SQL Server Version'" -E -h -1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Successfully connected to SQL Server Express!" -ForegroundColor Green
        Write-Host $result
    } else {
        Write-Host "? Failed to connect to SQL Server Express" -ForegroundColor Red
    }
} catch {
    Write-Host "? Error connecting to SQL Server Express: $_" -ForegroundColor Red
}
Write-Host ""

# Test 3: List Databases
Write-Host "3. Listing Databases..." -ForegroundColor Yellow
try {
    $databases = sqlcmd -S .\SQLEXPRESS -Q "SELECT name FROM sys.databases ORDER BY name" -E -h -1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Available databases:" -ForegroundColor Green
        $databases | ForEach-Object { Write-Host "  - $_" }
    }
} catch {
    Write-Host "? Error listing databases: $_" -ForegroundColor Red
}
Write-Host ""

# Test 4: Check if WefaaqDb exists
Write-Host "4. Checking for Wefaaq Databases..." -ForegroundColor Yellow
try {
    $wefaaqDbs = sqlcmd -S .\SQLEXPRESS -Q "SELECT name FROM sys.databases WHERE name LIKE 'Wefaaq%'" -E -h -1
    if ($wefaaqDbs) {
        Write-Host "Found Wefaaq databases:" -ForegroundColor Green
        $wefaaqDbs | ForEach-Object { Write-Host "  - $_" }
    } else {
        Write-Host "No Wefaaq databases found (they will be created on first migration)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "? Error checking for Wefaaq databases: $_" -ForegroundColor Red
}
Write-Host ""

# Test 5: Check TCP/IP Protocol
Write-Host "5. SQL Server Configuration Notes:" -ForegroundColor Yellow
Write-Host "  - Instance Name: SQLEXPRESS" -ForegroundColor White
Write-Host "  - Connection String Format: Server=.\SQLEXPRESS;..." -ForegroundColor White
Write-Host "  - Authentication: Windows Authentication (Integrated Security)" -ForegroundColor White
Write-Host ""

Write-Host "=== Test Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Run 'Update-Database' in Package Manager Console"
Write-Host "2. Or run 'dotnet ef database update --project Wefaaq.Dal --startup-project Wefaaq.Api'"
Write-Host ""
