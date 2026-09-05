using PersonalFinance.Application.Finance.Identity.RegisterUser;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Identity.Users;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Identity;

public sealed class RegisterUserHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeLedgerRepository _ledgerRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private RegisterUserHandler CreateHandler()
        => new(_userRepository, _ledgerRepository, _unitOfWork, _passwordHasher, _dateTimeProvider, new RegisterUserValidator());

    [Fact]
    public async Task HandleAsync_ShouldCreateUserAndPersonalLedger_WhenValid()
    {
        var command = new RegisterUserCommand("Jane Doe", "janedoe", "jane@example.com", null, "supersecret");

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Username.ShouldBe("janedoe");
        result.Value.LedgerId.ShouldNotBe(Guid.Empty);
        _unitOfWork.SaveChangesCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenUsernameAlreadyTaken()
    {
        var existing = User.Register(
            "Existing", "janedoe", "existing@example.com", null, "hash", _dateTimeProvider.UtcNow).Value;
        _userRepository.Seed(existing);

        var command = new RegisterUserCommand("Jane Doe", "janedoe", "jane@example.com", null, "supersecret");

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.Username.Taken");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenNeitherEmailNorPhoneProvided()
    {
        var command = new RegisterUserCommand("Jane Doe", "janedoe", null, null, "supersecret");

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        _unitOfWork.SaveChangesCallCount.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_ShouldHashPassword_BeforeStoringUser()
    {
        var command = new RegisterUserCommand("Jane Doe", "janedoe", "jane@example.com", null, "supersecret");

        await CreateHandler().HandleAsync(command, CancellationToken.None);

        var stored = await _userRepository.GetByUsernameAsync("janedoe", CancellationToken.None);
        stored.ShouldNotBeNull();
        stored.PasswordHash.ShouldBe("hashed:supersecret");
    }
}
