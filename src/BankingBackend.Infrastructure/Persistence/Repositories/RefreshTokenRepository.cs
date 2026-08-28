using BankingBackend.Core.Users;
using Microsoft.EntityFrameworkCore;

namespace BankingBackend.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context) => _context = context;

    public async Task<RefreshToken?> GetByValueAsync(Guid value, CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Value == value, cancellationToken);

    public void Add(RefreshToken refreshToken) => _context.RefreshTokens.Add(refreshToken);

}