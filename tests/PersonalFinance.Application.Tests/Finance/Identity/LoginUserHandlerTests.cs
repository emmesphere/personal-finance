using PersonalFinance.Application.Finance.Identity.LoginUser;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Identity.Users;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Identity;

public sealed class LoginUserHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeJwtTokenGenerator _jwtTokenGenerator = new();

    private LoginUserHandler CreateHandler()
        => new(_userRepository, _passwordHasher, _jwtTokenGenerator, new LoginUserValidator());

    private void SeedUser(string username, string password, bool isActive = true)
    {
        var user = User.Register("Jane Doe", username, "jane@example.com", null, _passwordHasher.Hash(password), DateTime.UtcNow).Value;
        if (!isActive)
            user.Deactivate();

        _userRepository.Seed(user);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        SeedUser("janedoe", "supersecret");

        var result = await CreateHandler().HandleAsync(new LoginUserCommand("janedoe", "supersecret"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("token-for-janedoe");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenPasswordIsWrong()
    {
        SeedUser("janedoe", "supersecret");

        var result = await CreateHandler().HandleAsync(new LoginUserCommand("janedoe", "wrongpassword"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.Credentials.Invalid");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenUserDoesNotExist()
    {
        var result = await CreateHandler().HandleAsync(new LoginUserCommand("nobody", "supersecret"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.Credentials.Invalid");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenUserIsDeactivated()
    {
        SeedUser("janedoe", "supersecret", isActive: false);

        var result = await CreateHandler().HandleAsync(new LoginUserCommand("janedoe", "supersecret"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.Credentials.Invalid");
    }
}
