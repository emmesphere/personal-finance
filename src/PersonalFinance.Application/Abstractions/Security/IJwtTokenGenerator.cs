using PersonalFinance.Domain.Identity.Users;

namespace PersonalFinance.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
