using PersonalFinance.Application.Finance.Accounts.CreateAccount;
using PersonalFinance.Application.Finance.Common;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;
using PersonalFinance.Domain.Finance.Ledgers;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Accounts;

public sealed class CreateAccountHandlerTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly FakeLedgerRepository _ledgerRepository = new();
    private readonly FakeAccountRepository _accountRepository = new();
    private readonly FakeJournalEntryRepository _journalEntryRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));

    private CreateAccountHandler CreateHandler()
        => new(
            _ledgerRepository,
            _accountRepository,
            _journalEntryRepository,
            _unitOfWork,
            _dateTimeProvider,
            new FakeCurrentUserService(_ownerId),
            new EquityBackingAccountProvisioner(_accountRepository),
            new CreateAccountValidator());

    private Ledger SeedLedger()
    {
        var ledger = Ledger.Create("My Ledger", UserId.From(_ownerId), _dateTimeProvider.UtcNow).Value;
        _ledgerRepository.Seed(ledger);
        return ledger;
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateAccount_WhenValid()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _unitOfWork.SaveChangesCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateAccount_WithDueDate_ForCreditCard()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "My Card", AccountType.CreditCard, 15);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenLedgerNotFound()
    {
        var command = new CreateAccountCommand(Guid.NewGuid(), "Wallet", AccountType.Wallet, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.NotFound");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenCallerIsNotLedgerMember()
    {
        var ledger = Ledger.Create("Other Ledger", UserId.From(Guid.NewGuid()), _dateTimeProvider.UtcNow).Value;
        _ledgerRepository.Seed(ledger);

        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.Member.Required");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenAccountNameAlreadyTaken()
    {
        var ledger = SeedLedger();
        _accountRepository.Seed(Account.Create(ledger.Id, "Wallet", AccountType.Wallet, null, null, _dateTimeProvider.UtcNow).Value);

        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.Name.Taken");
    }

    [Theory]
    [InlineData(AccountType.Income)]
    [InlineData(AccountType.Expense)]
    [InlineData(AccountType.Equity)]
    public async Task HandleAsync_ShouldFail_WhenTypeIsSystemManaged(AccountType type)
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Salary", type, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.Validation");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenOpeningBalanceIsNotPositive()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null, OpeningBalance: 0);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.Validation");
    }

    [Fact]
    public async Task HandleAsync_ShouldPostBalancedOpeningEntry_ForAssetAccount()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null, OpeningBalance: 500m);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _journalEntryRepository.Added.Count.ShouldBe(1);

        var entry = _journalEntryRepository.Added[0];
        var accountLine = entry.Lines.Single(l => l.AccountId == result.Value.AccountId);
        var equityLine = entry.Lines.Single(l => l.AccountId != result.Value.AccountId);

        accountLine.Type.ShouldBe(EntryType.Debit);
        accountLine.Amount.Amount.ShouldBe(500m);
        equityLine.Type.ShouldBe(EntryType.Credit);
        equityLine.Amount.Amount.ShouldBe(500m);
    }

    [Fact]
    public async Task HandleAsync_ShouldPostBalancedOpeningEntry_ForCreditCardAccount()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "My Card", AccountType.CreditCard, null, OpeningBalance: 250m);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var entry = _journalEntryRepository.Added.Single();
        var accountLine = entry.Lines.Single(l => l.AccountId == result.Value.AccountId);
        var equityLine = entry.Lines.Single(l => l.AccountId != result.Value.AccountId);

        accountLine.Type.ShouldBe(EntryType.Credit);
        equityLine.Type.ShouldBe(EntryType.Debit);
    }

    [Fact]
    public async Task HandleAsync_ShouldPostBalancedOpeningEntry_ForLoanAccount()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Car Loan", AccountType.Loan, 5, OpeningBalance: 500m);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var entry = _journalEntryRepository.Added.Single();
        var accountLine = entry.Lines.Single(l => l.AccountId == result.Value.AccountId);
        var equityLine = entry.Lines.Single(l => l.AccountId != result.Value.AccountId);

        accountLine.Type.ShouldBe(EntryType.Credit);
        accountLine.Amount.Amount.ShouldBe(500m);
        equityLine.Type.ShouldBe(EntryType.Debit);
    }

    [Fact]
    public async Task HandleAsync_ShouldReuseSameEquityAccount_AcrossMultipleOpeningBalances()
    {
        var ledger = SeedLedger();
        var firstCommand = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null, OpeningBalance: 100m);
        var secondCommand = new CreateAccountCommand(ledger.Id, "Bank", AccountType.BankAccount, null, OpeningBalance: 200m);

        var handler = CreateHandler();
        await handler.HandleAsync(firstCommand, CancellationToken.None);
        await handler.HandleAsync(secondCommand, CancellationToken.None);

        var equityAccountIds = _journalEntryRepository.Added
            .Select(e => e.Lines.Single(l => l.Type == EntryType.Credit).AccountId)
            .Distinct()
            .ToList();

        equityAccountIds.Count.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotPostJournalEntry_WhenOpeningBalanceOmitted()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _journalEntryRepository.Added.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenDueDateProvided_ForNonCreditCardOrDebitType()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, 10);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.DueDate.NotAllowed");
    }
}
