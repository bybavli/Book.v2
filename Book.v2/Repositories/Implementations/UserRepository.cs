using Book.v2.Data;
using Book.v2.Models.Entities;
using Book.v2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Book.v2.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly ContextDb _context;

    public UserRepository(ContextDb context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByIdWithPreferencesAsync(Guid id)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
