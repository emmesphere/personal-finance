using PersonalFinance.Application.Finance.Identity.DemoteUser;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Identity.Users;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Identity;

public sealed class DemoteUserHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private DemoteUserHandler CreateHandler() => new(_userRepository, _unitOfWork, new DemoteUserValidator());

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenDemotingTheLastActiveAdmin()
    {
        var admin = User.Register("Admin", "admin", "admin@example.com", null, "hash", DateTime.UtcNow).Value;
        admin.PromoteToAdmin();
        _userRepository.Seed(admin);

        var result = await CreateHandler().HandleAsync(new DemoteUserCommand(admin.Id), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.LastAdmin");
        admin.Role.ShouldBe(UserRole.Admin);
    }

    [Fact]
    public async Task HandleAsync_ShouldSucceed_WhenAnotherActiveAdminExists()
    {
        var admin1 = User.Register("Admin One", "admin1", "admin1@example.com", null, "hash", DateTime.UtcNow).Value;
        admin1.PromoteToAdmin();
        var admin2 = User.Register("Admin Two", "admin2", "admin2@example.com", null, "hash", DateTime.UtcNow).Value;
        admin2.PromoteToAdmin();
        _userRepository.Seed(admin1);
        _userRepository.Seed(admin2);

        var result = await CreateHandler().HandleAsync(new DemoteUserCommand(admin1.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        admin1.Role.ShouldBe(UserRole.User);
    }
}
