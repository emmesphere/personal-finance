using PersonalFinance.Application.Abstractions.Security;

namespace PersonalFinance.Application.Tests.Fakes;

public sealed class FakeCurrentUserService(Guid userId, bool isAdmin = false) : ICurrentUserService
{
    public Guid UserId { get; } = userId;

    public bool IsAuthenticated => true;

    public bool IsAdmin { get; } = isAdmin;
}
