namespace Book.v2.Models.DTOs;

public record BookDetailDto(
    Guid Id,
    string Title,
    string Author,
    string? Description,
    string? CoverImageUrl,
    string Genre,
    string? Tags,
    int TotalPages,
    double Rating,
    DateTime PublishedDate);
