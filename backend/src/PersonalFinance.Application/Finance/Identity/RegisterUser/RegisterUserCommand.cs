namespace PersonalFinance.Application.Finance.Identity.RegisterUser;

public sealed record RegisterUserCommand(
    string FullName,
    string Username,
    string? Email,
    string? PhoneNumber,
    string Password);
