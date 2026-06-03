using Book.v2.Data;
using Book.v2.Models.DTOs;
using Book.v2.Models.Entities;
using Book.v2.Repositories.Interfaces;
using Book.v2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Book.v2.Services.Implementations;

public class ReadingProgressService : IReadingProgressService
{
    private readonly IReadingProgressRepository _progressRepository;
    private readonly ContextDb _context;

    public ReadingProgressService(IReadingProgressRepository progressRepository, ContextDb context)
    {
        _progressRepository = progressRepository;
        _context = context;
    }

    public async Task<ReadingProgressDto?> GetProgressAsync(Guid userId, Guid bookId)
    {
        var progress = await _progressRepository.GetByUserAndBookAsync(userId, bookId);
        if (progress is null) return null;

        return new ReadingProgressDto(
            progress.CurrentPage,
            progress.ProgressPercentage,
            progress.LastReadAt);
    }

    public async Task<ReadingProgressDto> UpdateProgressAsync(Guid userId, Guid bookId, int currentPage, int totalPages)
    {
        var progress = await _context.ReadingProgresses
            .FirstOrDefaultAsync(rp => rp.UserId == userId && rp.BookId == bookId);

        if (progress is null)
        {
            progress = ReadingProgress.Create(userId, bookId);
            progress.UpdateProgress(currentPage, totalPages);
            await _context.ReadingProgresses.AddAsync(progress);
        }
        else
        {
            progress.UpdateProgress(currentPage, totalPages);
        }

        await _context.SaveChangesAsync();

        return new ReadingProgressDto(
            progress.CurrentPage,
            progress.ProgressPercentage,
            progress.LastReadAt);
    }
}
