namespace PersonalFinance.Application.Abstractions.Security;

public interface ICurrentUserService
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }
}
