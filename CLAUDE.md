# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Wefaaq is a .NET 8.0 Web API for managing clients (العملاء) and organizations (المؤسسات). The application uses Entity Framework Core with SQL Server for data persistence.

## Architecture

The solution follows a 3-layer architecture pattern:

- **Wefaaq.Api**: ASP.NET Core Web API layer with controllers, middleware, and DI configuration
- **Wefaaq.Bll**: Business Logic Layer containing services, DTOs, validators (FluentValidation), and AutoMapper profiles
- **Wefaaq.Dal**: Data Access Layer with Entity Framework Core entities, DbContext, and repository pattern implementations
- **Wefaaq.Tests**: xUnit test project

**Dependency flow**: Api → Bll → Dal (each layer only references the layer below it)

## Common Commands

### Build and Run
```bash
# Build entire solution
dotnet build

# Run the API (with automatic migrations and seeding in development)
dotnet run --project Wefaaq.Api

# Run with specific configuration
dotnet run --project Wefaaq.Api --configuration Release
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project Wefaaq.Dal --startup-project Wefaaq.Api

# Update database
dotnet ef database update --project Wefaaq.Dal --startup-project Wefaaq.Api

# Remove last migration
dotnet ef migrations remove --project Wefaaq.Dal --startup-project Wefaaq.Api

# Drop database
dotnet ef database drop --project Wefaaq.Dal --startup-project Wefaaq.Api
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with verbosity
dotnet test --verbosity detailed

# Run specific test project
dotnet test Wefaaq.Tests
```

## Key Architectural Patterns

### Repository Pattern
- Generic repository (`IGenericRepository<T>`) provides basic CRUD operations
- Specialized repositories (`IClientRepository`, `IOrganizationRepository`) extend generic repository with domain-specific queries
- All repositories are registered as scoped services

### Service Layer
- Services (`IClientService`, `IOrganizationService`) encapsulate business logic
- Services use AutoMapper to map between entities and DTOs
- FluentValidation validators are automatically applied via DI

### Middleware
- Global exception handler middleware (`ExceptionMiddleware`) provides centralized error handling
- Returns structured JSON error responses with appropriate HTTP status codes
- In development mode, includes stack traces and inner exception details

### Database Context
- `WefaaqContext` automatically updates timestamps (CreatedAt/UpdatedAt) via `SaveChanges` override
- Database seeding via `DataSeeder` class (runs automatically in development)
- Uses SQL Server with retry on failure enabled

## Domain Model

### Client Entity
- Represents customers with financial tracking (Balance property: negative = debtor, positive = creditor)
- Classification enum: `Mumayyaz`, `Aadi`, `Mahwari`
- Tracks external workers count
- One-to-many relationship with Organizations (a client can have multiple organizations)

### Organization Entity
- Represents business organizations
- Belongs to a single Client via `ClientId` foreign key
- Has collections of: Records, Licenses, Workers, Cars
- `CardExpiringSoon` flag for license expiration tracking

### Related Entities
- `OrganizationRecord`: Organization registration records (سجلات)
- `OrganizationLicense`: Business licenses (تراخيص) with expiry dates
- `OrganizationWorker`: Employee residence permits (إقامة) with expiry dates
- `OrganizationCar`: Company vehicles with operating card expiry dates

## Configuration

### Connection String
Connection string is read from `appsettings.json` or `appsettings.Development.json` under `ConnectionStrings:DefaultConnection`

### Swagger/OpenAPI
- Swagger UI is available at the root URL (`/`) in development mode
- API documentation generated from XML comments

### CORS
Currently configured with "AllowAll" policy for development (allows any origin/method/header)

## Development Notes

- The API automatically applies migrations and seeds data on startup in development mode (see Program.cs:96-115)
- Swagger UI is configured to run at the application root in development
- All timestamps use UTC
- Entity validation is performed at both the entity level (data annotations) and DTO level (FluentValidation)
