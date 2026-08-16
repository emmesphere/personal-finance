using PersonalFinance.Application.Finance.Common;
using PersonalFinance.Application.Finance.Incomes.AddIncome;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Categories;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;
using PersonalFinance.Domain.Finance.Ledgers;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Incomes;

public sealed class AddIncomeHandlerTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly FakeLedgerRepository _ledgerRepository = new();
    private readonly FakeAccountRepository _accountRepository = new();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeJournalEntryRepository _journalEntryRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));

    private AddIncomeHandler CreateHandler()
        => new(
            _ledgerRepository,
            _accountRepository,
            _categoryRepository,
            _journalEntryRepository,
            _unitOfWork,
            _dateTimeProvider,
            new FakeCurrentUserService(_ownerId),
            new CategoryBackingAccountProvisioner(_accountRepository),
            new AddIncomeValidator());

    private (Ledger Ledger, Account Wallet) SeedLedgerWithAccount(AccountType accountType = AccountType.Wallet)
    {
        var ledger = Ledger.Create("My Ledger", UserId.From(_ownerId), _dateTimeProvider.UtcNow).Value;
        var account = Account.Create(ledger.Id, "Wallet", accountType, dueDate: null, categoryId: null, _dateTimeProvider.UtcNow).Value;
        _ledgerRepository.Seed(ledger);
        _accountRepository.Seed(account);
        return (ledger, account);
    }

    private Category SeedCategory(CategoryKind kind = CategoryKind.Income, bool isActive = true)
    {
        var category = Category.Create("Salary", kind, createdByUserId: null, isSystemDefined: true, _dateTimeProvider.UtcNow).Value;
        if (!isActive)
            category.Deactivate();

        _categoryRepository.Seed(category);
        return category;
    }

    [Fact]
    public async Task HandleAsync_ShouldPostBalancedEntry_DebitingReceivingAccount_CreditingBackingAccount()
    {
        var (ledger, wallet) = SeedLedgerWithAccount();
        var category = SeedCategory();

        var command = new AddIncomeCommand(ledger.Id, category.Id, wallet.Id, 1000m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), "January salary");

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _journalEntryRepository.Added.Count.ShouldBe(1);
        var entry = _journalEntryRepository.Added[0];
        entry.Status.ShouldBe(JournalEntryStatus.Posted);
        entry.Lines.Single(l => l.Type == EntryType.Debit).AccountId.ShouldBe(wallet.Id);
        entry.Lines.Single(l => l.Type == EntryType.Credit).Amount.Amount.ShouldBe(1000m);
        _unitOfWork.SaveChangesCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenCategoryKindIsExpense()
    {
        var (ledger, wallet) = SeedLedgerWithAccount();
        var category = SeedCategory(CategoryKind.Expense);

        var command = new AddIncomeCommand(ledger.Id, category.Id, wallet.Id, 100m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.WrongKind");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenCategoryIsInactive()
    {
        var (ledger, wallet) = SeedLedgerWithAccount();
        var category = SeedCategory(isActive: false);

        var command = new AddIncomeCommand(ledger.Id, category.Id, wallet.Id, 100m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.Inactive");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenReceivingAccountBelongsToDifferentLedger()
    {
        var (ledger, _) = SeedLedgerWithAccount();
        var category = SeedCategory();
        var otherAccountId = Guid.NewGuid();

        var command = new AddIncomeCommand(ledger.Id, category.Id, otherAccountId, 100m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.NotFound");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenReceivingAccountIsCreditCard()
    {
        var (ledger, creditCard) = SeedLedgerWithAccount(AccountType.CreditCard);
        var category = SeedCategory();

        var command = new AddIncomeCommand(ledger.Id, category.Id, creditCard.Id, 100m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.NotEligibleForIncome");
    }

    [Fact]
    public async Task HandleAsync_ShouldReuseBackingAccount_OnSecondIncomeForSameCategory()
    {
        var (ledger, wallet) = SeedLedgerWithAccount();
        var category = SeedCategory();
        var handler = CreateHandler();

        await handler.HandleAsync(new AddIncomeCommand(ledger.Id, category.Id, wallet.Id, 100m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), null), CancellationToken.None);
        await handler.HandleAsync(new AddIncomeCommand(ledger.Id, category.Id, wallet.Id, 200m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), null), CancellationToken.None);

        var backingAccounts = (await _accountRepository.ListByLedgerAsync(ledger.Id, CancellationToken.None))
            .Where(a => a.Type == AccountType.Income)
            .ToList();

        backingAccounts.Count.ShouldBe(1);
    }
}
