using Microsoft.AspNetCore.Identity;

using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.Domain.Identity.Users;

namespace PersonalFinance.Infrastructure.Security;

public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(user: null!, password);

    public bool Verify(string passwordHash, string providedPassword)
        => _hasher.VerifyHashedPassword(user: null!, passwordHash, providedPassword) != PasswordVerificationResult.Failed;
}
