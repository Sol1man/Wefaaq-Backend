# Firebase Authentication Implementation Guide

## ? Completed Implementation

All necessary code changes have been successfully implemented for Firebase Authentication in your Wefaaq Backend API.

---

## ?? Installed Packages

The following NuGet packages have been added:

### Wefaaq.Api
```xml
<PackageReference Include="FirebaseAdmin" Version="3.0.0" />
<PackageReference Include="Google.Apis.Auth" Version="1.68.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
```

### Wefaaq.Bll
```xml
<PackageReference Include="FirebaseAdmin" Version="3.0.0" />
```

---

## ?? New Files Created

### 1. **Entities**
- `Wefaaq.Dal/Entities/User.cs` - User entity for authentication

### 2. **DTOs**
- `Wefaaq.Bll/DTOs/UserDto.cs` - User DTOs for requests/responses
  - UserDto
  - LoginRequestDto
  - LogoutRequestDto
  - LoginResponseDto
  - LogoutResponseDto

### 3. **Repository**
- `Wefaaq.Dal/RepositoriesInterfaces/IUserRepository.cs` - User repository interface
- `Wefaaq.Dal/Repositories/UserRepository.cs` - User repository implementation

### 4. **Service**
- `Wefaaq.Bll/Interfaces/IAuthService.cs` - Auth service interface
- `Wefaaq.Bll/Services/AuthService.cs` - Auth service implementation

### 5. **Controller**
- `Wefaaq.Api/Controllers/AuthController.cs` - Authentication endpoints

### 6. **Extensions**
- `Wefaaq.Api/Extensions/FirebaseAuthExtensions.cs` - Firebase configuration extension

---

## ?? Modified Files

### 1. **Wefaaq.Dal/WefaaqContext.cs**
- Added `DbSet<User> Users`
- Added User entity configuration in `OnModelCreating`
- Added User timestamp handling in `UpdateTimestamps`

### 2. **Wefaaq.Bll/Mappings/MappingProfile.cs**
- Added User ? UserDto mapping

### 3. **Wefaaq.Api/Program.cs**
- Registered IUserRepository and UserRepository
- Registered IAuthService and AuthService
- Added Firebase Authentication configuration
- Added `UseAuthentication()` middleware
- Updated Swagger configuration with JWT Bearer security

### 4. **Wefaaq.Api/appsettings.json**
- Added Firebase configuration section

---

## ??? Database Migration

### Manual Migration SQL (Run this in SQL Server)

```sql
-- Create Users table
CREATE TABLE [dbo].[Users] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [FirebaseUid] NVARCHAR(128) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [Name] NVARCHAR(255) NULL,
    [Role] NVARCHAR(50) NULL,
    [OrganizationId] UNIQUEIDENTIFIER NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastLoginAt] DATETIME2 NULL,
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Users_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId])
        REFERENCES [dbo].[Organizations] ([Id]) ON DELETE SET NULL
);

-- Create indexes
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_FirebaseUid] ON [dbo].[Users]([FirebaseUid] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]([Email] ASC);
CREATE NONCLUSTERED INDEX [IX_Users_OrganizationId] ON [dbo].[Users]([OrganizationId] ASC);
```

### Alternative: Using Entity Framework Migrations

If EF migrations work in your environment:

```bash
# Navigate to solution directory
cd D:\MyWork\Wefaaq-Backend

# Create migration
dotnet ef migrations add AddUserAuthentication --project Wefaaq.Dal --startup-project Wefaaq.Api

# Apply migration
dotnet ef database update --project Wefaaq.Dal --startup-project Wefaaq.Api
```

---

## ?? Firebase Configuration Setup

### Step 1: Get Firebase Service Account

1. Go to **Firebase Console** ? Your Project
2. Click ?? (Settings) ? **Project settings**
3. Go to **Service accounts** tab
4. Click **Generate new private key**
5. Save the JSON file as `firebase-service-account.json`

### Step 2: Add Service Account to Project

1. Copy `firebase-service-account.json` to `Wefaaq.Api` project root
2. Update `appsettings.json`:

```json
{
  "Firebase": {
    "ProjectId": "your-actual-firebase-project-id",
    "ServiceAccountPath": "firebase-service-account.json"
  }
}
```

### Step 3: Update .gitignore

Add this to your `.gitignore`:

```
# Firebase service account (sensitive)
firebase-service-account.json
```

---

## ?? API Endpoints

### 1. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6Ij..."
}
```

**Response (Success):**
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "POST Request successful.",
  "isError": false,
  "result": {
    "id": 1,
    "email": "user@example.com",
    "name": "John Doe",
    "firebaseUid": "abc123xyz",
    "role": "User",
    "organizationId": null,
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "lastLoginAt": "2024-01-15T10:30:00Z"
  }
}
```

**Response (Unauthorized):**
```json
{
  "version": "1.0",
  "statusCode": 401,
  "message": "Invalid or expired Firebase token",
  "isError": true
}
```

**Response (Forbidden - User Not Active):**
```json
{
  "version": "1.0",
  "statusCode": 403,
  "message": "You Are Not Authorized",
  "isError": true
}
```

### 2. Logout
```http
POST /api/auth/logout
Content-Type: application/json

{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6Ij..."
}
```

**Response:**
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "POST Request successful.",
  "isError": false,
  "result": {
    "success": true,
    "message": "Logged out successfully"
  }
}
```

### 3. Get Current User (Protected Endpoint Example)
```http
GET /api/auth/me
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6Ij...
```

**Response:**
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "GET Request successful.",
  "isError": false,
  "result": {
    "id": 1,
    "email": "user@example.com",
    "name": "John Doe",
    "firebaseUid": "abc123xyz",
    "role": "User",
    "organizationId": null,
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "lastLoginAt": "2024-01-15T10:30:00Z"
  }
}
```

---

## ?? Protecting Endpoints with Authentication

To protect any endpoint, simply add the `[Authorize]` attribute:

```csharp
[Authorize]
[HttpGet("get-all")]
public async Task<IActionResult> GetAll()
{
    // Only authenticated users can access this
    var clients = await _clientService.GetAllAsync();
    return Ok(clients);
}
```

To get the current user's Firebase UID in a protected endpoint:

```csharp
[Authorize]
[HttpGet("my-data")]
public async Task<IActionResult> GetMyData()
{
    // Get Firebase UID from claims
    string? firebaseUid = User.FindFirst("user_id")?.Value;
    
    // Use it to fetch user-specific data
    var userData = await _someService.GetDataForUser(firebaseUid);
    
    return Ok(userData);
}
```

---

## ?? Testing with Postman/cURL

### 1. Get Firebase ID Token (Frontend)

In your Angular app, after Firebase login:

```typescript
const user = await this.afAuth.currentUser;
const idToken = await user.getIdToken();
console.log('ID Token:', idToken);
```

### 2. Test Login Endpoint

```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"idToken":"YOUR_FIREBASE_ID_TOKEN_HERE"}' \
  -k
```

### 3. Test Protected Endpoint

```bash
curl -X GET "https://localhost:5001/api/auth/me" \
  -H "Authorization: Bearer YOUR_FIREBASE_ID_TOKEN_HERE" \
  -k
```

---

## ?? User Auto-Creation Flow

When a user logs in with a valid Firebase token:

1. **Token Validation**: Backend verifies Firebase ID token
2. **User Lookup**: Checks if user exists by `FirebaseUid`
3. **Auto-Creation**: If user doesn't exist:
   - Creates new User record
   - Sets Email from Firebase token
   - Sets Name from token (or email prefix if not available)
   - Sets default Role as "User"
   - Sets IsActive = true
4. **Login Timestamp**: Updates `LastLoginAt`
5. **Return User Data**: Returns user information to frontend

---

## ??? Security Features Implemented

? Firebase ID token verification (server-side)  
? Token expiration validation (1 hour)  
? User active status check  
? JWT Bearer authentication for protected endpoints  
? HTTPS only (enforced by Firebase)  
? Structured logging of authentication events  
? Unique FirebaseUid and Email constraints  

---

## ?? Next Steps

### 1. Update Frontend Environment

```typescript
// environments/api-url.ts
export class ApiUrl {
    static baseUrl = 'https://your-backend-api.com/api';
    static login = `${ApiUrl.baseUrl}/auth/login`;
    static logout = `${ApiUrl.baseUrl}/auth/logout`;
}
```

### 2. Create Firebase Service Account

- Download service account JSON from Firebase Console
- Place it in `Wefaaq.Api` folder
- Update `appsettings.json` with correct ProjectId

### 3. Run Database Migration

Execute the SQL script provided above in your SQL Server database.

### 4. Test Authentication Flow

1. Frontend: Login with Firebase (get ID token)
2. Call `POST /api/auth/login` with ID token
3. Receive user data
4. Store user data in localStorage
5. Use token for subsequent API calls

### 5. Protect Additional Endpoints (Optional)

Add `[Authorize]` to any endpoints that require authentication:

```csharp
// Example: Protect Client endpoints
[Authorize]
[HttpGet("get-all")]
public async Task<IActionResult> GetAll()
{
    // Only authenticated users can access
}
```

---

## ?? Configuration Checklist

- [ ] NuGet packages installed ?
- [ ] Code files created ?
- [ ] Code files modified ?
- [ ] Firebase service account downloaded
- [ ] `appsettings.json` updated with correct ProjectId
- [ ] Service account file placed in project
- [ ] Database migration applied
- [ ] `.gitignore` updated to exclude service account
- [ ] Frontend environment configured
- [ ] Authentication tested end-to-end

---

## ?? Troubleshooting

### Issue: "Could not load file or assembly 'System.Runtime'"
**Solution**: This is a known issue with EF Core tools. Use the manual SQL migration script provided above instead.

### Issue: "Firebase token validation failed"
**Solution**: 
- Verify ProjectId matches Firebase console
- Ensure service account JSON is valid
- Check token isn't expired (tokens expire after 1 hour)

### Issue: "User not authenticated" in protected endpoints
**Solution**:
- Ensure `Authorization: Bearer {token}` header is sent
- Verify `UseAuthentication()` is called before `UseAuthorization()` in Program.cs

### Issue: "CORS error"
**Solution**: The "AllowAll" CORS policy is already configured. For production, restrict origins:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://your-production-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

---

## ? Summary

Your Firebase Authentication backend is **fully implemented** and ready to use! The implementation includes:

- ? User entity and database schema
- ? User repository with Firebase UID lookups
- ? Authentication service with token validation
- ? Login/Logout API endpoints
- ? JWT Bearer authentication middleware
- ? Auto-wrapper integration for consistent responses
- ? Swagger UI with JWT Bearer support
- ? User auto-creation on first login
- ? Protected endpoint example (`/api/auth/me`)
- ? Comprehensive error handling and logging

**All you need to do now is:**
1. Run the database migration SQL
2. Add your Firebase service account file
3. Update appsettings.json with your ProjectId
4. Test the endpoints!

---

**Implementation Date**: 2025-01-23  
**Status**: ? Complete and Ready for Testing  
**Build Status**: ? Successful  

