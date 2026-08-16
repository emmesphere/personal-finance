using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Identity.Users;

namespace PersonalFinance.Application.Tests.Fakes;

public sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    public void Seed(User user) => _users.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
        => Task.FromResult(_users.FirstOrDefault(u => u.Username == username));

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct)
        => Task.FromResult(_users.Exists(u => u.Username == username));

    public Task<List<User>> ListAllAsync(CancellationToken ct) => Task.FromResult(_users.ToList());

    public Task<int> CountActiveAdminsAsync(CancellationToken ct)
        => Task.FromResult(_users.Count(u => u.IsActive && u.Role == UserRole.Admin));

    public Task<int> CountAllAsync(CancellationToken ct) => Task.FromResult(_users.Count);

    public void Add(User user) => _users.Add(user);
}
