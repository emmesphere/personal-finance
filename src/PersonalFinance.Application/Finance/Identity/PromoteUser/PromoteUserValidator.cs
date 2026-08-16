using FluentValidation;

namespace PersonalFinance.Application.Finance.Identity.PromoteUser;

public sealed class PromoteUserValidator : AbstractValidator<PromoteUserCommand>
{
    public PromoteUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
