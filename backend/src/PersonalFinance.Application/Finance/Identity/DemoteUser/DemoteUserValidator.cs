using FluentValidation;

namespace PersonalFinance.Application.Finance.Identity.DemoteUser;

public sealed class DemoteUserValidator : AbstractValidator<DemoteUserCommand>
{
    public DemoteUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
