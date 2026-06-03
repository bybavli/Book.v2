using Book.v2.Models.DTOs;

namespace Book.v2.Services.Interfaces;

public interface IReadingListService
{
    Task<List<ReadingListDto>> GetUserReadingListAsync(Guid userId);
    Task<bool> AddToReadingListAsync(Guid userId, Guid bookId);
    Task RemoveFromReadingListAsync(Guid userId, Guid bookId);
}
