using System.Text.Json;

namespace Book.v2.Models.Entities;

public class UserPreference
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string FavoriteGenres { get; private set; } = "[]";
    public string FavoriteTags { get; private set; } = "[]";

    public User User { get; private set; } = null!;

    private UserPreference() { }

    public static UserPreference Create(Guid userId, string[] genres, string[] tags)
    {
        return new UserPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FavoriteGenres = JsonSerializer.Serialize(genres),
            FavoriteTags = JsonSerializer.Serialize(tags)
        };
    }

    public string[] GetGenresArray() =>
        JsonSerializer.Deserialize<string[]>(FavoriteGenres) ?? [];

    public string[] GetTagsArray() =>
        JsonSerializer.Deserialize<string[]>(FavoriteTags) ?? [];

    public void UpdatePreferences(string[] genres, string[] tags)
    {
        FavoriteGenres = JsonSerializer.Serialize(genres);
        FavoriteTags = JsonSerializer.Serialize(tags);
    }
}
