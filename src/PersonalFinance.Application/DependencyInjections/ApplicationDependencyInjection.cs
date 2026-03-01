using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;

namespace PersonalFinance.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Validator
        services.AddScoped<IValidator<PostJournalEntryCommand>, PostJournalEntryValidator>();

        return services;
    }
}
