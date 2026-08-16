using PersonalFinance.Domain.Finance.Budgets;
using PersonalFinance.Domain.Finance.Common;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Finance.Budgets;

public sealed class MonthlyBudgetTests
{
    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var yearMonth = YearMonth.From(2026, 8);
        var amount = Money.Create(1000m).Value;

        var result = MonthlyBudget.Create(Guid.NewGuid(), yearMonth, amount, DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        result.Value.YearMonth.ShouldBe(yearMonth);
        result.Value.Amount.ShouldBe(amount);
    }

    [Fact]
    public void Create_ShouldFail_WhenLedgerIdIsEmpty()
    {
        var yearMonth = YearMonth.From(2026, 8);
        var amount = Money.Create(1000m).Value;

        var result = MonthlyBudget.Create(Guid.Empty, yearMonth, amount, DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("MonthlyBudget.Ledger.Empty");
    }

    [Fact]
    public void SetAmount_ShouldUpdateAmountAndUpdatedAt()
    {
        var budget = MonthlyBudget.Create(Guid.NewGuid(), YearMonth.From(2026, 8), Money.Create(1000m).Value, DateTime.UtcNow).Value;
        var newAmount = Money.Create(1500m).Value;
        var updatedAt = DateTime.UtcNow.AddDays(1);

        budget.SetAmount(newAmount, updatedAt);

        budget.Amount.ShouldBe(newAmount);
        budget.UpdatedAt.ShouldBe(updatedAt);
    }
}
