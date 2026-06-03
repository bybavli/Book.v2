using Book.v2.Data;
using Book.v2.Models.Entities;
using Book.v2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Book.v2.Repositories.Implementations;

public class ReadingListRepository : IReadingListRepository
{
    private readonly ContextDb _context;

    public ReadingListRepository(ContextDb context)
    {
        _context = context;
    }

    public async Task<List<ReadingListEntry>> GetByUserIdAsync(Guid userId)
    {
        return await _context.ReadingListEntries
            .AsNoTracking()
            .Include(r => r.Book)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.AddedAt)
            .ToListAsync();
    }

    public async Task AddEntryAsync(ReadingListEntry entry)
    {
        await _context.ReadingListEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveEntryAsync(Guid userId, Guid bookId)
    {
        var entry = await _context.ReadingListEntries
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);

        if (entry is not null)
        {
            _context.ReadingListEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid bookId)
    {
        return await _context.ReadingListEntries
            .AsNoTracking()
            .AnyAsync(r => r.UserId == userId && r.BookId == bookId);
    }
}
