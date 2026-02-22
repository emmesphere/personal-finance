using FluentValidation;

using PersonalFinance.Application.Abstractions.Messaging;
using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;

public sealed class PostJournalEntryHandler(
    ILedgerRepository ledgerRepository,
    IAccountRepository accountRepository,
    IJournalEntryRepository journalEntryRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IValidator<PostJournalEntryCommand> validator
) : ICommandHandler<PostJournalEntryCommand, Result<PostJournalEntryResponse>>
{
    public async Task<Result<PostJournalEntryResponse>> HandleAsync(PostJournalEntryCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {            
            return Result.Failure<PostJournalEntryResponse>(
                ResultError.Validation("JournalEntry.Validation", validation.Errors[0].ErrorMessage));
        }

        var ledger = await ledgerRepository.GetByIdAsync(command.LedgerId, ct);
        if (ledger is null)
            return Result.Failure<PostJournalEntryResponse>(
                ResultError.NotFound("Ledger.NotFound", "Ledger not found."));

        var userIdResult = UserId.Create(command.CreatedByUserId);
        if (userIdResult.IsFailure)
            return Result.Failure<PostJournalEntryResponse>(userIdResult.Error);

        var entryResult = JournalEntry.Create(
            command.LedgerId,
            userIdResult.Value,
            command.Date,
            command.Description);

        if (entryResult.IsFailure)
            return Result.Failure<PostJournalEntryResponse>(entryResult.Error);

        var entry = entryResult.Value;

        foreach (var line in command.Lines)
        {
            var moneyResult = Money.Create(line.Amount);
            if (moneyResult.IsFailure)
                return Result.Failure<PostJournalEntryResponse>(moneyResult.Error);

            var addLineResult = entry.AddLine(line.AccountId, line.Type, moneyResult.Value);
            if (addLineResult.IsFailure)
                return Result.Failure<PostJournalEntryResponse>(addLineResult.Error);
        }

        var accountIds = command.Lines.Select(x => x.AccountId).Distinct().ToArray();
        var accounts = await accountRepository.GetByIdsAsync(command.LedgerId, accountIds, ct);

        if (accounts.Count != accountIds.Length)
            return Result.Failure<PostJournalEntryResponse>(
                ResultError.NotFound("Account.NotFound", "One or more accounts were not found in this ledger."));

        var postResult = entry.Post(dateTimeProvider.UtcNow, ledger, accounts);
        if (postResult.IsFailure)
            return Result.Failure<PostJournalEntryResponse>(postResult.Error);

        journalEntryRepository.Add(entry);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new PostJournalEntryResponse(entry.Id));
    }
}
