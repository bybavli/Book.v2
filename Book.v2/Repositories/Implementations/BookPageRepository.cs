using Book.v2.Data;
using Book.v2.Models.Entities;
using Book.v2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Book.v2.Repositories.Implementations;

public class BookPageRepository : IBookPageRepository
{
    private readonly ContextDb _context;

    public BookPageRepository(ContextDb context)
    {
        _context = context;
    }

    public async Task<BookPage?> GetPageAsync(Guid bookId, int pageNumber)
    {
        return await _context.BookPages
            .AsNoTracking()
            .FirstOrDefaultAsync(bp => bp.BookId == bookId && bp.PageNumber == pageNumber);
    }

    public async Task<List<BookPage>> GetPageRangeAsync(Guid bookId, int startPage, int endPage)
    {
        return await _context.BookPages
            .AsNoTracking()
            .Where(bp => bp.BookId == bookId && bp.PageNumber >= startPage && bp.PageNumber <= endPage)
            .OrderBy(bp => bp.PageNumber)
            .ToListAsync();
    }

    public async Task<List<BookPage>> GetAllPagesAsync(Guid bookId)
    {
        return await _context.BookPages
            .AsNoTracking()
            .Where(bp => bp.BookId == bookId)
            .OrderBy(bp => bp.PageNumber)
            .ToListAsync();
    }
}
