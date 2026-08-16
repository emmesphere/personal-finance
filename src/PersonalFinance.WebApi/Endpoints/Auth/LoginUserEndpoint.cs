using PersonalFinance.Application.Finance.Identity.LoginUser;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.WebApi.Contracts;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Auth;

internal static class LoginUserEndpoint
{
    public static IEndpointRouteBuilder MapLoginUser(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/login", async (
            LoginUserRequest request,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var command = new LoginUserCommand(request.Username, request.Password);
            var result = await bus.InvokeAsync<Result<LoginUserResponse>>(command, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttp();
        });

        return endpoints;
    }
}
