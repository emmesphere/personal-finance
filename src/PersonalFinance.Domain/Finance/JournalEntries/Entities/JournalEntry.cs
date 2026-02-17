using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Extensions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Domain.Finance.JournalEntries;

public sealed class JournalEntry : AggregateRoot
{
    private readonly List<EntryLine> _lines = new();

    public Guid LedgerId { get; private set; }
    public UserId CreatedByUserId { get; private set; } = default!;
    public DateOnly Date { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public JournalEntryStatus Status { get; private set; }
    public DateTime? PostedAt { get; private set; }

    public IReadOnlyCollection<EntryLine> Lines => _lines.AsReadOnly();

    private JournalEntry() { }

    private JournalEntry(Guid ledgerId, UserId createdByUserId, DateOnly date, string description)
    {
        LedgerId = ledgerId;
        CreatedByUserId = createdByUserId;
        Date = date;
        Description = description ?? string.Empty;
        Status = JournalEntryStatus.Draft;
    }

    public static Result<JournalEntry> Create(Guid ledgerId, UserId createdByUserId, DateOnly date, string description)
    {
        if (ledgerId == Guid.Empty)
            return Result.Failure<JournalEntry>(
                ResultError.Validation("JournalEntry.Ledger.Empty", "LedgerId is required."));

        return Result.Success(new JournalEntry(ledgerId, createdByUserId, date, description));
    }

    public Result ChangeDescription(string description)
    {
        if (Status != JournalEntryStatus.Draft)
            return Result.Failure(ResultError.Conflict("JournalEntry.Immutable", "Cannot edit a posted journal entry."));

        Description = description ?? string.Empty;
        return Result.Success();
    }

    public Result ChangeDate(DateOnly date)
    {
        if (Status != JournalEntryStatus.Draft)
            return Result.Failure(ResultError.Conflict("JournalEntry.Immutable", "Cannot edit a posted journal entry."));

        Date = date;
        return Result.Success();
    }

    public Result<EntryLine> AddLine(Guid accountId, EntryType type, Money amount)
    {
        if (Status != JournalEntryStatus.Draft)
            return Result.Failure<EntryLine>(ResultError.Conflict("JournalEntry.Immutable", "Cannot edit a posted journal entry."));

        var lineResult = EntryLine.Create(accountId, type, amount);
        if (lineResult.IsFailure)
            return Result.Failure<EntryLine>(lineResult.Error);

        _lines.Add(lineResult.Value);
        return Result.Success(lineResult.Value);
    }

    public Result RemoveLine(Guid lineId)
    {
        if (Status != JournalEntryStatus.Draft)
            return Result.Failure(ResultError.Conflict("JournalEntry.Immutable", "Cannot edit a posted journal entry."));

        var idx = _lines.FindIndex(l => l.Id == lineId);
        if (idx < 0)
            return Result.Failure(ResultError.NotFound("JournalEntry.Line.NotFound", "Entry line not found."));

        _lines.RemoveAt(idx);
        return Result.Success();
    }

    public Result Post(DateTime eventTime, Ledger ledger, IReadOnlyCollection<Account> accountsInvolved)
    {
        if (Status == JournalEntryStatus.Posted)
            return Result.Failure(ResultError.Conflict("JournalEntry.AlreadyPosted", "Journal entry already posted."));

        if (_lines.Count < 2)
            return Result.Failure(ResultError.Validation("JournalEntry.Lines.Min", "At least two lines are required."));

        if (ledger is null)
            return Result.Failure(ResultError.Validation("JournalEntry.Ledger.Null", "Ledger cannot be null."));

        var memberCheck = ledger.EnsureMember(CreatedByUserId);
        if (memberCheck.IsFailure)
            return memberCheck;

        var accountById = accountsInvolved.ToDictionary(a => a.Id, a => a);

        foreach (var line in _lines)
        {
            if (!accountById.TryGetValue(line.AccountId, out var acc))
                return Result.Failure(ResultError.NotFound("JournalEntry.Account.NotFound", "An account used in entry lines was not provided."));

            if (acc.LedgerId != LedgerId)
                return Result.Failure(ResultError.Conflict("JournalEntry.Account.WrongLedger", "All accounts must belong to the same ledger."));
        }

        var debitTotal = _lines.Where(l => l.Type == EntryType.Debit).Sum(l => l.Amount.Amount);
        var creditTotal = _lines.Where(l => l.Type == EntryType.Credit).Sum(l => l.Amount.Amount);

        if (debitTotal != creditTotal)
            return Result.Failure(ResultError.Validation("JournalEntry.Unbalanced", "Debits must equal credits."));

        Status = JournalEntryStatus.Posted;
        PostedAt = eventTime;

        AddDomainEvent(new JournalEntryPostedDomainEvent(
            eventTime,
            LedgerId,
            Id,
            Date,
            debitTotal));

        return Result.Success();
    }
}