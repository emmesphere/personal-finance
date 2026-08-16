namespace PersonalFinance.Application.Finance.Identity.RegisterUser;

public sealed record RegisterUserResponse(Guid UserId, Guid LedgerId, string Username);
