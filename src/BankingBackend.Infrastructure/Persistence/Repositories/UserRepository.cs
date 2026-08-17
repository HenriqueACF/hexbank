using BankingBackend.Core.Users;
using Microsoft.EntityFrameworkCore;

namespace BankingBackend.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) => _context = context;

    public async Task<User?> GetByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default) =>
        await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> ExistsByCpfAsync(
        Cpf cpf,
        CancellationToken cancellationToken = default) =>
        await _context.Users
            .AnyAsync(u => u.Cpf == cpf, cancellationToken);

    public void Add(User user) => _context.Users.Add(user);
}