using Book.v2.Data;
using Book.v2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Book.v2.Repositories.Implementations;

public class BookRepository : IBookRepository
{
    private readonly ContextDb _context;

    public BookRepository(ContextDb context)
    {
        _context = context;
    }

    public async Task<Models.Entities.Book?> GetByIdAsync(Guid id)
    {
        return await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Models.Entities.Book>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Books
            .AsNoTracking()
            .OrderByDescending(b => b.PublishedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Models.Entities.Book>> SearchByGenreAsync(string genre)
    {
        return await _context.Books
            .AsNoTracking()
            .Where(b => b.Genre == genre)
            .OrderByDescending(b => b.Rating)
            .ToListAsync();
    }

    public async Task<List<Models.Entities.Book>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        return await _context.Books
            .AsNoTracking()
            .Where(b => idList.Contains(b.Id))
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Books.CountAsync();
    }
}
