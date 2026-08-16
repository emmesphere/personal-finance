using FluentValidation;

namespace PersonalFinance.Application.Finance.Categories.DeactivateCategory;

public sealed class DeactivateCategoryValidator : AbstractValidator<DeactivateCategoryCommand>
{
    public DeactivateCategoryValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
