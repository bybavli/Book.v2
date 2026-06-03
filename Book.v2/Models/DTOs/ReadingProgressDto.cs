namespace Book.v2.Models.DTOs;

public record ReadingProgressDto(
    int CurrentPage,
    double ProgressPercentage,
    DateTime LastReadAt);
