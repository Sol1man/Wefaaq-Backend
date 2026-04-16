# Development vs Production Environment Differences

This document outlines the differences between the Development (`development` branch) and Production (`master` branch) environments, and provides a checklist for merging development changes to production.

---

## Table of Contents
1. [Environment Overview](#environment-overview)
2. [Backend Configuration Differences](#backend-configuration-differences)
3. [Frontend Configuration Differences](#frontend-configuration-differences)
4. [Features Hidden in Production](#features-hidden-in-production)
5. [Pre-Merge Checklist](#pre-merge-checklist)
6. [Merge Steps](#merge-steps)
7. [Post-Merge Verification](#post-merge-verification)

---

## Environment Overview

| Aspect | Development | Production |
|--------|-------------|------------|
| **Branch** | `development` | `master` |
| **Backend Hosting** | localhost:7014 | Railway (wefaaq-backend-production.up.railway.app) |
| **Frontend Hosting** | localhost:4200 | Production Server |
| **Database** | WefaaqDb_Dev (Local) | WefaaqDb_Prod (Azure SQL) |
| **Auto Deploy** | No | Yes (on push to master) |

---

## Backend Configuration Differences

### Connection Strings

| Setting | Development (`appsettings.Development.json`) | Production (`appsettings.Production.json`) |
|---------|---------------------------------------------|-------------------------------------------|
| Server | `localhost` or local machine name | Azure SQL Server |
| Database | `WefaaqDb_Dev` | `WefaaqDb_Prod` |
| Authentication | Windows Auth / Local SQL | SQL Authentication (User ID & Password) |
| Encryption | `Encrypt=False` | `Encrypt=True` |
| Trust Certificate | `TrustServerCertificate=True` | `TrustServerCertificate=False` |

### Logging Levels

| Logger | Development | Production |
|--------|-------------|------------|
| Default | `Debug` / `Information` | `Warning` |
| Microsoft.AspNetCore | `Information` | `Warning` |
| Microsoft.EntityFrameworkCore | `Information` | `Warning` |

### Configuration Files

```
Wefaaq.Api/
├── appsettings.json                 # Base configuration (fallback)
├── appsettings.Development.json     # Development overrides
└── appsettings.Production.json      # Production overrides
```

**Important:** The correct configuration is automatically loaded based on the `ASPNETCORE_ENVIRONMENT` environment variable.

---

## Frontend Configuration Differences

### Environment Files

| Setting | Development (`environment.ts`) | Production (`environment.prod.ts`) |
|---------|-------------------------------|-----------------------------------|
| `production` | `false` | `true` |
| `apiUrl` | `https://localhost:7014/api` | `https://wefaaq-backend-production.up.railway.app/api` |
| `whiteList` | `['localhost:4200/admin', 'mange.template.com']` | `['localhost', 'mange.template.com']` |
| `blackList` | Has login routes | Empty `[]` |

### Configuration Files

```
src/environments/
├── environment.ts          # Development configuration
└── environment.prod.ts     # Production configuration
```

**Important:** Angular CLI automatically uses `environment.prod.ts` when building with `--configuration production`.

---

## Features Hidden in Production

The following features are only visible in Development mode:

| Feature | Location | Condition |
|---------|----------|-----------|
| Fill Mock Data Button | User Add/Edit Page | `*ngIf="!isProduction"` |

These features help with testing and development but should not be visible to end users.

---

## Pre-Merge Checklist

Before merging `development` to `master`, verify the following:

### Code Quality
- [ ] All new features are tested and working
- [ ] No console.log statements left in code (except error logging)
- [ ] No hardcoded development URLs or credentials
- [ ] All TypeScript/Angular compilation errors resolved
- [ ] All .NET build errors resolved

### Configuration
- [ ] `environment.prod.ts` has correct production API URL
- [ ] `appsettings.Production.json` has correct connection string (or uses environment variables)
- [ ] No development-only features visible in production build
- [ ] Firebase configuration is correct for production

### Database
- [ ] All migrations are created and tested
- [ ] No breaking changes to existing data
- [ ] Seed data is appropriate for production (if any)

### Security
- [ ] No sensitive data in source code
- [ ] API keys and secrets are in environment variables (not in code)
- [ ] CORS settings are appropriate for production

---

## Merge Steps

### Step 1: Ensure Development Branch is Up to Date
```bash
git checkout development
git pull origin development
```

### Step 2: Run Tests and Build Locally
```bash
# Backend
cd "Wefaaq Backend"
dotnet build --configuration Release
dotnet test

# Frontend
cd "Wefaaq_front"
npm install
ng build --configuration production
```

### Step 3: Apply Database Migrations to Production
If the database schema has changed (new tables, columns, relationships, etc.), you must apply migrations to the production database **before** or **immediately after** deploying the new code.

#### Option A: Apply Migrations via Command Line
```bash
cd "Wefaaq Backend"

# Apply migrations to production database
# Make sure to use the production connection string
dotnet ef database update --project Wefaaq.Dal --startup-project Wefaaq.Api --connection "Server=tcp:{your-azure-sql-server}.database.windows.net,1433;Database=WefaaqDb_Prod;User ID={username};Password={password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### Option B: Generate SQL Script and Run Manually
```bash
cd "Wefaaq Backend"

# Generate SQL script for pending migrations
dotnet ef migrations script --idempotent --project Wefaaq.Dal --startup-project Wefaaq.Api -o migration_script.sql
```
Then run the generated `migration_script.sql` on your production database using Azure Portal, SQL Server Management Studio, or Azure Data Studio.

#### Option C: Auto-Migration on Startup (Already Configured)
The application is configured to auto-apply migrations on startup in `Program.cs`. However, this only works if:
- The application has permission to modify the database schema
- The `ASPNETCORE_ENVIRONMENT` is set correctly

**Recommendation:** For production, Option B (SQL Script) is the safest approach as it allows you to review changes before applying them.

#### Verify Migration Status
```bash
# List pending migrations
dotnet ef migrations list --project Wefaaq.Dal --startup-project Wefaaq.Api
```

### Step 4: Commit Any Pending Changes
```bash
git add .
git commit -m "Prepare for production release"
```

### Step 5: Merge to Master
```bash
# Switch to master branch
git checkout master

# Pull latest master
git pull origin master

# Merge development into master
git merge development

# Resolve any merge conflicts if they occur
# Then commit the merge
```

### Step 6: Push to Master (Triggers Auto-Deploy)
```bash
git push origin master
```

### Step 7: Switch Back to Development
```bash
git checkout development
```

---

## Post-Merge Verification

After deploying to production, verify the following:

### Backend API
- [ ] API is accessible at production URL
- [ ] Health check endpoint responds (if available)
- [ ] Authentication is working
- [ ] Database connections are successful

### Frontend Application
- [ ] Application loads without errors
- [ ] Login functionality works
- [ ] All CRUD operations work (Clients, Organizations, etc.)
- [ ] No development features visible (e.g., Mock Data button)

### Monitoring
- [ ] Check application logs for errors
- [ ] Monitor database performance
- [ ] Verify no sensitive data in logs

---

## Rollback Procedure

If issues are found in production:

### Quick Rollback
```bash
# Revert to previous commit on master
git checkout master
git revert HEAD
git push origin master
```

### Full Rollback
```bash
# Reset master to a specific commit
git checkout master
git reset --hard <previous-stable-commit>
git push origin master --force
```

**Warning:** Force push should only be used in emergencies and with team coordination.

---

## Environment Variables

### Backend (Railway)
These should be set in the Railway dashboard:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=<production-connection-string>`

### Frontend
Environment is determined at build time by Angular CLI:
- Development: `ng serve` or `ng build`
- Production: `ng build --configuration production`

---

## Contact

For deployment issues or questions, contact the development team.

---

*Last Updated: January 2025*
