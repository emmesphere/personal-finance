using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Application.Finance.Reports.GetAdminSummary;

public sealed class GetAdminSummaryHandler(
    IUserRepository userRepository,
    ILedgerRepository ledgerRepository,
    IJournalEntryRepository journalEntryRepository,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<Result<GetAdminSummaryResponse>> HandleAsync(GetAdminSummaryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = dateTimeProvider.UtcNow;

        var totalUsers = await userRepository.CountAllAsync(ct);
        var totalLedgers = await ledgerRepository.CountAllAsync(ct);
        var postedThisMonth = await journalEntryRepository.CountPostedInMonthAsync(now.Year, now.Month, ct);

        return Result.Success(new GetAdminSummaryResponse(totalUsers, totalLedgers, postedThisMonth));
    }
}
