using HealthCheckApi.Data;
using HealthCheckApi.Entity;
using HealthCheckApi.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HealthCheckApi.Repository;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task DeleteUser(Guid id, CancellationToken ct)
    {
        var user = await _context.Users.FindAsync(id) ?? throw new InvalidOperationException("User is null");
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<UserEntity> CreateUserAsync(UserEntity user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    public async Task<UserEntity?> GetUserByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<UserEntity?> GetUserByEmail(string email, CancellationToken ct)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email),ct);
    }
}
