using Microsoft.EntityFrameworkCore;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Identity.Users;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(PersonalFinanceDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
        => context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
        => context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct)
        => context.Users.AnyAsync(u => u.Username == username, ct);

    public Task<List<User>> ListAllAsync(CancellationToken ct)
        => context.Users.AsNoTracking().ToListAsync(ct);

    public Task<int> CountActiveAdminsAsync(CancellationToken ct)
        => context.Users.CountAsync(u => u.IsActive && u.Role == UserRole.Admin, ct);

    public Task<int> CountAllAsync(CancellationToken ct)
        => context.Users.CountAsync(ct);

    public void Add(User user) => context.Users.Add(user);
}
