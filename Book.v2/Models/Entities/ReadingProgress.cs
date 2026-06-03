namespace Book.v2.Models.Entities;

public class ReadingProgress
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid BookId { get; private set; }
    public int CurrentPage { get; private set; }
    public double ProgressPercentage { get; private set; }
    public DateTime LastReadAt { get; private set; }

    public User User { get; private set; } = null!;
    public Book Book { get; private set; } = null!;

    private ReadingProgress() { }

    public static ReadingProgress Create(Guid userId, Guid bookId)
    {
        return new ReadingProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BookId = bookId,
            CurrentPage = 0,
            ProgressPercentage = 0,
            LastReadAt = DateTime.UtcNow
        };
    }

    public void UpdateProgress(int currentPage, int totalPages)
    {
        if (currentPage < 0) throw new ArgumentException("Page cannot be negative");
        if (totalPages <= 0) throw new ArgumentException("Total pages must be positive");

        CurrentPage = Math.Min(currentPage, totalPages);
        ProgressPercentage = Math.Round((double)CurrentPage / totalPages * 100, 2);
        LastReadAt = DateTime.UtcNow;
    }
}
