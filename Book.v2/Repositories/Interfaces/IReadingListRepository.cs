using Book.v2.Models.Entities;

namespace Book.v2.Repositories.Interfaces;

public interface IReadingListRepository
{
    Task<List<ReadingListEntry>> GetByUserIdAsync(Guid userId);
    Task AddEntryAsync(ReadingListEntry entry);
    Task RemoveEntryAsync(Guid userId, Guid bookId);
    Task<bool> ExistsAsync(Guid userId, Guid bookId);
}
