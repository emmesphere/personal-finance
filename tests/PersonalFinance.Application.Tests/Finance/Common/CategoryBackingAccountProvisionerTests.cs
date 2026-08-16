using PersonalFinance.Application.Finance.Common;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Categories;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Common;

public sealed class CategoryBackingAccountProvisionerTests
{
    private readonly FakeAccountRepository _accountRepository = new();
    private readonly DateTime _now = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    private CategoryBackingAccountProvisioner CreateProvisioner() => new(_accountRepository);

    [Fact]
    public async Task GetOrCreateAsync_ShouldCreateBackingAccount_WhenNoneExists()
    {
        var ledgerId = Guid.NewGuid();
        var category = Category.Create("Food", CategoryKind.Expense, createdByUserId: null, isSystemDefined: true, _now).Value;

        var result = await CreateProvisioner().GetOrCreateAsync(ledgerId, category, _now, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Type.ShouldBe(AccountType.Expense);
        result.Value.CategoryId.ShouldBe(category.Id);
        result.Value.Name.ShouldBe("Food");
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldCreateIncomeTypeAccount_ForIncomeCategory()
    {
        var ledgerId = Guid.NewGuid();
        var category = Category.Create("Salary", CategoryKind.Income, createdByUserId: null, isSystemDefined: true, _now).Value;

        var result = await CreateProvisioner().GetOrCreateAsync(ledgerId, category, _now, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Type.ShouldBe(AccountType.Income);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldReuseExistingBackingAccount_OnSecondCall()
    {
        var ledgerId = Guid.NewGuid();
        var category = Category.Create("Food", CategoryKind.Expense, createdByUserId: null, isSystemDefined: true, _now).Value;
        var provisioner = CreateProvisioner();

        var first = await provisioner.GetOrCreateAsync(ledgerId, category, _now, CancellationToken.None);
        var second = await provisioner.GetOrCreateAsync(ledgerId, category, _now, CancellationToken.None);

        first.IsSuccess.ShouldBeTrue();
        second.IsSuccess.ShouldBeTrue();
        second.Value.Id.ShouldBe(first.Value.Id);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldCreateSeparateAccounts_ForDifferentLedgers()
    {
        var category = Category.Create("Food", CategoryKind.Expense, createdByUserId: null, isSystemDefined: true, _now).Value;
        var provisioner = CreateProvisioner();

        var forLedgerA = await provisioner.GetOrCreateAsync(Guid.NewGuid(), category, _now, CancellationToken.None);
        var forLedgerB = await provisioner.GetOrCreateAsync(Guid.NewGuid(), category, _now, CancellationToken.None);

        forLedgerA.Value.Id.ShouldNotBe(forLedgerB.Value.Id);
    }
}
