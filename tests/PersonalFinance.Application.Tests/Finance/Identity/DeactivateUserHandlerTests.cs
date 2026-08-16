using PersonalFinance.Application.Finance.Identity.DeactivateUser;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Identity.Users;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Identity;

public sealed class DeactivateUserHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private DeactivateUserHandler CreateHandler() => new(_userRepository, _unitOfWork, new DeactivateUserValidator());

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenDeactivatingTheLastActiveAdmin()
    {
        var admin = User.Register("Admin", "admin", "admin@example.com", null, "hash", DateTime.UtcNow).Value;
        admin.PromoteToAdmin();
        _userRepository.Seed(admin);

        var result = await CreateHandler().HandleAsync(new DeactivateUserCommand(admin.Id), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.LastAdmin");
        admin.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldSucceed_WhenDeactivatingAdmin_AndAnotherActiveAdminExists()
    {
        var admin1 = User.Register("Admin One", "admin1", "admin1@example.com", null, "hash", DateTime.UtcNow).Value;
        admin1.PromoteToAdmin();
        var admin2 = User.Register("Admin Two", "admin2", "admin2@example.com", null, "hash", DateTime.UtcNow).Value;
        admin2.PromoteToAdmin();
        _userRepository.Seed(admin1);
        _userRepository.Seed(admin2);

        var result = await CreateHandler().HandleAsync(new DeactivateUserCommand(admin1.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        admin1.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldSucceed_WhenDeactivatingRegularUser()
    {
        var user = User.Register("Jane", "jane", "jane@example.com", null, "hash", DateTime.UtcNow).Value;
        _userRepository.Seed(user);

        var result = await CreateHandler().HandleAsync(new DeactivateUserCommand(user.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenUserNotFound()
    {
        var result = await CreateHandler().HandleAsync(new DeactivateUserCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.NotFound");
    }
}
