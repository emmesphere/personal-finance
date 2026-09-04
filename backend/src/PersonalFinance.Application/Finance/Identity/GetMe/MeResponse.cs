namespace PersonalFinance.Application.Finance.Identity.GetMe;

public sealed record LedgerMembership(Guid LedgerId, string LedgerName, bool IsOwner);

public sealed record MeResponse(
    Guid Id,
    string FullName,
    string Username,
    string? Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyCollection<LedgerMembership> Ledgers);
