using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.Ledgers;
using PersonalFinance.Domain.Identity.Users;

namespace PersonalFinance.Application.Finance.Identity.RegisterUser;

public sealed class RegisterUserHandler(
    IUserRepository userRepository,
    ILedgerRepository ledgerRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider,
    IValidator<RegisterUserCommand> validator)
{
    public async Task<Result<RegisterUserResponse>> HandleAsync(RegisterUserCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<RegisterUserResponse>(
                ResultError.Validation("User.Validation", validation.Errors[0].ErrorMessage));
        }

        if (await userRepository.ExistsByUsernameAsync(command.Username, ct))
        {
            return Result.Failure<RegisterUserResponse>(
                ResultError.Conflict("User.Username.Taken", "Username is already taken."));
        }

        var passwordHash = passwordHasher.Hash(command.Password);

        var userResult = User.Register(
            command.FullName,
            command.Username,
            command.Email,
            command.PhoneNumber,
            passwordHash,
            dateTimeProvider.UtcNow);

        if (userResult.IsFailure)
            return Result.Failure<RegisterUserResponse>(userResult.Error);

        var user = userResult.Value;

        var ledgerResult = Ledger.Create(
            $"{user.FullName}'s Ledger",
            UserId.From(user.Id),
            dateTimeProvider.UtcNow);

        if (ledgerResult.IsFailure)
            return Result.Failure<RegisterUserResponse>(ledgerResult.Error);

        var ledger = ledgerResult.Value;

        userRepository.Add(user);
        ledgerRepository.Add(ledger);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new RegisterUserResponse(user.Id, ledger.Id, user.Username));
    }
}
