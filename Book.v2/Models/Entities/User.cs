namespace Book.v2.Models.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private readonly List<ReadingListEntry> _readingList = new();
    public IReadOnlyCollection<ReadingListEntry> ReadingList => _readingList.AsReadOnly();

    private readonly List<ReadingProgress> _readingProgresses = new();
    public IReadOnlyCollection<ReadingProgress> ReadingProgresses => _readingProgresses.AsReadOnly();

    public UserPreference? Preference { get; private set; }

    private User() { }

    public static User Create(string username, string email)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateWithId(Guid id, string username, string email)
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string username, string email)
    {
        Username = username;
        Email = email;
    }
}
