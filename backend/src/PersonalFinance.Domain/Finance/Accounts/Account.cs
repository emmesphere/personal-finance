using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Domain.Finance.Accounts;

public sealed class Account : AggregateRoot
{
    public Guid LedgerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public DueDate? DueDate { get; private set; }
    public Guid? CategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Account() { }

    private Account(Guid ledgerId, string name, AccountType type, DueDate? dueDate, Guid? categoryId, DateTime createdAt)
    {
        LedgerId = ledgerId;
        Name = name;
        Type = type;
        DueDate = dueDate;
        CategoryId = categoryId;
        CreatedAt = createdAt;
        IsActive = true;
    }

    private static bool AllowsDueDate(AccountType type) => type is AccountType.CreditCard or AccountType.Debit or AccountType.Loan;

    private static bool IsCategoryBacking(AccountType type) => type is AccountType.Income or AccountType.Expense;

    public static Result<Account> Create(
        Guid ledgerId,
        string name,
        AccountType type,
        DueDate? dueDate,
        Guid? categoryId,
        DateTime createdAt)
    {
        if (ledgerId == Guid.Empty)
            return Result.Failure<Account>(ResultError.Validation("Account.Ledger.Empty", "LedgerId is required."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Account>(ResultError.Validation("Account.Name.Empty", "Account name is required."));

        if (dueDate is not null && !AllowsDueDate(type))
            return Result.Failure<Account>(ResultError.Validation("Account.DueDate.NotAllowed", "Only Credit Card and Debit accounts can have a DueDate."));

        if (IsCategoryBacking(type) && categoryId is null)
            return Result.Failure<Account>(ResultError.Validation("Account.CategoryId.Required", "Income and Expense accounts must reference a Category."));

        if (!IsCategoryBacking(type) && categoryId is not null)
            return Result.Failure<Account>(ResultError.Validation("Account.CategoryId.NotAllowed", "Only Income and Expense accounts can reference a Category."));

        return Result.Success(new Account(ledgerId, name.Trim(), type, dueDate, categoryId, createdAt));
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(ResultError.Validation("Account.Name.Empty", "Account name is required."));

        Name = name.Trim();
        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }
}
