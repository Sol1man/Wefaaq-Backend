using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Login with Firebase ID token
    /// </summary>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Logout user
    /// </summary>
    Task<LogoutResponseDto> LogoutAsync(LogoutRequestDto request);

    /// <summary>
    /// Get user by Firebase UID
    /// </summary>
    Task<UserDto?> GetUserByFirebaseUidAsync(string firebaseUid);
}
