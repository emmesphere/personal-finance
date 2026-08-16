using System.IdentityModel.Tokens.Jwt;

using PersonalFinance.Application.Abstractions.Security;

namespace PersonalFinance.WebApi.Security;

#pragma warning disable CA1515 // Must be public: Wolverine's handler codegen constructs it across the assembly boundary.
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
#pragma warning restore CA1515
{
    public Guid UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;
}
