using Book.v2.Models.DTOs;
using Book.v2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Book.v2.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/reading-list")]
public class ReadingListController : ControllerBase
{
    private readonly IReadingListService _readingListService;

    public ReadingListController(IReadingListService readingListService)
    {
        _readingListService = readingListService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReadingListDto>>> GetReadingList(Guid userId)
    {
        var list = await _readingListService.GetUserReadingListAsync(userId);
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult> AddToReadingList(Guid userId, [FromBody] AddToReadingListRequest request)
    {
        var success = await _readingListService.AddToReadingListAsync(userId, request.BookId);
        if (!success) return Conflict("Book already in reading list or does not exist.");

        return Created();
    }

    [HttpDelete("{bookId:guid}")]
    public async Task<ActionResult> RemoveFromReadingList(Guid userId, Guid bookId)
    {
        await _readingListService.RemoveFromReadingListAsync(userId, bookId);
        return NoContent();
    }
}
