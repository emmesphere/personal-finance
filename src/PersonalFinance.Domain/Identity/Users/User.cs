using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Domain.Identity.Users;

public sealed class User : AggregateRoot
{
    public string FullName { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { }

    private User(
        string fullName,
        string username,
        string? email,
        string? phoneNumber,
        string passwordHash,
        UserRole role,
        DateTime createdAt)
    {
        FullName = fullName;
        Username = username;
        Email = email;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = createdAt;
        IsActive = true;
    }

    public static Result<User> Register(
        string fullName,
        string username,
        string? email,
        string? phoneNumber,
        string passwordHash,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure<User>(ResultError.Validation("User.FullName.Empty", "Full name is required."));

        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<User>(ResultError.Validation("User.Username.Empty", "Username is required."));

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<User>(ResultError.Validation("User.PasswordHash.Empty", "Password is required."));

        var hasEmail = !string.IsNullOrWhiteSpace(email);
        var hasPhone = !string.IsNullOrWhiteSpace(phoneNumber);

        if (!hasEmail && !hasPhone)
            return Result.Failure<User>(ResultError.Validation("User.Contact.Required", "Either email or phone number is required."));

        return Result.Success(new User(
            fullName.Trim(),
            username.Trim(),
            hasEmail ? email!.Trim() : null,
            hasPhone ? phoneNumber!.Trim() : null,
            passwordHash,
            UserRole.User,
            createdAt));
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }

    public Result Reactivate()
    {
        IsActive = true;
        return Result.Success();
    }

    public Result PromoteToAdmin()
    {
        Role = UserRole.Admin;
        return Result.Success();
    }

    public Result DemoteToUser()
    {
        Role = UserRole.User;
        return Result.Success();
    }
}
