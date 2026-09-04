using FluentValidation;

namespace PersonalFinance.Application.Finance.Incomes.AddIncome;

public sealed class AddIncomeValidator : AbstractValidator<AddIncomeCommand>
{
    public AddIncomeValidator()
    {
        RuleFor(x => x.LedgerId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ReceivingAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
