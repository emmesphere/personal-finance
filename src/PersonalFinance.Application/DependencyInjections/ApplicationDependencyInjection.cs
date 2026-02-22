using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using PersonalFinance.Application.Abstractions.Messaging;
using PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        // Dispatcher
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        // Handler
        services.AddScoped<
            ICommandHandler<PostJournalEntryCommand, Result<PostJournalEntryResponse>>,
            PostJournalEntryHandler>();

        // Validator
        services.AddScoped<IValidator<PostJournalEntryCommand>, PostJournalEntryValidator>();

        return services;
    }
}
