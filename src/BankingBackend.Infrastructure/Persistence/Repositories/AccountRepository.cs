using BankingBackend.Core.Accounts;
using Microsoft.EntityFrameworkCore;

namespace BankingBackend.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context) => _context = context;

    public async Task<Account?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public void Add(Account account) => _context.Accounts.Add(account);
}