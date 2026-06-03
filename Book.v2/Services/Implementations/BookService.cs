using Book.v2.Models.DTOs;
using Book.v2.Repositories.Interfaces;
using Book.v2.Services.Interfaces;

namespace Book.v2.Services.Implementations;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookPageRepository _bookPageRepository;

    public BookService(IBookRepository bookRepository, IBookPageRepository bookPageRepository)
    {
        _bookRepository = bookRepository;
        _bookPageRepository = bookPageRepository;
    }

    public async Task<BookDetailDto?> GetBookDetailAsync(Guid bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null) return null;

        return new BookDetailDto(
            book.Id,
            book.Title,
            book.Author,
            book.Description,
            book.CoverImageUrl,
            book.Genre,
            book.Tags,
            book.TotalPages,
            book.Rating,
            book.PublishedDate);
    }

    public async Task<List<BookDto>> GetBooksAsync(int page, int pageSize)
    {
        var books = await _bookRepository.GetAllAsync(page, pageSize);

        return books.Select(b => new BookDto(
            b.Id,
            b.Title,
            b.Author,
            b.Genre,
            b.CoverImageUrl,
            b.Rating,
            b.TotalPages)).ToList();
    }

    public async Task<BookPageDto?> GetBookPageAsync(Guid bookId, int pageNumber)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null) return null;

        var page = await _bookPageRepository.GetPageAsync(bookId, pageNumber);
        if (page is null) return null;

        return new BookPageDto(page.PageNumber, page.Content, book.TotalPages);
    }

    public async Task<List<BookPageDto>> GetBookPagesAsync(Guid bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null) return [];

        var pages = await _bookPageRepository.GetAllPagesAsync(bookId);

        return pages.Select(p => new BookPageDto(p.PageNumber, p.Content, book.TotalPages)).ToList();
    }
}
