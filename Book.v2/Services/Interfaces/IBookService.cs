using Book.v2.Models.DTOs;

namespace Book.v2.Services.Interfaces;

public interface IBookService
{
    Task<BookDetailDto?> GetBookDetailAsync(Guid bookId);
    Task<List<BookDto>> GetBooksAsync(int page, int pageSize);
    Task<BookPageDto?> GetBookPageAsync(Guid bookId, int pageNumber);
    Task<List<BookPageDto>> GetBookPagesAsync(Guid bookId);
}
