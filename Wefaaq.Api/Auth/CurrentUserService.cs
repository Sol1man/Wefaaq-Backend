using System.Security.Claims;
using Wefaaq.Bll.Constants;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Auth;

/// <summary>
/// Reads identity from the current HTTP context. Claims are populated upstream
/// by <see cref="RoleClaimsTransformation"/>.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public int? UserId
    {
        get
        {
            var raw = _accessor.HttpContext?.User?.FindFirst("userId")?.Value;
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public bool IsAdmin =>
        _accessor.HttpContext?.User?.IsInRole(Roles.Admin) == true;
}
