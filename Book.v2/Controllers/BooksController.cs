using Book.v2.Models.DTOs;
using Book.v2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Book.v2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IRecommendationService _recommendationService;

    public BooksController(IBookService bookService, IRecommendationService recommendationService)
    {
        _bookService = bookService;
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 12;

        var books = await _bookService.GetBooksAsync(page, pageSize);
        return Ok(books);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDetailDto>> GetBook(Guid id)
    {
        var book = await _bookService.GetBookDetailAsync(id);
        if (book is null) return NotFound();

        return Ok(book);
    }

    [HttpGet("{id:guid}/pages/{pageNumber:int}")]
    public async Task<ActionResult<BookPageDto>> GetBookPage(Guid id, int pageNumber)
    {
        if (pageNumber < 1) return BadRequest("Page number must be at least 1.");

        var page = await _bookService.GetBookPageAsync(id, pageNumber);
        if (page is null) return NotFound();

        return Ok(page);
    }

    [HttpGet("{id:guid}/pages")]
    public async Task<ActionResult<List<BookPageDto>>> GetBookPages(Guid id)
    {
        var pages = await _bookService.GetBookPagesAsync(id);
        if (pages.Count == 0) return NotFound();

        return Ok(pages);
    }



    [HttpGet("{id:guid}/similar")]
    public async Task<ActionResult<List<RecommendationDto>>> GetSimilarBooks(
        Guid id,
        [FromQuery] int count = 10)
    {
        if (count < 1 || count > 50) count = 10;

        var similar = await _recommendationService.GetSimilarBooksAsync(id, count);
        return Ok(similar);
    }
}

