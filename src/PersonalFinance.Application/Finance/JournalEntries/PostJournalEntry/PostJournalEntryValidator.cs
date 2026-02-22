using FluentValidation;

namespace PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;

public sealed class PostJournalEntryValidator : AbstractValidator<PostJournalEntryCommand>
{
    public PostJournalEntryValidator()
    {
        RuleFor(x => x.LedgerId).NotEmpty();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x.Lines)
            .NotNull()
            .Must(l => l.Count >= 2)
            .WithMessage("At least two lines are required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.AccountId).NotEmpty();
            line.RuleFor(x => x.Amount).GreaterThan(0);
            line.RuleFor(x => x.Type).IsInEnum();
        });
    }
}
