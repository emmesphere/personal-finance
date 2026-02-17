using PersonalFinance.Application.ToDos.CreateToDo;

using Microsoft.Extensions.DependencyInjection;

namespace PersonalFinance.Application;

public static class ApplicationDependenciInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateToDoHandler>();

        return services;
    }
}
