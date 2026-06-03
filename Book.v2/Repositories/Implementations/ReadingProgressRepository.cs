using Book.v2.Data;
using Book.v2.Models.Entities;
using Book.v2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Book.v2.Repositories.Implementations;

public class ReadingProgressRepository : IReadingProgressRepository
{
    private readonly ContextDb _context;

    public ReadingProgressRepository(ContextDb context)
    {
        _context = context;
    }

    public async Task<ReadingProgress?> GetByUserAndBookAsync(Guid userId, Guid bookId)
    {
        return await _context.ReadingProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(rp => rp.UserId == userId && rp.BookId == bookId);
    }

    public async Task<List<ReadingProgress>> GetAllByUserAsync(Guid userId)
    {
        return await _context.ReadingProgresses
            .AsNoTracking()
            .Where(rp => rp.UserId == userId)
            .ToListAsync();
    }

    public async Task UpsertAsync(ReadingProgress progress)
    {
        var existing = await _context.ReadingProgresses
            .FirstOrDefaultAsync(rp => rp.UserId == progress.UserId && rp.BookId == progress.BookId);

        if (existing is null)
        {
            await _context.ReadingProgresses.AddAsync(progress);
        }
        else
        {
            existing.UpdateProgress(progress.CurrentPage, progress.CurrentPage > 0 ? (int)(progress.CurrentPage / (progress.ProgressPercentage / 100.0)) : 1);
        }

        await _context.SaveChangesAsync();
    }
}
