using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Domain.Finance.Accounts;

public sealed class Account : AggregateRoot
{
    public Guid LedgerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public DueDate? DueDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Account() { }

    private Account(Guid ledgerId, string name, AccountType type, DueDate? dueDate, DateTime createdAt)
    {
        LedgerId = ledgerId;
        Name = name;
        Type = type;
        DueDate = dueDate;
        CreatedAt = createdAt;
        IsActive = true;
    }

    public static Result<Account> Create(
        Guid ledgerId,
        string name,
        AccountType type,
        DueDate? dueDate,
        DateTime createdAt)
    {
        if (ledgerId == Guid.Empty)
            return Result.Failure<Account>(ResultError.Validation("Account.Ledger.Empty", "LedgerId is required."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Account>(ResultError.Validation("Account.Name.Empty", "Account name is required."));

        if (type == AccountType.Liability && dueDate is null)
            return Result.Failure<Account>(ResultError.Validation("Account.DueDate.Required", "Liability accounts must have a DueDate."));

        if (type != AccountType.Liability && dueDate is not null)
            return Result.Failure<Account>(ResultError.Validation("Account.DueDate.NotAllowed", "Only Liability accounts can have a DueDate."));

        return Result.Success(new Account(ledgerId, name.Trim(), type, dueDate, createdAt));
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
