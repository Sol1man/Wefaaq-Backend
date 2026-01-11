# ========================================
# Test Wefaaq Backend with Azure SQL Database
# ========================================
# This script runs your local backend connected to Azure SQL
# without modifying any configuration files
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Wefaaq Backend with Azure SQL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# IMPORTANT: Replace this with your actual Azure SQL connection string
$azureConnectionString = "Server=tcp:wefaaq-server.database.windows.net,1433;Initial Catalog=wefaaq-prod-db;Persist Security Info=False;User ID=abdelrahman-wefaaq;Password=VA@q2GcqqR@hcde;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "Setting Azure SQL connection string..." -ForegroundColor Yellow
$env:CONNECTION_STRING = $azureConnectionString

Write-Host "Connection string set (masked for security)" -ForegroundColor Green
Write-Host ""

Write-Host "Starting Wefaaq API..." -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""
Write-Host "Once started, test these URLs:" -ForegroundColor Cyan
Write-Host "  - Swagger UI: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "  - Get Clients: http://localhost:5000/api/clients" -ForegroundColor White
Write-Host "  - Get Organizations: http://localhost:5000/api/organizations" -ForegroundColor White
Write-Host ""

# Run the API
dotnet run --project Wefaaq.Api
