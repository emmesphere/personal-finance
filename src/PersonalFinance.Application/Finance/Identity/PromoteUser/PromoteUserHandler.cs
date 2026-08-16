using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Application.Finance.Identity.PromoteUser;

public sealed class PromoteUserHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IValidator<PromoteUserCommand> validator)
{
    public async Task<Result> HandleAsync(PromoteUserCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Result.Failure(ResultError.Validation("User.Validation", validation.Errors[0].ErrorMessage));

        var user = await userRepository.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return Result.Failure(ResultError.NotFound("User.NotFound", "User not found."));

        var result = user.PromoteToAdmin();
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
