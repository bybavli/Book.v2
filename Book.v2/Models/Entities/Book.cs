namespace Book.v2.Models.Entities;

public class Book
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public string Genre { get; private set; } = string.Empty;
    public string? Tags { get; private set; }
    public int TotalPages { get; private set; }
    public string? ContentFileUrl { get; private set; }
    public DateTime PublishedDate { get; private set; }
    public double Rating { get; private set; }

    private readonly List<ReadingListEntry> _inReadingLists = new();
    public IReadOnlyCollection<ReadingListEntry> InReadingLists => _inReadingLists.AsReadOnly();

    private readonly List<BookPage> _pages = new();
    public IReadOnlyCollection<BookPage> Pages => _pages.AsReadOnly();

    private Book() { }

    public static Book Create(
        string title,
        string author,
        string genre,
        int totalPages,
        string? description = null,
        string? coverImageUrl = null,
        string? tags = null,
        double rating = 0.0)
    {
        return new Book
        {
            Id = Guid.NewGuid(),
            Title = title,
            Author = author,
            Genre = genre,
            TotalPages = totalPages,
            Description = description,
            CoverImageUrl = coverImageUrl,
            Tags = tags,
            Rating = rating,
            PublishedDate = DateTime.UtcNow
        };
    }

    public void AddPage(int pageNumber, string content)
    {
        _pages.Add(BookPage.Create(Id, pageNumber, content));
    }

    public string[] GetTagsArray() =>
        Tags?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
}
