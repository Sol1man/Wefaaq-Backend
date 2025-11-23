using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Wefaaq.Api.Extensions;

/// <summary>
/// Firebase authentication configuration extensions
/// </summary>
public static class FirebaseAuthExtensions
{
    /// <summary>
    /// Configure Firebase Admin SDK and JWT Bearer Authentication
    /// </summary>
    public static IServiceCollection AddFirebaseAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get Firebase configuration
        var projectId = configuration["Firebase:ProjectId"];
        var serviceAccountPath = configuration["Firebase:ServiceAccountPath"];

        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException(
                "Firebase ProjectId is not configured in appsettings.json. " +
                "Please add Firebase:ProjectId to your configuration.");
        }

        // Initialize Firebase Admin SDK only if not already initialized
        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                // Try to initialize with service account file
                if (!string.IsNullOrEmpty(serviceAccountPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), serviceAccountPath);
                    
                    if (File.Exists(fullPath))
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(fullPath),
                            ProjectId = projectId
                        });
                        Console.WriteLine($"? Firebase initialized successfully with service account: {serviceAccountPath}");
                    }
                    else
                    {
                        // Service account path configured but file doesn't exist
                        throw new FileNotFoundException(
                            $"Firebase service account file not found at: {fullPath}\n" +
                            $"Please download your service account JSON from Firebase Console and place it at: {fullPath}\n" +
                            $"Or update Firebase:ServiceAccountPath in appsettings.json");
                    }
                }
                else
                {
                    // No service account path - try default credentials
                    try
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.GetApplicationDefault(),
                            ProjectId = projectId
                        });
                        Console.WriteLine("? Firebase initialized successfully with Application Default Credentials");
                    }
                    catch
                    {
                        throw new InvalidOperationException(
                            "Firebase could not be initialized. Please provide a service account file.\n" +
                            "Download your service account JSON from Firebase Console:\n" +
                            "1. Go to Firebase Console ? Project Settings ? Service Accounts\n" +
                            "2. Click 'Generate new private key'\n" +
                            "3. Save the file as 'firebase-service-account.json' in your API project root\n" +
                            "4. Update Firebase:ServiceAccountPath in appsettings.json");
                    }
                }
            }
            catch (Exception ex) when (ex is not FileNotFoundException && ex is not InvalidOperationException)
            {
                throw new InvalidOperationException(
                    $"Failed to initialize Firebase Admin SDK: {ex.Message}\n" +
                    $"Please check your Firebase configuration in appsettings.json", ex);
            }
        }
        else
        {
            Console.WriteLine("?? Firebase already initialized, skipping initialization");
        }

        // Configure JWT Bearer Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://securetoken.google.com/{projectId}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{projectId}",
                    ValidateAudience = true,
                    ValidAudience = projectId,
                    ValidateLifetime = true
                };
            });

        services.AddAuthorization();

        return services;
    }
}
