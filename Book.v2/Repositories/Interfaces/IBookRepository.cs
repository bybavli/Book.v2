using Book.v2.Models.Entities;

namespace Book.v2.Repositories.Interfaces;

public interface IBookRepository
{
    Task<Models.Entities.Book?> GetByIdAsync(Guid id);
    Task<List<Models.Entities.Book>> GetAllAsync(int page, int pageSize);
    Task<List<Models.Entities.Book>> SearchByGenreAsync(string genre);
    Task<List<Models.Entities.Book>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<int> CountAsync();
}
