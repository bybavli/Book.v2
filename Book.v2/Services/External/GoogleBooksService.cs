using System.Text.Json;

namespace Book.v2.Services.External;




public class GoogleBooksService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleBooksService> _logger;
    private const string BaseUrl = "https://www.googleapis.com/books/v1/volumes";

    public GoogleBooksService(HttpClient httpClient, ILogger<GoogleBooksService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }




    public async Task<GoogleBookResult?> SearchBookAsync(string title, string author)
    {
        try
        {
            var query = Uri.EscapeDataString($"intitle:{title}+inauthor:{author}");
            var url = $"{BaseUrl}?q={query}&langRestrict=tr&maxResults=3&printType=books";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google Books API returned {Status} for '{Title}'", response.StatusCode, title);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
                return null;

            var item = items[0];
            var volumeInfo = item.GetProperty("volumeInfo");

            var result = new GoogleBookResult
            {
                GoogleBooksId = item.GetProperty("id").GetString() ?? "",
                Title = volumeInfo.TryGetProperty("title", out var t) ? t.GetString() ?? title : title,
                Author = volumeInfo.TryGetProperty("authors", out var authors) && authors.GetArrayLength() > 0
                    ? authors[0].GetString() ?? author
                    : author,
                Description = volumeInfo.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                PageCount = volumeInfo.TryGetProperty("pageCount", out var pc) ? pc.GetInt32() : 0,
                Rating = volumeInfo.TryGetProperty("averageRating", out var rating) ? rating.GetDouble() : 0,
                Categories = volumeInfo.TryGetProperty("categories", out var cats)
                    ? cats.EnumerateArray().Select(c => c.GetString() ?? "").ToList()
                    : [],
            };

            if (volumeInfo.TryGetProperty("imageLinks", out var imageLinks))
            {
                if (imageLinks.TryGetProperty("thumbnail", out var thumb))
                {

                    var thumbUrl = thumb.GetString() ?? "";
                    thumbUrl = thumbUrl.Replace("http://", "https://");

                    thumbUrl = thumbUrl.Replace("zoom=1", "zoom=2");

                    thumbUrl = thumbUrl.Replace("&edge=curl", "");
                    result.ThumbnailUrl = thumbUrl;
                }
            }

            if (volumeInfo.TryGetProperty("industryIdentifiers", out var identifiers))
            {
                foreach (var id in identifiers.EnumerateArray())
                {
                    var type = id.TryGetProperty("type", out var idType) ? idType.GetString() : "";
                    if (type == "ISBN_13")
                    {
                        result.Isbn = id.GetProperty("identifier").GetString();
                        break;
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch book '{Title}' from Google Books", title);
            return null;
        }
    }



    public async Task<List<GoogleBookResult>> SearchByQueryAsync(string query, int maxResults = 20)
    {
        var results = new List<GoogleBookResult>();

        try
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"{BaseUrl}?q={encodedQuery}&langRestrict=tr&maxResults={Math.Min(maxResults, 40)}&printType=books&orderBy=relevance";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("items", out var items)) return results;

            foreach (var item in items.EnumerateArray())
            {
                try
                {
                    var volumeInfo = item.GetProperty("volumeInfo");

                    var result = new GoogleBookResult
                    {
                        GoogleBooksId = item.GetProperty("id").GetString() ?? "",
                        Title = volumeInfo.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                        Author = volumeInfo.TryGetProperty("authors", out var authors) && authors.GetArrayLength() > 0
                            ? authors[0].GetString() ?? "Bilinmeyen Yazar"
                            : "Bilinmeyen Yazar",
                        Description = volumeInfo.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        PageCount = volumeInfo.TryGetProperty("pageCount", out var pc) ? pc.GetInt32() : 0,
                        Rating = volumeInfo.TryGetProperty("averageRating", out var rating) ? rating.GetDouble() : 0,
                        Categories = volumeInfo.TryGetProperty("categories", out var cats)
                            ? cats.EnumerateArray().Select(c => c.GetString() ?? "").ToList()
                            : [],
                    };

                    if (volumeInfo.TryGetProperty("imageLinks", out var imageLinks))
                    {
                        if (imageLinks.TryGetProperty("thumbnail", out var thumb))
                        {
                            var thumbUrl = thumb.GetString() ?? "";
                            thumbUrl = thumbUrl.Replace("http://", "https://");
                            thumbUrl = thumbUrl.Replace("zoom=1", "zoom=2");
                            thumbUrl = thumbUrl.Replace("&edge=curl", "");
                            result.ThumbnailUrl = thumbUrl;
                        }
                    }

                    if (volumeInfo.TryGetProperty("industryIdentifiers", out var identifiers))
                    {
                        foreach (var id in identifiers.EnumerateArray())
                        {
                            var type = id.TryGetProperty("type", out var idType) ? idType.GetString() : "";
                            if (type == "ISBN_13")
                            {
                                result.Isbn = id.GetProperty("identifier").GetString();
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(result.Title) && !string.IsNullOrEmpty(result.ThumbnailUrl))
                    {
                        results.Add(result);
                    }
                }
                catch {  }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to search Google Books with query '{Query}'", query);
        }

        return results;
    }
}

public class GoogleBookResult
{
    public string GoogleBooksId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Isbn { get; set; }
    public int PageCount { get; set; }
    public double Rating { get; set; }
    public List<string> Categories { get; set; } = [];
}
