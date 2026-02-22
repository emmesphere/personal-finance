namespace PersonalFinance.Application.Abstractions.Messaging;

public interface ICommandDispatcher
{
    Task<TResponse> Dispatch<TResponse>(ICommand<TResponse> command, CancellationToken ct);
}
