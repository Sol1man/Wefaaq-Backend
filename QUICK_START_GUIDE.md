# ?? Quick Start Guide - Firebase Authentication Setup

## ?? 10-Minute Setup

Follow these steps to get Firebase Authentication working in your Wefaaq Backend API.

---

## Step 1: Firebase Console Setup (5 minutes)

### 1.1 Get Your Firebase Project ID
1. Go to [Firebase Console](https://console.firebase.google.com)
2. Select your project (or create one)
3. Note your **Project ID** (you'll need this)

### 1.2 Download Service Account Key
1. In Firebase Console, click ?? (Settings) ? **Project settings**
2. Go to **Service accounts** tab
3. Click **Generate new private key**
4. Click **Generate key** button
5. Save the JSON file as `firebase-service-account.json`

### 1.3 Place Service Account File
1. Copy `firebase-service-account.json` to `D:\MyWork\Wefaaq-Backend\Wefaaq.Api\`
2. The file should be in the same folder as `Program.cs`

---

## Step 2: Update Configuration (1 minute)

### 2.1 Update appsettings.json

Open `Wefaaq.Api/appsettings.json` and update:

```json
{
  "Firebase": {
    "ProjectId": "YOUR_ACTUAL_FIREBASE_PROJECT_ID_HERE",
    "ServiceAccountPath": "firebase-service-account.json"
  }
}
```

Replace `YOUR_ACTUAL_FIREBASE_PROJECT_ID_HERE` with your actual Project ID from Step 1.1

---

## Step 3: Database Migration (2 minutes)

### Option A: Manual SQL (Recommended)

1. Open **SQL Server Management Studio**
2. Connect to your database: `WefaaqDb`
3. Run this SQL script:

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

-- Verify table creation
SELECT * FROM sys.tables WHERE name = 'Users';
```

4. Verify the Users table was created successfully

### Option B: Entity Framework (if EF migrations work)

```bash
cd D:\MyWork\Wefaaq-Backend
dotnet ef migrations add AddUserAuthentication --project Wefaaq.Dal --startup-project Wefaaq.Api
dotnet ef database update --project Wefaaq.Dal --startup-project Wefaaq.Api
```

---

## Step 4: Update .gitignore (30 seconds)

Add this to your `.gitignore` file:

```
# Firebase service account (NEVER commit this!)
firebase-service-account.json
**/firebase-service-account.json
```

---

## Step 5: Test the Implementation (2 minutes)

### 5.1 Build the Project

```bash
cd D:\MyWork\Wefaaq-Backend
dotnet build
```

You should see: **Build succeeded**

### 5.2 Run the API

```bash
cd D:\MyWork\Wefaaq-Backend\Wefaaq.Api
dotnet run
```

### 5.3 Open Swagger UI

Navigate to: `https://localhost:5001` (or the port shown in console)

You should see:
- ? Swagger UI loads successfully
- ? Three new endpoints under "Auth" section:
  - `POST /api/auth/login`
  - `POST /api/auth/logout`
  - `GET /api/auth/me`
- ? ?? Lock icon (indicating JWT Bearer support)

---

## Step 6: Test Login with Frontend (Optional)

If you have the Angular frontend running:

1. Navigate to the login page
2. Sign in with a valid Firebase user
3. Check browser console for:
   - Firebase ID token
   - Backend API call to `/api/auth/login`
   - User data stored in localStorage

---

## ? Verification Checklist

- [ ] Firebase Project ID obtained
- [ ] Service account JSON downloaded
- [ ] `firebase-service-account.json` placed in `Wefaaq.Api` folder
- [ ] `appsettings.json` updated with correct Project ID
- [ ] Database migration executed successfully
- [ ] Users table exists in database
- [ ] `.gitignore` updated
- [ ] Project builds successfully
- [ ] Swagger UI shows Auth endpoints
- [ ] Ready for frontend integration!

---

## ?? Quick Test (Without Frontend)

You can test the login endpoint using the Firebase token from your Angular app:

### 1. Get a Firebase Token

In your Angular app console:
```javascript
firebase.auth().currentUser.getIdToken().then(token => console.log(token));
```

### 2. Test with cURL

```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"idToken":"PASTE_YOUR_FIREBASE_TOKEN_HERE"}' \
  -k
```

### 3. Expected Response

```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "POST Request successful.",
  "isError": false,
  "result": {
    "id": 1,
    "email": "user@example.com",
    "name": "User Name",
    "firebaseUid": "...",
    "role": "User",
    "isActive": true,
    "createdAt": "2024-01-23T...",
    "lastLoginAt": "2024-01-23T..."
  }
}
```

---

## ?? Troubleshooting

### Issue: "FirebaseAuth.DefaultInstance.get returned null"
**Solution**: 
- Firebase service account file is missing
- Download from Firebase Console ? Project Settings ? Service Accounts
- Save as `firebase-service-account.json` in `Wefaaq.Api` folder
- Restart the application
- You should see "? Firebase initialized successfully" in console

### Issue: "Firebase service account file not found"
**Solution**:
- The file path in appsettings.json doesn't match the actual file location
- Make sure `firebase-service-account.json` is in the API project root folder
- Update the path in appsettings.json if needed

### Issue: Build fails
**Solution**: Make sure all NuGet packages are restored:
```bash
dotnet restore
```

### Issue: "Could not load Firebase"
**Solution**: 
- Check `firebase-service-account.json` is in correct location
- Verify ProjectId in `appsettings.json` matches Firebase Console
- Check JSON file is valid (not corrupted)

### Issue: "Users table not found"
**Solution**: Run the SQL migration script again

### Issue: Swagger doesn't show Auth endpoints
**Solution**: 
- Rebuild the project
- Clear browser cache
- Check `AuthController.cs` exists in Controllers folder

---

## ?? Next Steps

After setup is complete:

1. **Read**: `FRONTEND_INTEGRATION_GUIDE.md` for Angular integration
2. **Test**: End-to-end authentication flow
3. **Secure**: Add `[Authorize]` attribute to protect your endpoints
4. **Customize**: Add custom roles and permissions as needed

---

## ?? You're Done!

Your Firebase Authentication backend is now ready to use!

**Total Setup Time**: ~10 minutes  
**Status**: ? Ready for Integration  

---

**Need Help?**
- Check `FIREBASE_AUTH_IMPLEMENTATION.md` for detailed documentation
- Check `FRONTEND_INTEGRATION_GUIDE.md` for frontend integration
- Review error logs in console for specific issues

**Happy Coding! ??**

