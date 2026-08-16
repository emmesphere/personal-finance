using PersonalFinance.Application.Finance.Identity.RegisterUser;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.WebApi.Contracts;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Auth;

internal static class RegisterUserEndpoint
{
    public static IEndpointRouteBuilder MapRegisterUser(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/register", async (
            RegisterUserRequest request,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var command = new RegisterUserCommand(
                request.FullName, request.Username, request.Email, request.PhoneNumber, request.Password);

            var result = await bus.InvokeAsync<Result<RegisterUserResponse>>(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value.UserId}", result.Value)
                : result.ToHttp();
        });

        return endpoints;
    }
}
