using PersonalFinance.Application.Finance.Budgets.SetMonthlyBudget;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.Ledgers;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Budgets;

public sealed class SetMonthlyBudgetHandlerTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly FakeLedgerRepository _ledgerRepository = new();
    private readonly FakeMonthlyBudgetRepository _budgetRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));

    private SetMonthlyBudgetHandler CreateHandler()
        => new(_ledgerRepository, _budgetRepository, _unitOfWork, _dateTimeProvider, new FakeCurrentUserService(_ownerId), new SetMonthlyBudgetValidator());

    private Ledger SeedLedger()
    {
        var ledger = Ledger.Create("My Ledger", UserId.From(_ownerId), _dateTimeProvider.UtcNow).Value;
        _ledgerRepository.Seed(ledger);
        return ledger;
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateBudget_WhenNoneExists()
    {
        var ledger = SeedLedger();

        var result = await CreateHandler().HandleAsync(new SetMonthlyBudgetCommand(ledger.Id, 2026, 8, 1000m), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var stored = await _budgetRepository.GetAsync(ledger.Id, YearMonth.From(2026, 8), CancellationToken.None);
        stored.ShouldNotBeNull();
        stored.Amount.Amount.ShouldBe(1000m);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateExistingBudget_OnSecondCallForSameMonth()
    {
        var ledger = SeedLedger();
        var handler = CreateHandler();

        await handler.HandleAsync(new SetMonthlyBudgetCommand(ledger.Id, 2026, 8, 1000m), CancellationToken.None);
        var result = await handler.HandleAsync(new SetMonthlyBudgetCommand(ledger.Id, 2026, 8, 1500m), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var stored = await _budgetRepository.GetAsync(ledger.Id, YearMonth.From(2026, 8), CancellationToken.None);
        stored!.Amount.Amount.ShouldBe(1500m);
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenLedgerNotFound()
    {
        var result = await CreateHandler().HandleAsync(new SetMonthlyBudgetCommand(Guid.NewGuid(), 2026, 8, 1000m), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.NotFound");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenMonthOutOfRange()
    {
        var ledger = SeedLedger();

        var result = await CreateHandler().HandleAsync(new SetMonthlyBudgetCommand(ledger.Id, 2026, 13, 1000m), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("MonthlyBudget.Validation");
    }
}
