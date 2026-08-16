using PersonalFinance.Application.Finance.Accounts.CreateAccount;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.Ledgers;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Accounts;

public sealed class CreateAccountHandlerTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly FakeLedgerRepository _ledgerRepository = new();
    private readonly FakeAccountRepository _accountRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));

    private CreateAccountHandler CreateHandler()
        => new(
            _ledgerRepository,
            _accountRepository,
            _unitOfWork,
            _dateTimeProvider,
            new FakeCurrentUserService(_ownerId),
            new CreateAccountValidator());

    private Ledger SeedLedger()
    {
        var ledger = Ledger.Create("My Ledger", UserId.From(_ownerId), _dateTimeProvider.UtcNow).Value;
        _ledgerRepository.Seed(ledger);
        return ledger;
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateAccount_WhenValid()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _unitOfWork.SaveChangesCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateAccount_WithDueDate_ForCreditCard()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "My Card", AccountType.CreditCard, 15);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenLedgerNotFound()
    {
        var command = new CreateAccountCommand(Guid.NewGuid(), "Wallet", AccountType.Wallet, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.NotFound");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenCallerIsNotLedgerMember()
    {
        var ledger = Ledger.Create("Other Ledger", UserId.From(Guid.NewGuid()), _dateTimeProvider.UtcNow).Value;
        _ledgerRepository.Seed(ledger);

        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.Member.Required");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenAccountNameAlreadyTaken()
    {
        var ledger = SeedLedger();
        _accountRepository.Seed(Account.Create(ledger.Id, "Wallet", AccountType.Wallet, null, null, _dateTimeProvider.UtcNow).Value);

        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.Name.Taken");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenTypeIsIncomeOrExpense()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Salary", AccountType.Income, null);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.Validation");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenDueDateProvided_ForNonCreditCardOrDebitType()
    {
        var ledger = SeedLedger();
        var command = new CreateAccountCommand(ledger.Id, "Wallet", AccountType.Wallet, 10);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.DueDate.NotAllowed");
    }
}
