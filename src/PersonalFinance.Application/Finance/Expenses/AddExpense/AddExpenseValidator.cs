using FluentValidation;

namespace PersonalFinance.Application.Finance.Expenses.AddExpense;

public sealed class AddExpenseValidator : AbstractValidator<AddExpenseCommand>
{
    public AddExpenseValidator()
    {
        RuleFor(x => x.LedgerId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PaymentAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x.InstallmentCount)
            .GreaterThanOrEqualTo(1)
            .When(x => x.InstallmentCount.HasValue);
    }
}
