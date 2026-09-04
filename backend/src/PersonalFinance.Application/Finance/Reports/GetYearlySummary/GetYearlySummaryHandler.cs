using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Finance.Reports.GetYearlySummary;

public sealed class GetYearlySummaryHandler(
    ILedgerRepository ledgerRepository,
    IFinanceReportQueries reportQueries,
    ICurrentUserService currentUserService)
{
    public async Task<Result<GetYearlySummaryResponse>> HandleAsync(GetYearlySummaryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ledger = await ledgerRepository.GetByIdAsync(query.LedgerId, ct);
        if (ledger is null)
            return Result.Failure<GetYearlySummaryResponse>(ResultError.NotFound("Ledger.NotFound", "Ledger not found."));

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure<GetYearlySummaryResponse>(userIdResult.Error);

        var memberCheck = ledger.EnsureMember(userIdResult.Value);
        if (memberCheck.IsFailure)
            return Result.Failure<GetYearlySummaryResponse>(memberCheck.Error);

        var months = await reportQueries.GetYearlyExpenseTotalsAsync(query.LedgerId, query.Year, ct);

        return Result.Success(new GetYearlySummaryResponse(query.Year, months));
    }
}
