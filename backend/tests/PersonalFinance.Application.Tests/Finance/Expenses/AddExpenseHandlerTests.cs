using PersonalFinance.Application.Finance.Common;
using PersonalFinance.Application.Finance.Expenses.AddExpense;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Categories;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;
using PersonalFinance.Domain.Finance.Ledgers;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Expenses;

public sealed class AddExpenseHandlerTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly FakeLedgerRepository _ledgerRepository = new();
    private readonly FakeAccountRepository _accountRepository = new();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeJournalEntryRepository _journalEntryRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));

    private AddExpenseHandler CreateHandler()
        => new(
            _ledgerRepository,
            _accountRepository,
            _categoryRepository,
            _journalEntryRepository,
            _unitOfWork,
            _dateTimeProvider,
            new FakeCurrentUserService(_ownerId),
            new CategoryBackingAccountProvisioner(_accountRepository),
            new AddExpenseValidator());

    private (Ledger Ledger, Account Account) SeedLedgerWithAccount(AccountType accountType)
    {
        var ledger = Ledger.Create("My Ledger", UserId.From(_ownerId), _dateTimeProvider.UtcNow).Value;
        var dueDate = accountType == AccountType.CreditCard ? DueDate.Create(10).Value : null;
        var account = Account.Create(ledger.Id, "Payment", accountType, dueDate, categoryId: null, _dateTimeProvider.UtcNow).Value;
        _ledgerRepository.Seed(ledger);
        _accountRepository.Seed(account);
        return (ledger, account);
    }

    private Category SeedExpenseCategory()
    {
        var category = Category.Create("Food", CategoryKind.Expense, createdByUserId: null, isSystemDefined: true, _dateTimeProvider.UtcNow).Value;
        _categoryRepository.Seed(category);
        return category;
    }

    [Fact]
    public async Task HandleAsync_ShouldPostSingleEntry_WithNoInstallmentFields_ForWalletPayment()
    {
        var (ledger, wallet) = SeedLedgerWithAccount(AccountType.Wallet);
        var category = SeedExpenseCategory();

        var command = new AddExpenseCommand(ledger.Id, category.Id, wallet.Id, 100m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), "Groceries", null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.InstallmentPlanId.ShouldBeNull();
        _journalEntryRepository.Added.Count.ShouldBe(1);
        var entry = _journalEntryRepository.Added[0];
        entry.Lines.Single(l => l.Type == EntryType.Credit).AccountId.ShouldBe(wallet.Id);
        entry.InstallmentPlanId.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldSetInstallmentPlan_WithCountOne_ForOneTimeCreditCardCharge()
    {
        var (ledger, card) = SeedLedgerWithAccount(AccountType.CreditCard);
        var category = SeedExpenseCategory();

        var command = new AddExpenseCommand(ledger.Id, category.Id, card.Id, 100m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), "Dinner", null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.InstallmentPlanId.ShouldNotBeNull();
        result.Value.InstallmentCount.ShouldBe(1);
        _journalEntryRepository.Added.Count.ShouldBe(1);
        var entry = _journalEntryRepository.Added[0];
        entry.InstallmentNumber.ShouldBe(1);
        entry.InstallmentTotalCount.ShouldBe(1);
        entry.Lines.Single(l => l.Type == EntryType.Credit).AccountId.ShouldBe(card.Id);
    }

    [Fact]
    public async Task HandleAsync_ShouldSplitAcrossThreeMonths_WithRemainderOnFirstInstallment()
    {
        var (ledger, card) = SeedLedgerWithAccount(AccountType.CreditCard);
        var category = SeedExpenseCategory();
        var purchaseDate = new DateOnly(2026, 1, 15);

        var command = new AddExpenseCommand(ledger.Id, category.Id, card.Id, 100m, purchaseDate, "New TV", 3);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.InstallmentCount.ShouldBe(3);
        _journalEntryRepository.Added.Count.ShouldBe(3);

        var entries = _journalEntryRepository.Added.OrderBy(e => e.InstallmentNumber).ToList();

        // 100 / 3 = 33.33 base; remainder goes to installment 1: 33.34 + 33.33 + 33.33 = 100.00
        entries[0].Lines.Single(l => l.Type == EntryType.Debit).Amount.Amount.ShouldBe(33.34m);
        entries[1].Lines.Single(l => l.Type == EntryType.Debit).Amount.Amount.ShouldBe(33.33m);
        entries[2].Lines.Single(l => l.Type == EntryType.Debit).Amount.Amount.ShouldBe(33.33m);

        entries[0].Date.ShouldBe(new DateOnly(2026, 1, 15));
        entries[1].Date.ShouldBe(new DateOnly(2026, 2, 15));
        entries[2].Date.ShouldBe(new DateOnly(2026, 3, 15));

        var planId = entries[0].InstallmentPlanId;
        planId.ShouldNotBeNull();
        entries.ShouldAllBe(e => e.InstallmentPlanId == planId);
        entries.ShouldAllBe(e => e.InstallmentTotalCount == 3);
        entries.Select(e => e.InstallmentNumber).ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenInstallmentsRequestedForNonCreditCardAccount()
    {
        var (ledger, wallet) = SeedLedgerWithAccount(AccountType.Wallet);
        var category = SeedExpenseCategory();

        var command = new AddExpenseCommand(ledger.Id, category.Id, wallet.Id, 100m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), null, 3);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Expense.InstallmentsRequireCreditCard");
        _journalEntryRepository.Added.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenCategoryKindIsIncome()
    {
        var (ledger, wallet) = SeedLedgerWithAccount(AccountType.Wallet);
        var category = Category.Create("Salary", CategoryKind.Income, createdByUserId: null, isSystemDefined: true, _dateTimeProvider.UtcNow).Value;
        _categoryRepository.Seed(category);

        var command = new AddExpenseCommand(ledger.Id, category.Id, wallet.Id, 100m, DateOnly.FromDateTime(_dateTimeProvider.UtcNow), null, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.WrongKind");
    }
}
