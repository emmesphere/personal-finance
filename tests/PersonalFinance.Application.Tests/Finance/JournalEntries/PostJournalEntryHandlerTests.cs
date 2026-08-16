using PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;
using PersonalFinance.Domain.Finance.Ledgers;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.JournalEntries;

public sealed class PostJournalEntryHandlerTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly FakeLedgerRepository _ledgerRepository = new();
    private readonly FakeAccountRepository _accountRepository = new();
    private readonly FakeJournalEntryRepository _journalEntryRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));

    private PostJournalEntryHandler CreateHandler()
        => new(
            _ledgerRepository,
            _accountRepository,
            _journalEntryRepository,
            _unitOfWork,
            _dateTimeProvider,
            new FakeCurrentUserService(_ownerId),
            new PostJournalEntryValidator());

    private (Ledger Ledger, Account Wallet, Account Food) SeedLedgerWithAccounts()
    {
        var ledger = Ledger.Create("My Ledger", UserId.From(_ownerId), _dateTimeProvider.UtcNow).Value;
        var wallet = Account.Create(ledger.Id, "Wallet", AccountType.Wallet, dueDate: null, categoryId: null, _dateTimeProvider.UtcNow).Value;
        var food = Account.Create(ledger.Id, "Food", AccountType.Expense, dueDate: null, categoryId: Guid.NewGuid(), _dateTimeProvider.UtcNow).Value;

        _ledgerRepository.Seed(ledger);
        _accountRepository.Seed(wallet, food);

        return (ledger, wallet, food);
    }

    [Fact]
    public async Task HandleAsync_ShouldPostEntry_WhenLinesBalance()
    {
        var (ledger, wallet, food) = SeedLedgerWithAccounts();
        var command = new PostJournalEntryCommand(
            ledger.Id,
            DateOnly.FromDateTime(_dateTimeProvider.UtcNow),
            "Groceries",
            [
                new PostJournalEntryLineDto(food.Id, EntryType.Debit, 50m),
                new PostJournalEntryLineDto(wallet.Id, EntryType.Credit, 50m),
            ]);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _journalEntryRepository.Added.Count.ShouldBe(1);
        _journalEntryRepository.Added[0].Status.ShouldBe(JournalEntryStatus.Posted);
        _unitOfWork.SaveChangesCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenLedgerNotFound()
    {
        var command = new PostJournalEntryCommand(
            Guid.NewGuid(),
            DateOnly.FromDateTime(_dateTimeProvider.UtcNow),
            "Groceries",
            [
                new PostJournalEntryLineDto(Guid.NewGuid(), EntryType.Debit, 50m),
                new PostJournalEntryLineDto(Guid.NewGuid(), EntryType.Credit, 50m),
            ]);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.NotFound");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenLinesAreUnbalanced()
    {
        var (ledger, wallet, food) = SeedLedgerWithAccounts();
        var command = new PostJournalEntryCommand(
            ledger.Id,
            DateOnly.FromDateTime(_dateTimeProvider.UtcNow),
            "Groceries",
            [
                new PostJournalEntryLineDto(food.Id, EntryType.Debit, 50m),
                new PostJournalEntryLineDto(wallet.Id, EntryType.Credit, 40m),
            ]);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.Unbalanced");
        _journalEntryRepository.Added.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenValidationFails()
    {
        var command = new PostJournalEntryCommand(
            Guid.Empty,
            DateOnly.FromDateTime(_dateTimeProvider.UtcNow),
            "Groceries",
            [
                new PostJournalEntryLineDto(Guid.NewGuid(), EntryType.Debit, 50m),
                new PostJournalEntryLineDto(Guid.NewGuid(), EntryType.Credit, 50m),
            ]);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.Validation");
        _unitOfWork.SaveChangesCallCount.ShouldBe(0);
    }
}
