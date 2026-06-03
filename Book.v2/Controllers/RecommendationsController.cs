using Book.v2.Models.DTOs;
using Book.v2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Book.v2.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RecommendationDto>>> GetRecommendations(
        Guid userId,
        [FromQuery] int count = 10)
    {
        if (count < 1 || count > 50) count = 10;

        var recommendations = await _recommendationService.GetRecommendationsAsync(userId, count);
        return Ok(recommendations);
    }
}
