using Microsoft.Extensions.DependencyInjection;

namespace PersonalFinance.Application.Abstractions.Messaging;

public sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public Task<TResponse> Dispatch<TResponse>(ICommand<TResponse> command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);

        return (Task<TResponse>)handlerType
            .GetMethod(nameof(ICommandHandler<ICommand<TResponse>, TResponse>.HandleAsync))!
            .Invoke(handler, new object[] { command, ct })!;
    }
}
