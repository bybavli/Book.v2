using Book.v2.Models.Entities;

namespace Book.v2.Repositories.Interfaces;

public interface IBookPageRepository
{
    Task<BookPage?> GetPageAsync(Guid bookId, int pageNumber);
    Task<List<BookPage>> GetPageRangeAsync(Guid bookId, int startPage, int endPage);
    Task<List<BookPage>> GetAllPagesAsync(Guid bookId);
}
