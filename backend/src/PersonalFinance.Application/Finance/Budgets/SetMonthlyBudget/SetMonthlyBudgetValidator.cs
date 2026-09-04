using FluentValidation;

namespace PersonalFinance.Application.Finance.Budgets.SetMonthlyBudget;

public sealed class SetMonthlyBudgetValidator : AbstractValidator<SetMonthlyBudgetCommand>
{
    public SetMonthlyBudgetValidator()
    {
        RuleFor(x => x.LedgerId).NotEmpty();
        RuleFor(x => x.Year).GreaterThan(0);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
