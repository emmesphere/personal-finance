using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Application.Finance.Identity.GetMe;

public sealed class GetMeHandler(
    IUserRepository userRepository,
    ILedgerRepository ledgerRepository,
    ICurrentUserService currentUserService)
{
    public async Task<Result<MeResponse>> HandleAsync(GetMeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var user = await userRepository.GetByIdAsync(currentUserService.UserId, ct);
        if (user is null)
            return Result.Failure<MeResponse>(ResultError.NotFound("User.NotFound", "User not found."));

        var ledgers = await ledgerRepository.ListByMemberUserIdAsync(currentUserService.UserId, ct);

        IReadOnlyCollection<LedgerMembership> memberships = ledgers
            .Select(l => new LedgerMembership(l.Id, l.Name, l.OwnerUserId.Value == currentUserService.UserId))
            .ToArray();

        return Result.Success(new MeResponse(
            user.Id,
            user.FullName,
            user.Username,
            user.Email,
            user.PhoneNumber,
            user.Role.ToString(),
            user.IsActive,
            user.CreatedAt,
            memberships));
    }
}
