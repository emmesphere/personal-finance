using PersonalFinance.Application.Abstractions.Events;
using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.Infrastructure.Events;
using PersonalFinance.Infrastructure.Persistence.InMemory;
using PersonalFinance.Infrastructure.Time;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PersonalFinance.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IToDoListRepository, InMemoryToDoListRepository>();
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
