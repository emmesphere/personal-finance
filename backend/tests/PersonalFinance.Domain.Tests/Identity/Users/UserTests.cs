using PersonalFinance.Domain.Identity.Users;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Identity.Users;

public sealed class UserTests
{
    [Fact]
    public void Register_ShouldSucceed_WhenOnlyEmailProvided()
    {
        var result = User.Register("Jane Doe", "janedoe", "jane@example.com", null, "hash", DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe("jane@example.com");
        result.Value.PhoneNumber.ShouldBeNull();
    }

    [Fact]
    public void Register_ShouldSucceed_WhenOnlyPhoneNumberProvided()
    {
        var result = User.Register("Jane Doe", "janedoe", null, "+15551234567", "hash", DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        result.Value.PhoneNumber.ShouldBe("+15551234567");
        result.Value.Email.ShouldBeNull();
    }

    [Fact]
    public void Register_ShouldFail_WhenNeitherEmailNorPhoneNumberProvided()
    {
        var result = User.Register("Jane Doe", "janedoe", null, null, "hash", DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.Contact.Required");
    }

    [Fact]
    public void Register_ShouldDefaultToUserRole()
    {
        var result = User.Register("Jane Doe", "janedoe", "jane@example.com", null, "hash", DateTime.UtcNow);

        result.Value.Role.ShouldBe(UserRole.User);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_ShouldFail_WhenFullNameIsBlank(string fullName)
    {
        var result = User.Register(fullName, "janedoe", "jane@example.com", null, "hash", DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.FullName.Empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_ShouldFail_WhenUsernameIsBlank(string username)
    {
        var result = User.Register("Jane Doe", username, "jane@example.com", null, "hash", DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("User.Username.Empty");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var user = User.Register("Jane Doe", "janedoe", "jane@example.com", null, "hash", DateTime.UtcNow).Value;

        user.Deactivate();

        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void PromoteToAdmin_ShouldSetRoleToAdmin()
    {
        var user = User.Register("Jane Doe", "janedoe", "jane@example.com", null, "hash", DateTime.UtcNow).Value;

        user.PromoteToAdmin();

        user.Role.ShouldBe(UserRole.Admin);
    }
}
