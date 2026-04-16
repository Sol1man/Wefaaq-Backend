# Railway Deployment Guide for Wefaaq Backend

This guide will walk you through deploying your Wefaaq .NET 8.0 Web API to Railway.app step-by-step.

---

## PART 1: PREPARE YOUR AZURE SQL DATABASE (15 minutes)

### Step 1: Get Your Azure SQL Connection String

1. Go to **Azure Portal**: https://portal.azure.com
2. Sign in with your Azure account
3. In the search bar at the top, type **"SQL databases"**
4. Click on **"SQL databases"** from the results
5. Find and click on your database (e.g., **"WefaaqDb"**)
6. In the left menu, click **"Connection strings"**
7. You'll see two connection string options:
   - ✅ **ADO.NET (SQL authentication)** ← Use this one
   - ❌ **ADO.NET (Microsoft Entra passwordless authentication)** ← Don't use this
8. Click on **"ADO.NET (SQL authentication)"**
9. Copy the entire connection string (it should look like this):

```
Server=tcp:your-server.database.windows.net,1433;Initial Catalog=WefaaqDb;Persist Security Info=False;User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

10. **IMPORTANT:** Replace `{your_username}` and `{your_password}` with your actual SQL Server admin credentials

**Don't know your password?** See Step 2 below.

---

### Step 2: Reset Azure SQL Admin Password (if needed)

**If you don't remember your Azure SQL Server admin password:**

1. In Azure Portal, search for **"SQL servers"** (not "SQL databases")
2. Click on your **SQL Server** (e.g., "wefaaq-server")
3. In the Overview page, look for **"Server admin"** - this shows your admin username (e.g., `sqladmin`)
4. In the left menu, scroll down and click **"Reset password"**
5. Enter a new password (must be strong: uppercase, lowercase, numbers, special characters)
6. **SAVE THIS PASSWORD SOMEWHERE SAFE** (you'll need it in a moment)
7. Click **"Save"**

**Now update your connection string:**
- Replace `{your_username}` with your admin username (e.g., `sqladmin`)
- Replace `{your_password}` with the password you just set

**Example connection string (with real values):**
```
Server=tcp:wefaaq-server.database.windows.net,1433;Initial Catalog=WefaaqDb;User ID=sqladmin;Password=MyStr0ngP@ssw0rd;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

---

### Step 3: Configure Azure SQL Firewall (CRITICAL!)

**Railway needs access to your Azure SQL database. Without this, your app will fail to connect.**

1. Still in Azure Portal, go to your **SQL Server** (not database)
2. In the left menu, click **"Networking"** or **"Firewalls and virtual networks"**
3. You'll see a section called **"Firewall rules"**
4. Click **"+ Add a firewall rule"** or **"Add client IP"**
5. Create a new rule:
   - **Rule name:** `AllowRailway`
   - **Start IP address:** `0.0.0.0`
   - **End IP address:** `255.255.255.255`
6. Also check the box: **"Allow Azure services and resources to access this server"** (set to YES)
7. Click **"Save"** at the top

**Why?** This allows Railway (which runs outside Azure) to connect to your database.

---

### Step 4: Test Your Connection String Locally (Optional but Recommended)

Before deploying, test that your connection string works:

**Windows PowerShell:**
```powershell
$env:CONNECTION_STRING="Server=tcp:your-server.database.windows.net,1433;Initial Catalog=WefaaqDb;User ID=sqladmin;Password=YourPassword;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

dotnet run --project Wefaaq.Api
```

If it runs without database errors, your connection string is correct!

Press `Ctrl+C` to stop.

---

## PART 2: PUSH YOUR CODE TO GITHUB (10 minutes)

### Step 5: Check if Your Code is in Git

Open **PowerShell** or **Command Prompt** and navigate to your project:

```powershell
cd "D:\Wefaaq Project\Wefaaq Backend"
```

Check Git status:
```powershell
git status
```

**If you see:** `"fatal: not a git repository"`
- Your code is NOT in Git yet → Continue to Step 6

**If you see:** A list of files and branch info
- Your code IS in Git already → Skip to Step 7

---

### Step 6: Initialize Git (if needed)

```powershell
# Initialize Git
git init

# Add all files
git add .

# Create first commit
git commit -m "Prepare Wefaaq backend for Railway deployment"
```

---

### Step 7: Create GitHub Repository

1. Go to: https://github.com/new
2. Fill in the form:
   - **Repository name:** `Wefaaq-Backend`
   - **Description:** "Wefaaq .NET 8.0 Web API"
   - **Visibility:** Choose **Private** (recommended for production code)
   - **DO NOT check:** "Add a README file"
   - **DO NOT check:** "Add .gitignore"
   - **DO NOT check:** "Choose a license"
3. Click **"Create repository"**

GitHub will show you instructions. You'll see commands like:

```bash
git remote add origin https://github.com/YOUR-USERNAME/Wefaaq-Backend.git
git branch -M main
git push -u origin main
```

---

### Step 8: Push Code to GitHub

Copy the commands from GitHub (or use these, replacing YOUR-USERNAME):

```powershell
# Add GitHub as remote
git remote add origin https://github.com/YOUR-USERNAME/Wefaaq-Backend.git

# Rename branch to main (if needed)
git branch -M main

# Push code to GitHub
git push -u origin main
```

**You may be asked to sign in to GitHub** - follow the prompts.

After pushing, refresh your GitHub repository page - you should see your code!

---

## PART 3: DEPLOY TO RAILWAY (15 minutes)

### Step 9: Sign Up for Railway

1. Go to: https://railway.app
2. Click **"Login"** (top right)
3. Click **"Login with GitHub"**
4. **Authorize Railway** to access your GitHub account
5. Complete any email verification if prompted

---

### Step 10: Create New Railway Project

1. In Railway Dashboard, click **"+ New Project"** (or "New" button)
2. Select **"Deploy from GitHub repo"**
3. You'll see a list of your GitHub repositories
4. Find **"Wefaaq-Backend"** and click on it
5. Railway will start deploying automatically

**What happens now:**
- Railway detects it's a .NET project
- Starts building your application
- **The build will likely FAIL** - this is expected! (No environment variables set yet)

Don't worry, we'll fix this in the next steps.

---

### Step 11: Add Environment Variables

This is the **MOST IMPORTANT STEP**. Your connection string goes here!

1. In Railway, click on your service/project (you should see "Wefaaq-Backend" or similar)
2. Look for tabs at the top: **Deployments**, **Variables**, **Settings**, **Metrics**, **Logs**
3. Click the **"Variables"** tab
4. Click **"+ New Variable"** button

---

#### Variable 1: CONNECTION_STRING

**Variable Name:** (type exactly as shown)
```
CONNECTION_STRING
```

**Value:** (paste your Azure SQL connection string from Step 1/2)
```
Server=tcp:wefaaq-server.database.windows.net,1433;Initial Catalog=WefaaqDb;User ID=sqladmin;Password=YourPassword;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Click **"Add"**

**Important:**
- Make sure there are NO extra spaces before or after the connection string
- Make sure you replaced `{your_username}` and `{your_password}` with real values
- After adding, it will show as `•••••••••` (masked) for security

---

#### Variable 2: ASPNETCORE_ENVIRONMENT

Click **"+ New Variable"** again

**Variable Name:**
```
ASPNETCORE_ENVIRONMENT
```

**Value:**
```
Production
```

Click **"Add"**

---

### Step 12: Redeploy with Environment Variables

Now that environment variables are set, redeploy:

1. Click the **"Deployments"** tab
2. Find the latest deployment (probably shows "Failed" or "Crashed")
3. Click the **three dots (⋮)** next to it
4. Click **"Redeploy"**

Railway will rebuild and redeploy your application.

---

### Step 13: Monitor the Deployment

1. Click on the deployment that's in progress (it should say "Building" or "Deploying")
2. Click the **"Build Logs"** tab to watch the build process
3. Look for:
   - ✅ "Build successful"
   - ✅ "Deployment successful"
   - ✅ Messages about the app starting

**If you see errors:**
- Check that your CONNECTION_STRING is correct
- Check that Azure SQL firewall is configured (Step 3)
- Read the error message carefully

---

### Step 14: Get Your Railway URL

1. Go to the **"Settings"** tab of your service
2. Scroll down to the **"Domains"** section
3. Click **"Generate Domain"**
4. Railway will create a URL like: `https://wefaaq-backend-production-xxxx.up.railway.app`
5. **Copy this URL** - this is your API's public address!

---

### Step 15: Test Your Deployed API

Open your browser and test these URLs (replace with your Railway URL):

#### Test 1: Check if API is running
```
https://your-railway-url.up.railway.app/api/clients
```

You should see either:
- ✅ A JSON response (possibly empty array `[]` or authentication error)
- ❌ If you see 404 or error, continue troubleshooting

#### Test 2: Swagger UI (optional)
```
https://your-railway-url.up.railway.app/swagger
```

**Note:** Swagger is disabled in production by default. If you want to enable it, let me know.

---

## PART 4: DATABASE MIGRATIONS (10 minutes)

Your code is deployed, but the database might not have the latest tables and structure.

### Step 16: Choose Migration Strategy

You have **two options**:

---

#### **Option A: Run Migrations Manually (RECOMMENDED)**

**Run migrations from your local machine to Azure SQL:**

1. Open PowerShell on your local machine
2. Navigate to your project:
   ```powershell
   cd "D:\Wefaaq Project\Wefaaq Backend"
   ```

3. Set the connection string environment variable (use your Azure SQL connection string):
   ```powershell
   $env:ConnectionStrings__DefaultConnection="Server=tcp:wefaaq-server.database.windows.net,1433;Initial Catalog=WefaaqDb;User ID=sqladmin;Password=YourPassword;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;"
   ```

4. Run migrations:
   ```powershell
   dotnet ef database update --project Wefaaq.Dal --startup-project Wefaaq.Api
   ```

5. You should see:
   - ✅ "Applying migration '20240101_MigrationName'"
   - ✅ "Done"

**This creates all tables and schema in your Azure SQL database.**

---

#### **Option B: Enable Auto-Migrations on Railway**

**I can modify your code to run migrations automatically when Railway starts.**

**Pros:**
- Automatic, no manual steps
- Migrations run on every deployment

**Cons:**
- Slightly slower startup time
- Riskier for large databases

**Do you want me to enable this?** Let me know and I'll update the code.

---

### Step 17: Verify Database Migration

Check that migrations worked:

1. Go to Azure Portal → Your SQL Database
2. Click **"Query editor"** (left menu)
3. Sign in with your SQL admin credentials
4. Run this query:
   ```sql
   SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'
   ```

You should see tables like:
- `Clients`
- `Organizations`
- `Users`
- `OrganizationLicenses`
- `OrganizationWorkers`
- `OrganizationCars`
- `__EFMigrationsHistory`

---

### Step 18: Seed Data (Optional)

**Your code only seeds data in Development mode.** To seed data in production:

**Option 1: Manually insert test data** using Azure Query Editor

**Option 2: I can enable seeding in production** (let me know if you want this)

**Option 3: Create users through your API** once it's running

---

## PART 5: FINAL CONFIGURATION (10 minutes)

### Step 19: Configure Firebase (if using Firebase Authentication)

Your app uses Firebase for authentication. Railway needs access to your Firebase credentials.

#### Check Your Firebase File

1. On your local machine, check if this file exists:
   ```
   D:\Wefaaq Project\Wefaaq Backend\Wefaaq.Api\GoogleFiles\firebase-adminsdk.json
   ```

2. Open it and check it's a valid JSON file

#### Add Firebase to Railway

**Option A: Environment Variable (More Secure)**

1. Open `firebase-adminsdk.json` in a text editor
2. Copy the ENTIRE contents
3. In Railway → Variables tab
4. Add new variable:
   - **Name:** `FIREBASE_SERVICE_ACCOUNT_JSON`
   - **Value:** Paste the entire JSON content
5. **Note:** You'll need to update your code to read from this variable (I can help with this)

**Option B: Include in Git Repository**

1. Make sure `GoogleFiles/firebase-adminsdk.json` is in your repository
2. Push to GitHub:
   ```powershell
   git add .
   git commit -m "Add Firebase service account"
   git push
   ```
3. Railway will automatically redeploy

**Which option do you prefer?** Let me know and I'll help you set it up.

---

### Step 20: Final Testing Checklist

Test your deployed API:

- [ ] API responds at your Railway URL
- [ ] Database connection works (no connection errors)
- [ ] Can retrieve clients: `GET /api/clients`
- [ ] Can retrieve organizations: `GET /api/organizations`
- [ ] Firebase authentication works (if applicable)
- [ ] CORS works with your frontend

---

## PART 6: TROUBLESHOOTING

### Problem: "Cannot connect to database"

**Solutions:**
1. Check CONNECTION_STRING variable is set in Railway
2. Verify Azure SQL firewall allows `0.0.0.0 - 255.255.255.255`
3. Check connection string has correct username/password
4. Test connection string locally first (Step 4)

---

### Problem: "Application failed to start"

**Solutions:**
1. Check Railway deployment logs: Deployments tab → Click deployment → View logs
2. Look for specific error messages
3. Verify all environment variables are set
4. Check that your code builds locally: `dotnet build`

---

### Problem: "404 Not Found on all endpoints"

**Solutions:**
1. Make sure you're using the correct URL format:
   - ✅ `https://your-url.railway.app/api/clients`
   - ❌ `https://your-url.railway.app/clients`
2. Check that controllers are in the `Wefaaq.Api` project
3. Verify routes in your controllers

---

### Problem: "Firebase authentication fails"

**Solutions:**
1. Verify Firebase service account JSON is accessible
2. Check that `FIREBASE_SERVICE_ACCOUNT_JSON` variable is set
3. Verify Firebase project ID matches
4. Check Railway logs for Firebase-specific errors

---

### Problem: "Migration fails" or "Table already exists"

**Solutions:**
1. Check Azure SQL Query Editor to see which tables exist
2. If tables exist but migrations table is missing, you may need to reset
3. Run: `dotnet ef migrations list` to see applied migrations
4. Consider running: `dotnet ef database update 0` then re-apply

---

## PART 7: UPDATING YOUR DEPLOYMENT

### How to Deploy Code Changes

Every time you make changes to your code:

```powershell
# 1. Save your changes in Visual Studio/VS Code

# 2. Commit changes
git add .
git commit -m "Describe what you changed"

# 3. Push to GitHub
git push

# 4. Railway automatically detects and redeploys!
```

Railway will automatically:
- Detect the GitHub push
- Rebuild your application
- Deploy the new version
- Switch traffic to the new version

---

### How to Change Environment Variables

1. Go to Railway → Your service → Variables tab
2. Click the variable you want to change
3. Edit the value
4. Click outside to save
5. Go to Deployments tab → Click "Redeploy"

---

## PART 8: IMPORTANT NOTES

### Security Reminders

- ⚠️ **NEVER commit connection strings to GitHub**
- ⚠️ **NEVER commit `appsettings.Production.json` with real credentials**
- ✅ **ALWAYS use environment variables for secrets**
- ✅ **Keep your repository private** if it contains any sensitive code

### Cost Information

**Railway Pricing:**
- **Hobby Plan:** $5 credit/month (free to start)
  - Apps sleep after inactivity
  - Good for testing
- **Pro Plan:** $20/month
  - No sleep
  - Better for production

Monitor usage: Railway Dashboard → Usage tab

**Azure SQL Pricing:**
- Check your Azure subscription for SQL costs
- Consider Basic tier for development ($5-15/month)
- Scale up for production

---

## PART 9: NEXT STEPS AFTER DEPLOYMENT

### Recommended Actions

1. **Set up database backups** in Azure Portal
2. **Enable Application Insights** for monitoring
3. **Create a staging environment** (another Railway project)
4. **Add health check endpoint** to your API
5. **Configure custom domain** (if you have one)
6. **Set up automated testing** before deployment
7. **Document your API** with Swagger/OpenAPI
8. **Update your frontend** to use the Railway URL

---

## PART 10: ADMIN USER SETUP

### Your Application Admin User

Based on your `DataSeeder.cs`, your admin user is:

**Email:** `admin@wefaaq.com`
**Firebase UID:** `3dMt71HRn7gBLiTYpHgy9UtKyWX2`
**Role:** Administrator

### Setting Up the Admin Password

Since you use Firebase Authentication:

1. Go to: https://console.firebase.google.com
2. Select project: **wefaaq-2a942**
3. Click **"Authentication"** in left menu
4. Click **"Users"** tab
5. Click **"Add user"**
6. Add user:
   - Email: `admin@wefaaq.com`
   - Password: Choose a secure password
7. After creating, copy the **User UID**
8. Make sure it matches the UID in your `DataSeeder.cs`: `3dMt71HRn7gBLiTYpHgy9UtKyWX2`

**Note:** If UIDs don't match, you'll need to update either Firebase or your code.

---

## SUPPORT

### If You Get Stuck

1. **Check Railway Logs:**
   - Railway Dashboard → Your Service → Logs tab
   - Look for error messages

2. **Check Azure Portal:**
   - SQL Database → Monitoring → Failures
   - Check firewall rules

3. **Test Locally First:**
   - Always test changes locally before deploying

4. **Ask for Help:**
   - Provide specific error messages
   - Share Railway deployment logs
   - Describe what you were trying to do

---

## QUICK REFERENCE

### Railway URL Structure
```
https://your-service-name-production-xxxx.up.railway.app
```

### Common API Endpoints
```
GET  https://your-url/api/clients
GET  https://your-url/api/organizations
POST https://your-url/api/auth/login
```

### Important Environment Variables
```
CONNECTION_STRING=<Azure SQL connection string>
ASPNETCORE_ENVIRONMENT=Production
FIREBASE_SERVICE_ACCOUNT_JSON=<Firebase JSON>
```

### Quick Deploy Commands
```powershell
git add .
git commit -m "Your message"
git push
```

---

**You're ready to deploy! Start with PART 1 and work through each section step by step.**

**Good luck! 🚀**
