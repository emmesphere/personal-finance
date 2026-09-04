using PersonalFinance.Domain.Identity.Users;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<User?> GetByUsernameAsync(string username, CancellationToken ct);

    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct);

    Task<List<User>> ListAllAsync(CancellationToken ct);

    Task<int> CountActiveAdminsAsync(CancellationToken ct);

    Task<int> CountAllAsync(CancellationToken ct);

    void Add(User user);
}
