namespace PersonalFinance.WebApi.Contracts;

internal sealed record RegisterUserRequest(
    string FullName,
    string Username,
    string? Email,
    string? PhoneNumber,
    string Password);
