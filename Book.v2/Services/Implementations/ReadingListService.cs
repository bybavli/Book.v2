using Book.v2.Models.DTOs;
using Book.v2.Models.Entities;
using Book.v2.Repositories.Interfaces;
using Book.v2.Services.Interfaces;

namespace Book.v2.Services.Implementations;

public class ReadingListService : IReadingListService
{
    private readonly IReadingListRepository _readingListRepository;
    private readonly IReadingProgressRepository _progressRepository;
    private readonly IBookRepository _bookRepository;

    public ReadingListService(
        IReadingListRepository readingListRepository,
        IReadingProgressRepository progressRepository,
        IBookRepository bookRepository)
    {
        _readingListRepository = readingListRepository;
        _progressRepository = progressRepository;
        _bookRepository = bookRepository;
    }

    public async Task<List<ReadingListDto>> GetUserReadingListAsync(Guid userId)
    {
        var entries = await _readingListRepository.GetByUserIdAsync(userId);
        var progresses = await _progressRepository.GetAllByUserAsync(userId);
        var progressLookup = progresses.ToDictionary(p => p.BookId);

        return entries.Select(entry =>
        {
            ReadingProgressDto? progressDto = null;
            if (progressLookup.TryGetValue(entry.BookId, out var progress))
            {
                progressDto = new ReadingProgressDto(
                    progress.CurrentPage,
                    progress.ProgressPercentage,
                    progress.LastReadAt);
            }

            return new ReadingListDto(
                entry.BookId,
                entry.Book.Title,
                entry.Book.Author,
                entry.Book.CoverImageUrl,
                entry.Book.Genre,
                entry.AddedAt,
                progressDto);
        }).ToList();
    }

    public async Task<bool> AddToReadingListAsync(Guid userId, Guid bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null) return false;

        var exists = await _readingListRepository.ExistsAsync(userId, bookId);
        if (exists) return false;

        var entry = ReadingListEntry.Create(userId, bookId);
        await _readingListRepository.AddEntryAsync(entry);
        return true;
    }

    public async Task RemoveFromReadingListAsync(Guid userId, Guid bookId)
    {
        await _readingListRepository.RemoveEntryAsync(userId, bookId);
    }
}
