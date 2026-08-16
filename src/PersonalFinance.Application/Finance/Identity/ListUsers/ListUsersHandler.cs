using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Application.Finance.Identity.ListUsers;

public sealed class ListUsersHandler(IUserRepository userRepository)
{
    public async Task<Result<IReadOnlyCollection<UserSummary>>> HandleAsync(ListUsersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var users = await userRepository.ListAllAsync(ct);

        IReadOnlyCollection<UserSummary> summaries = users
            .Select(u => new UserSummary(u.Id, u.FullName, u.Username, u.Email, u.PhoneNumber, u.Role.ToString(), u.IsActive, u.CreatedAt))
            .ToArray();

        return Result.Success(summaries);
    }
}
