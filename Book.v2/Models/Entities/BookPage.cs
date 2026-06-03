namespace Book.v2.Models.Entities;

public class BookPage
{
    public Guid Id { get; private set; }
    public Guid BookId { get; private set; }
    public int PageNumber { get; private set; }
    public string Content { get; private set; } = string.Empty;

    public Book Book { get; private set; } = null!;

    private BookPage() { }

    public static BookPage Create(Guid bookId, int pageNumber, string content)
    {
        return new BookPage
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            PageNumber = pageNumber,
            Content = content
        };
    }
}
