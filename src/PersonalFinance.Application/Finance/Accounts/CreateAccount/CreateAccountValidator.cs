using FluentValidation;

using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Application.Finance.Accounts.CreateAccount;

public sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.LedgerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Type)
            .IsInEnum()
            .Must(type => type is not (AccountType.Income or AccountType.Expense))
            .WithMessage("Income and Expense account types are managed automatically and cannot be created directly.");

        RuleFor(x => x.DueDateDay)
            .InclusiveBetween(1, 31)
            .When(x => x.DueDateDay.HasValue);
    }
}
