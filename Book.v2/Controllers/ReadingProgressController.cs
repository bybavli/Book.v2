using Book.v2.Models.DTOs;
using Book.v2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Book.v2.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/progress")]
public class ReadingProgressController : ControllerBase
{
    private readonly IReadingProgressService _progressService;

    public ReadingProgressController(IReadingProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet("{bookId:guid}")]
    public async Task<ActionResult<ReadingProgressDto>> GetProgress(Guid userId, Guid bookId)
    {
        var progress = await _progressService.GetProgressAsync(userId, bookId);
        if (progress is null) return NotFound();

        return Ok(progress);
    }

    [HttpPut("{bookId:guid}")]
    public async Task<ActionResult<ReadingProgressDto>> UpdateProgress(
        Guid userId,
        Guid bookId,
        [FromBody] UpdateProgressRequest request)
    {
        try
        {
            var progress = await _progressService.UpdateProgressAsync(userId, bookId, request.CurrentPage, request.TotalPages);
            return Ok(progress);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
