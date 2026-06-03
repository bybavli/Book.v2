namespace Book.v2.Models.DTOs;

public record RecommendationDto(
    Guid BookId,
    string Title,
    string Author,
    string? CoverImageUrl,
    string Genre,
    double Rating,
    double RelevanceScore,
    string Reason);
