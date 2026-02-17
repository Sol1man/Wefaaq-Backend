using AutoMapper;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Logging;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal.Entities;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Bll.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService(
	IUserRepository userRepository,
	IMapper mapper,
	ILogger<AuthService> logger)
	: IAuthService
{
	/// <summary>
    /// Login with Firebase ID token
    /// </summary>
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            // Verify Firebase ID Token
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(request.IdToken);

            string firebaseUid = decodedToken.Uid;
            string? email = decodedToken.Claims.ContainsKey("email")
                ? decodedToken.Claims["email"].ToString()
                : null;

            if (string.IsNullOrEmpty(email))
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Email not found in Firebase token"
                };
            }

            // Check if user exists in database
            var user = await userRepository.GetByFirebaseUidAsync(firebaseUid);

            if (user == null)
            {
                // Auto-create user if not exists
                user = new User
                {
                    FirebaseUid = firebaseUid,
                    Email = email,
                    Name = decodedToken.Claims.ContainsKey("name")
                        ? decodedToken.Claims["name"].ToString()
                        : email.Split('@')[0],
                    RoleId = 2, // Default to User role (Id = 2)
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await userRepository.AddAsync(user);
                logger.LogInformation("New user created with FirebaseUid: {FirebaseUid}", firebaseUid);
            }
            else if (!user.IsActive)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "You Are Not Authorized"
                };
            }

            // Update last login timestamp
            await userRepository.UpdateLastLoginAsync(user.Id);

            var userDto = mapper.Map<UserDto>(user);

            return new LoginResponseDto
            {
                Success = true,
                User = userDto,
                Message = "Login successful"
            };
        }
        catch (FirebaseAuthException ex)
        {
            logger.LogError(ex, "Firebase authentication error during login. Code: {Code}, Message: {Message}", ex.ErrorCode, ex.Message);
            return new LoginResponseDto
            {
                Success = false,
                Message = $"Firebase auth error: {ex.ErrorCode} - {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login: {Message}", ex.Message);
            return new LoginResponseDto
            {
                Success = false,
                Message = $"Login error: {ex.GetType().Name} - {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Logout user
    /// </summary>
    public async Task<LogoutResponseDto> LogoutAsync(LogoutRequestDto request)
    {
        try
        {
            // Optional: Verify token to log the event
            try
            {
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(request.IdToken);

                string firebaseUid = decodedToken.Uid;
                logger.LogInformation("User logged out: {FirebaseUid}", firebaseUid);
            }
            catch (FirebaseAuthException)
            {
                // Token might be invalid/expired, but still allow logout
                logger.LogWarning("Logout attempted with invalid token");
            }

            return new LogoutResponseDto
            {
                Success = true,
                Message = "Logged out successfully"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            // Still return success for logout
            return new LogoutResponseDto
            {
                Success = true,
                Message = "Logged out"
            };
        }
    }

    /// <summary>
    /// Get user by Firebase UID
    /// </summary>
    public async Task<UserDto?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        var user = await userRepository.GetByFirebaseUidAsync(firebaseUid);
        return user != null ? mapper.Map<UserDto>(user) : null;
    }
}
