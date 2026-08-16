using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Categories;

namespace PersonalFinance.Application.Finance.Common;

public sealed class CategoryBackingAccountProvisioner(IAccountRepository accountRepository)
{
    public async Task<Result<Account>> GetOrCreateAsync(Guid ledgerId, Category category, DateTime createdAt, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(category);

        var existing = await accountRepository.GetCategoryBackingAccountAsync(ledgerId, category.Id, ct);
        if (existing is not null)
            return Result.Success(existing);

        var accountType = category.Kind == CategoryKind.Income ? AccountType.Income : AccountType.Expense;

        var accountResult = Account.Create(ledgerId, category.Name, accountType, dueDate: null, category.Id, createdAt);
        if (accountResult.IsFailure)
            return accountResult;

        accountRepository.Add(accountResult.Value);
        return accountResult;
    }
}
