using Book.v2.Models.Entities;

namespace Book.v2.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByIdWithPreferencesAsync(Guid id);
}
