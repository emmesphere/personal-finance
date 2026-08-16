using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Finance.Accounts.CreateAccount;

public sealed class CreateAccountHandler(
    ILedgerRepository ledgerRepository,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IValidator<CreateAccountCommand> validator)
{
    public async Task<Result<CreateAccountResponse>> HandleAsync(CreateAccountCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<CreateAccountResponse>(
                ResultError.Validation("Account.Validation", validation.Errors[0].ErrorMessage));
        }

        var ledger = await ledgerRepository.GetByIdAsync(command.LedgerId, ct);
        if (ledger is null)
            return Result.Failure<CreateAccountResponse>(
                ResultError.NotFound("Ledger.NotFound", "Ledger not found."));

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure<CreateAccountResponse>(userIdResult.Error);

        var memberCheck = ledger.EnsureMember(userIdResult.Value);
        if (memberCheck.IsFailure)
            return Result.Failure<CreateAccountResponse>(memberCheck.Error);

        if (await accountRepository.ExistsByNameAsync(command.LedgerId, command.Name, ct))
        {
            return Result.Failure<CreateAccountResponse>(
                ResultError.Conflict("Account.Name.Taken", "An account with this name already exists in this ledger."));
        }

        DueDate? dueDate = null;
        if (command.DueDateDay.HasValue)
        {
            var dueDateResult = DueDate.Create(command.DueDateDay.Value);
            if (dueDateResult.IsFailure)
                return Result.Failure<CreateAccountResponse>(dueDateResult.Error);

            dueDate = dueDateResult.Value;
        }

        var accountResult = Account.Create(
            command.LedgerId,
            command.Name,
            command.Type,
            dueDate,
            categoryId: null,
            dateTimeProvider.UtcNow);

        if (accountResult.IsFailure)
            return Result.Failure<CreateAccountResponse>(accountResult.Error);

        var account = accountResult.Value;

        accountRepository.Add(account);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new CreateAccountResponse(account.Id));
    }
}
