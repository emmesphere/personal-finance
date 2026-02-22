using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PersonalFinance.Application.Abstractions.Events;
using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.Infrastructure.Events;
using PersonalFinance.Infrastructure.Persistence;
using PersonalFinance.Infrastructure.Persistence.InMemory;
using PersonalFinance.Infrastructure.Persistence.Repositories;
using PersonalFinance.Infrastructure.Time;

namespace PersonalFinance.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PersonalFinanceDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .UseSnakeCaseNamingConvention();
        });

        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
