using PersonalFinance.Application.Abstractions.Security;

namespace PersonalFinance.Application.Tests.Fakes;

public sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed:{password}";

    public bool Verify(string passwordHash, string providedPassword) => passwordHash == $"hashed:{providedPassword}";
}
