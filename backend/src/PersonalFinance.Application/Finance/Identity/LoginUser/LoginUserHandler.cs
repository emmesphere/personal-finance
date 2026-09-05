using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Application.Finance.Identity.LoginUser;

public sealed class LoginUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<LoginUserCommand> validator)
{
    public async Task<Result<LoginUserResponse>> HandleAsync(LoginUserCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<LoginUserResponse>(
                ResultError.Validation("User.Validation", validation.Errors[0].ErrorMessage));
        }

        var user = await userRepository.GetByUsernameAsync(command.Username, ct);
        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            return Result.Failure<LoginUserResponse>(
                ResultError.Validation("User.Credentials.Invalid", "Invalid username or password."));
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return Result.Success(new LoginUserResponse(token, user.Id, user.Username, user.Role.ToString()));
    }
}
