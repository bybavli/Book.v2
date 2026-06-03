using Book.v2.Models.DTOs;

namespace Book.v2.Services.Interfaces;

public interface IRecommendationService
{
    Task<List<RecommendationDto>> GetRecommendationsAsync(Guid userId, int count = 10);
    Task<List<RecommendationDto>> GetSimilarBooksAsync(Guid bookId, int count = 10);
}
