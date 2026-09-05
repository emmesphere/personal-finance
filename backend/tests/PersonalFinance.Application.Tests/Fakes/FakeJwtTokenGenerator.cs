using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.Domain.Identity.Users;

namespace PersonalFinance.Application.Tests.Fakes;

public sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public string GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return $"token-for-{user.Username}";
    }
}
