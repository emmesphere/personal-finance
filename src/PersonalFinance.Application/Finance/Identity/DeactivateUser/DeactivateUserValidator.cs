using FluentValidation;

namespace PersonalFinance.Application.Finance.Identity.DeactivateUser;

public sealed class DeactivateUserValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
