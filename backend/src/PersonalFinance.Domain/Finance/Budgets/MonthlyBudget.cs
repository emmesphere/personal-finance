using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Domain.Finance.Budgets;

public sealed class MonthlyBudget : AggregateRoot
{
    public Guid LedgerId { get; private set; }
    public YearMonth YearMonth { get; private set; } = default!;
    public Money Amount { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private MonthlyBudget() { }

    private MonthlyBudget(Guid ledgerId, YearMonth yearMonth, Money amount, DateTime createdAt)
    {
        LedgerId = ledgerId;
        YearMonth = yearMonth;
        Amount = amount;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static Result<MonthlyBudget> Create(Guid ledgerId, YearMonth yearMonth, Money amount, DateTime createdAt)
    {
        if (ledgerId == Guid.Empty)
            return Result.Failure<MonthlyBudget>(ResultError.Validation("MonthlyBudget.Ledger.Empty", "LedgerId is required."));

        ArgumentNullException.ThrowIfNull(yearMonth);
        ArgumentNullException.ThrowIfNull(amount);

        return Result.Success(new MonthlyBudget(ledgerId, yearMonth, amount, createdAt));
    }

    public Result SetAmount(Money amount, DateTime updatedAt)
    {
        ArgumentNullException.ThrowIfNull(amount);

        Amount = amount;
        UpdatedAt = updatedAt;
        return Result.Success();
    }
}
