namespace PersonalFinance.Application.Finance.Identity.ListUsers;

public sealed record UserSummary(
    Guid Id,
    string FullName,
    string Username,
    string? Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    DateTime CreatedAt);
