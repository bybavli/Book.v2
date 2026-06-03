using Book.v2.Models.DTOs;
using Book.v2.Repositories.Interfaces;
using Book.v2.Services.Interfaces;

namespace Book.v2.Services.Implementations;

public class ContentBasedRecommendationService : IRecommendationService
{
    private readonly IBookRepository _bookRepository;
    private readonly IReadingListRepository _readingListRepository;
    private readonly IUserRepository _userRepository;

    private const double GenreExactMatchWeight = 0.40;
    private const double TagOverlapWeight = 0.30;
    private const double AuthorMatchWeight = 0.15;
    private const double RatingWeight = 0.15;

    public ContentBasedRecommendationService(
        IBookRepository bookRepository,
        IReadingListRepository readingListRepository,
        IUserRepository userRepository)
    {
        _bookRepository = bookRepository;
        _readingListRepository = readingListRepository;
        _userRepository = userRepository;
    }

    public async Task<List<RecommendationDto>> GetRecommendationsAsync(Guid userId, int count = 10)
    {

        var user = await _userRepository.GetByIdWithPreferencesAsync(userId);
        if (user is null) return [];

        var readingListEntries = await _readingListRepository.GetByUserIdAsync(userId);
        var readingListBookIds = readingListEntries.Select(e => e.BookId).ToHashSet();

        var readingListBooks = await _bookRepository.GetByIdsAsync(readingListBookIds);

        var userGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var userTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var userAuthors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var book in readingListBooks)
        {
            userGenres.Add(book.Genre);
            userAuthors.Add(book.Author);

            foreach (var tag in book.GetTagsArray())
            {
                userTags.Add(tag);
            }
        }

        if (user.Preference is not null)
        {
            foreach (var genre in user.Preference.GetGenresArray())
                userGenres.Add(genre);

            foreach (var tag in user.Preference.GetTagsArray())
                userTags.Add(tag);
        }

        var totalBooks = await _bookRepository.CountAsync();
        var allBooks = await _bookRepository.GetAllAsync(1, totalBooks);
        var candidateBooks = allBooks.Where(b => !readingListBookIds.Contains(b.Id)).ToList();

        var scoredCandidates = new List<(Models.Entities.Book Book, double Score, string Reason)>();

        foreach (var candidate in candidateBooks)
        {
            var reasons = new List<string>();

            double genreScore = 0.0;
            if (userGenres.Contains(candidate.Genre))
            {
                genreScore = 1.0;
                reasons.Add($"Favori türünüz: {candidate.Genre}");
            }

            double tagScore = 0.0;
            var candidateTags = candidate.GetTagsArray().ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (userTags.Count > 0 && candidateTags.Count > 0)
            {
                var intersection = userTags.Intersect(candidateTags, StringComparer.OrdinalIgnoreCase).Count();
                var union = userTags.Union(candidateTags, StringComparer.OrdinalIgnoreCase).Count();
                tagScore = union > 0 ? (double)intersection / union : 0.0;

                if (intersection > 0)
                {
                    var matchedTags = userTags.Intersect(candidateTags, StringComparer.OrdinalIgnoreCase);
                    reasons.Add($"Ortak etiketler: {string.Join(", ", matchedTags)}");
                }
            }

            double authorScore = 0.0;
            if (userAuthors.Contains(candidate.Author))
            {
                authorScore = 1.0;
                reasons.Add($"Okuduğunuz yazar: {candidate.Author}");
            }

            double ratingScore = Math.Min(candidate.Rating / 5.0, 1.0);

            double totalScore = (genreScore * GenreExactMatchWeight)
                              + (tagScore * TagOverlapWeight)
                              + (authorScore * AuthorMatchWeight)
                              + (ratingScore * RatingWeight);

            totalScore = Math.Round(totalScore, 4);

            if (totalScore > 0)
            {
                string reason = reasons.Count > 0
                    ? string.Join(" • ", reasons)
                    : "Yüksek puanlı kitap";

                scoredCandidates.Add((candidate, totalScore, reason));
            }
        }

        return scoredCandidates
            .OrderByDescending(sc => sc.Score)
            .ThenByDescending(sc => sc.Book.Rating)
            .Take(count)
            .Select(sc => new RecommendationDto(
                sc.Book.Id,
                sc.Book.Title,
                sc.Book.Author,
                sc.Book.CoverImageUrl,
                sc.Book.Genre,
                sc.Book.Rating,
                sc.Score,
                sc.Reason))
            .ToList();
    }



    public async Task<List<RecommendationDto>> GetSimilarBooksAsync(Guid bookId, int count = 10)
    {
        var sourceBook = await _bookRepository.GetByIdAsync(bookId);
        if (sourceBook is null) return [];

        var sourceTags = sourceBook.GetTagsArray().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sourceGenre = sourceBook.Genre;
        var sourceAuthor = sourceBook.Author;

        var totalBooks = await _bookRepository.CountAsync();
        var allBooks = await _bookRepository.GetAllAsync(1, totalBooks);
        var candidates = allBooks.Where(b => b.Id != bookId).ToList();

        var scoredCandidates = new List<(Models.Entities.Book Book, double Score, string Reason)>();

        foreach (var candidate in candidates)
        {
            var reasons = new List<string>();

            double genreScore = 0.0;
            if (string.Equals(candidate.Genre, sourceGenre, StringComparison.OrdinalIgnoreCase))
            {
                genreScore = 1.0;
                reasons.Add($"Aynı tür: {candidate.Genre}");
            }

            double tagScore = 0.0;
            var candidateTags = candidate.GetTagsArray().ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (sourceTags.Count > 0 && candidateTags.Count > 0)
            {
                var intersection = sourceTags.Intersect(candidateTags, StringComparer.OrdinalIgnoreCase).Count();
                var union = sourceTags.Union(candidateTags, StringComparer.OrdinalIgnoreCase).Count();
                tagScore = union > 0 ? (double)intersection / union : 0.0;

                if (intersection > 0)
                {
                    var matchedTags = sourceTags.Intersect(candidateTags, StringComparer.OrdinalIgnoreCase);
                    reasons.Add($"Ortak temalar: {string.Join(", ", matchedTags)}");
                }
            }

            double authorScore = 0.0;
            if (string.Equals(candidate.Author, sourceAuthor, StringComparison.OrdinalIgnoreCase))
            {
                authorScore = 1.0;
                reasons.Add($"Aynı yazar: {candidate.Author}");
            }

            double ratingScore = Math.Min(candidate.Rating / 5.0, 1.0);

            double totalScore = (genreScore * 0.40) + (tagScore * 0.35) + (authorScore * 0.15) + (ratingScore * 0.10);
            totalScore = Math.Round(totalScore, 4);

            if (totalScore > 0.05) // Minimum threshold
            {
                string reason = reasons.Count > 0
                    ? string.Join(" • ", reasons)
                    : "Yüksek puanlı kitap";

                scoredCandidates.Add((candidate, totalScore, reason));
            }
        }

        return scoredCandidates
            .OrderByDescending(sc => sc.Score)
            .ThenByDescending(sc => sc.Book.Rating)
            .Take(count)
            .Select(sc => new RecommendationDto(
                sc.Book.Id,
                sc.Book.Title,
                sc.Book.Author,
                sc.Book.CoverImageUrl,
                sc.Book.Genre,
                sc.Book.Rating,
                sc.Score,
                sc.Reason))
            .ToList();
    }
}
