namespace Book.v2.Models.DTOs;

public record ReadingListDto(
    Guid BookId,
    string Title,
    string Author,
    string? CoverImageUrl,
    string Genre,
    DateTime AddedAt,
    ReadingProgressDto? Progress);
