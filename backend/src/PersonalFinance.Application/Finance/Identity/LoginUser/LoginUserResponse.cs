namespace PersonalFinance.Application.Finance.Identity.LoginUser;

public sealed record LoginUserResponse(string AccessToken, Guid UserId, string Username, string Role);
