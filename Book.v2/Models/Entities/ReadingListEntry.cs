namespace Book.v2.Models.Entities;

public class ReadingListEntry
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid BookId { get; private set; }
    public DateTime AddedAt { get; private set; }

    public User User { get; private set; } = null!;
    public Book Book { get; private set; } = null!;

    private ReadingListEntry() { }

    public static ReadingListEntry Create(Guid userId, Guid bookId)
    {
        return new ReadingListEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BookId = bookId,
            AddedAt = DateTime.UtcNow
        };
    }
}
