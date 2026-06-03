namespace Book.v2.Models.DTOs;

public record BookDto(
    Guid Id,
    string Title,
    string Author,
    string Genre,
    string? CoverImageUrl,
    double Rating,
    int TotalPages);
