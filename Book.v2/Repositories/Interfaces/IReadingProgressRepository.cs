using Book.v2.Models.Entities;

namespace Book.v2.Repositories.Interfaces;

public interface IReadingProgressRepository
{
    Task<ReadingProgress?> GetByUserAndBookAsync(Guid userId, Guid bookId);
    Task<List<ReadingProgress>> GetAllByUserAsync(Guid userId);
    Task UpsertAsync(ReadingProgress progress);
}
