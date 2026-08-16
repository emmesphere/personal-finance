using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;
using PersonalFinance.Domain.Finance.Ledgers;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Finance.JournalEntries;

public sealed class JournalEntryTests
{
    private static (Ledger Ledger, Account AccountA, Account AccountB) CreateLedgerWithAccounts(UserId owner)
    {
        var ledger = Ledger.Create("My Ledger", owner, DateTime.UtcNow).Value;
        var accountA = Account.Create(ledger.Id, "Wallet", AccountType.Wallet, dueDate: null, categoryId: null, DateTime.UtcNow).Value;
        var accountB = Account.Create(ledger.Id, "Food", AccountType.Expense, dueDate: null, categoryId: Guid.NewGuid(), DateTime.UtcNow).Value;

        return (ledger, accountA, accountB);
    }

    [Fact]
    public void Create_ShouldStartAsDraft()
    {
        var owner = UserId.From(Guid.NewGuid());

        var result = JournalEntry.Create(Guid.NewGuid(), owner, DateOnly.FromDateTime(DateTime.UtcNow), "test");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(JournalEntryStatus.Draft);
    }

    [Fact]
    public void Create_ShouldFail_WhenLedgerIdIsEmpty()
    {
        var result = JournalEntry.Create(Guid.Empty, UserId.From(Guid.NewGuid()), DateOnly.FromDateTime(DateTime.UtcNow), "test");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.Ledger.Empty");
    }

    [Fact]
    public void AddLine_ShouldFail_AfterPosted()
    {
        var owner = UserId.From(Guid.NewGuid());
        var (ledger, accountA, accountB) = CreateLedgerWithAccounts(owner);
        var entry = JournalEntry.Create(ledger.Id, owner, DateOnly.FromDateTime(DateTime.UtcNow), "test").Value;
        entry.AddLine(accountA.Id, EntryType.Debit, Money.Create(10m).Value);
        entry.AddLine(accountB.Id, EntryType.Credit, Money.Create(10m).Value);
        entry.Post(DateTime.UtcNow, ledger, [accountA, accountB]);

        var result = entry.AddLine(accountA.Id, EntryType.Debit, Money.Create(5m).Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.Immutable");
    }

    [Fact]
    public void Post_ShouldFail_WhenFewerThanTwoLines()
    {
        var owner = UserId.From(Guid.NewGuid());
        var (ledger, accountA, _) = CreateLedgerWithAccounts(owner);
        var entry = JournalEntry.Create(ledger.Id, owner, DateOnly.FromDateTime(DateTime.UtcNow), "test").Value;
        entry.AddLine(accountA.Id, EntryType.Debit, Money.Create(10m).Value);

        var result = entry.Post(DateTime.UtcNow, ledger, [accountA]);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.Lines.Min");
    }

    [Fact]
    public void Post_ShouldFail_WhenDebitsDoNotEqualCredits()
    {
        var owner = UserId.From(Guid.NewGuid());
        var (ledger, accountA, accountB) = CreateLedgerWithAccounts(owner);
        var entry = JournalEntry.Create(ledger.Id, owner, DateOnly.FromDateTime(DateTime.UtcNow), "test").Value;
        entry.AddLine(accountA.Id, EntryType.Debit, Money.Create(10m).Value);
        entry.AddLine(accountB.Id, EntryType.Credit, Money.Create(5m).Value);

        var result = entry.Post(DateTime.UtcNow, ledger, [accountA, accountB]);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.Unbalanced");
    }

    [Fact]
    public void Post_ShouldFail_WhenCreatorIsNotLedgerMember()
    {
        var owner = UserId.From(Guid.NewGuid());
        var stranger = UserId.From(Guid.NewGuid());
        var (ledger, accountA, accountB) = CreateLedgerWithAccounts(owner);
        var entry = JournalEntry.Create(ledger.Id, stranger, DateOnly.FromDateTime(DateTime.UtcNow), "test").Value;
        entry.AddLine(accountA.Id, EntryType.Debit, Money.Create(10m).Value);
        entry.AddLine(accountB.Id, EntryType.Credit, Money.Create(10m).Value);

        var result = entry.Post(DateTime.UtcNow, ledger, [accountA, accountB]);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.Member.Required");
    }

    [Fact]
    public void Post_ShouldFail_WhenAccountBelongsToDifferentLedger()
    {
        var owner = UserId.From(Guid.NewGuid());
        var (ledger, accountA, _) = CreateLedgerWithAccounts(owner);
        var otherLedgerAccount = Account.Create(Guid.NewGuid(), "Other", AccountType.Expense, dueDate: null, categoryId: Guid.NewGuid(), DateTime.UtcNow).Value;
        var entry = JournalEntry.Create(ledger.Id, owner, DateOnly.FromDateTime(DateTime.UtcNow), "test").Value;
        entry.AddLine(accountA.Id, EntryType.Debit, Money.Create(10m).Value);
        entry.AddLine(otherLedgerAccount.Id, EntryType.Credit, Money.Create(10m).Value);

        var result = entry.Post(DateTime.UtcNow, ledger, [accountA, otherLedgerAccount]);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.Account.WrongLedger");
    }

    [Fact]
    public void Post_ShouldSucceed_AndRaiseDomainEvent_WhenBalanced()
    {
        var owner = UserId.From(Guid.NewGuid());
        var (ledger, accountA, accountB) = CreateLedgerWithAccounts(owner);
        var entry = JournalEntry.Create(ledger.Id, owner, DateOnly.FromDateTime(DateTime.UtcNow), "test").Value;
        entry.AddLine(accountA.Id, EntryType.Debit, Money.Create(10m).Value);
        entry.AddLine(accountB.Id, EntryType.Credit, Money.Create(10m).Value);

        var result = entry.Post(DateTime.UtcNow, ledger, [accountA, accountB]);

        result.IsSuccess.ShouldBeTrue();
        entry.Status.ShouldBe(JournalEntryStatus.Posted);
        entry.PostedAt.ShouldNotBeNull();
        entry.DomainEvents.Count.ShouldBe(1);
        entry.DomainEvents.Single().ShouldBeOfType<JournalEntryPostedDomainEvent>();
    }

    [Fact]
    public void Post_ShouldFail_WhenAlreadyPosted()
    {
        var owner = UserId.From(Guid.NewGuid());
        var (ledger, accountA, accountB) = CreateLedgerWithAccounts(owner);
        var entry = JournalEntry.Create(ledger.Id, owner, DateOnly.FromDateTime(DateTime.UtcNow), "test").Value;
        entry.AddLine(accountA.Id, EntryType.Debit, Money.Create(10m).Value);
        entry.AddLine(accountB.Id, EntryType.Credit, Money.Create(10m).Value);
        entry.Post(DateTime.UtcNow, ledger, [accountA, accountB]);

        var result = entry.Post(DateTime.UtcNow, ledger, [accountA, accountB]);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.AlreadyPosted");
    }
}
