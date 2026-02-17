using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Domain.Finance.JournalEntries;

public sealed class EntryLine : Entity
{
    public Guid AccountId { get; }
    public EntryType Type { get; }
    public Money Amount { get; }

    private EntryLine(Guid accountId, EntryType type, Money amount)
    {
        AccountId = accountId;
        Type = type;
        Amount = amount;
    }

    internal static Result<EntryLine> Create(Guid accountId, EntryType type, Money amount)
    {
        if (accountId == Guid.Empty)
            return Result.Failure<EntryLine>(ResultError.Validation("EntryLine.Account.Empty", "AccountId is required."));

        return Result.Success(new EntryLine(accountId, type, amount));
    }
}
