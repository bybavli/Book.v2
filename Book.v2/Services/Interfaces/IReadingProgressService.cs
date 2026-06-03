using Book.v2.Models.DTOs;

namespace Book.v2.Services.Interfaces;

public interface IReadingProgressService
{
    Task<ReadingProgressDto?> GetProgressAsync(Guid userId, Guid bookId);
    Task<ReadingProgressDto> UpdateProgressAsync(Guid userId, Guid bookId, int currentPage, int totalPages);
}
