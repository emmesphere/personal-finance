namespace PersonalFinance.Application.Abstractions.Messaging;

#pragma warning disable CA1040 // Avoid empty interfaces
#pragma warning disable S2326 // Unused type parameters should be removed
public interface ICommand<out TResponse> { }
#pragma warning restore S2326 // Unused type parameters should be removed
#pragma warning restore CA1040 // Avoid empty interfaces