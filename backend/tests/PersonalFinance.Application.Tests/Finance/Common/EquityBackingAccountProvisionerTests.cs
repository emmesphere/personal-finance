using PersonalFinance.Application.Finance.Common;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Accounts;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Common;

public sealed class EquityBackingAccountProvisionerTests
{
    private readonly FakeAccountRepository _accountRepository = new();
    private readonly DateTime _now = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    private EquityBackingAccountProvisioner CreateProvisioner() => new(_accountRepository);

    [Fact]
    public async Task GetOrCreateAsync_ShouldCreateBackingAccount_WhenNoneExists()
    {
        var ledgerId = Guid.NewGuid();

        var result = await CreateProvisioner().GetOrCreateAsync(ledgerId, _now, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Type.ShouldBe(AccountType.Equity);
        result.Value.Name.ShouldBe(EquityBackingAccountProvisioner.AccountName);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldReuseExistingBackingAccount_OnSecondCall()
    {
        var ledgerId = Guid.NewGuid();
        var provisioner = CreateProvisioner();

        var first = await provisioner.GetOrCreateAsync(ledgerId, _now, CancellationToken.None);
        var second = await provisioner.GetOrCreateAsync(ledgerId, _now, CancellationToken.None);

        first.IsSuccess.ShouldBeTrue();
        second.IsSuccess.ShouldBeTrue();
        second.Value.Id.ShouldBe(first.Value.Id);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldCreateSeparateAccounts_ForDifferentLedgers()
    {
        var provisioner = CreateProvisioner();

        var forLedgerA = await provisioner.GetOrCreateAsync(Guid.NewGuid(), _now, CancellationToken.None);
        var forLedgerB = await provisioner.GetOrCreateAsync(Guid.NewGuid(), _now, CancellationToken.None);

        forLedgerA.Value.Id.ShouldNotBe(forLedgerB.Value.Id);
    }
}
