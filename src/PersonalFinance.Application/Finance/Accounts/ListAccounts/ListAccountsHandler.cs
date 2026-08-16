using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Finance.Accounts.ListAccounts;

public sealed class ListAccountsHandler(
    ILedgerRepository ledgerRepository,
    IAccountRepository accountRepository,
    ICurrentUserService currentUserService)
{
    public async Task<Result<IReadOnlyCollection<AccountSummary>>> HandleAsync(ListAccountsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ledger = await ledgerRepository.GetByIdAsync(query.LedgerId, ct);
        if (ledger is null)
            return Result.Failure<IReadOnlyCollection<AccountSummary>>(
                ResultError.NotFound("Ledger.NotFound", "Ledger not found."));

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure<IReadOnlyCollection<AccountSummary>>(userIdResult.Error);

        var memberCheck = ledger.EnsureMember(userIdResult.Value);
        if (memberCheck.IsFailure)
            return Result.Failure<IReadOnlyCollection<AccountSummary>>(memberCheck.Error);

        var accounts = await accountRepository.ListByLedgerAsync(query.LedgerId, ct);

        IReadOnlyCollection<AccountSummary> summaries = accounts
            .Where(a => a.Type is not (AccountType.Income or AccountType.Expense))
            .Select(a => new AccountSummary(a.Id, a.Name, a.Type, a.DueDate?.Day, a.IsActive))
            .ToArray();

        return Result.Success(summaries);
    }
}
