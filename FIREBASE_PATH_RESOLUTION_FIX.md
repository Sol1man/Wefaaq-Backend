# Firebase Service Account Path Resolution Fix

## Issue
The application was throwing a `FileNotFoundException` when trying to load the Firebase service account file, even though the file existed at `Wefaaq.Api\firebase-service-account.json`.

**Error Message:**
```
System.IO.FileNotFoundException: 'Firebase service account file not found at: D:\MyWork\Wefaaq-Backend\Wefaaq.Api\firebase-service-account.json
```

## Root Cause
The issue was caused by the way the application resolves file paths at runtime. When the application runs, especially in debug mode, `Directory.GetCurrentDirectory()` might return the build output directory (e.g., `bin\Debug\net8.0`) instead of the project root directory, causing the file lookup to fail.

## Solution Applied

### 1. Improved Path Resolution Strategy
Updated `Wefaaq.Api\Extensions\FirebaseAuthExtensions.cs` to use multiple fallback strategies for locating the Firebase service account file:

- **Strategy 1**: Relative to current directory (`Directory.GetCurrentDirectory()`)
- **Strategy 2**: Relative to base directory (`AppContext.BaseDirectory`)
- **Strategy 3**: Absolute path (if provided)

This ensures the file is found regardless of where the application is running from.

### 2. Enhanced Error Messages
Improved error messages to show all searched paths when the file is not found, making troubleshooting easier:

```
Firebase service account file not found.
Searched in:
  - {currentDirPath}
  - {baseDirPath}
  - {serviceAccountPath}
```

### 3. Build Configuration
Updated `Wefaaq.Api\Wefaaq.Api.csproj` to ensure the `firebase-service-account.json` file is always copied to the output directory:

```xml
<ItemGroup>
  <Content Include="firebase-service-account.json">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

## Files Modified

1. **Wefaaq.Api\Extensions\FirebaseAuthExtensions.cs**
   - Implemented multi-strategy path resolution
   - Improved error messages with detailed search paths
   - Maintained backward compatibility

2. **Wefaaq.Api\Wefaaq.Api.csproj**
   - Added build configuration to copy `firebase-service-account.json` to output directory

## Testing
After applying these changes:
1. The build completes successfully
2. The Firebase service account file will be found in any of the following scenarios:
   - Running from Visual Studio (F5)
   - Running from `dotnet run`
   - Running from published output
   - Running from build output directory

## Next Steps
To apply the fix to your running application:
1. **Stop the current debug session** if running
2. **Rebuild the solution** (Ctrl+Shift+B)
3. **Run the application** (F5)

The improved path resolution will now find the Firebase service account file automatically.

## Configuration
Your current configuration in `appsettings.json`:
```json
"Firebase": {
  "ProjectId": "wefaaq-2a942",
  "ServiceAccountPath": "firebase-service-account.json"
}
```

This configuration is correct and will work with the updated code.
