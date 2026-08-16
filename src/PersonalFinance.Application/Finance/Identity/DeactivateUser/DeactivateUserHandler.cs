using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Identity.Users;

namespace PersonalFinance.Application.Finance.Identity.DeactivateUser;

public sealed class DeactivateUserHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IValidator<DeactivateUserCommand> validator)
{
    public async Task<Result> HandleAsync(DeactivateUserCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Result.Failure(ResultError.Validation("User.Validation", validation.Errors[0].ErrorMessage));

        var user = await userRepository.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return Result.Failure(ResultError.NotFound("User.NotFound", "User not found."));

        if (user.IsActive && user.Role == UserRole.Admin && await userRepository.CountActiveAdminsAsync(ct) <= 1)
            return Result.Failure(ResultError.Conflict("User.LastAdmin", "Cannot deactivate the last active administrator."));

        var result = user.Deactivate();
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
