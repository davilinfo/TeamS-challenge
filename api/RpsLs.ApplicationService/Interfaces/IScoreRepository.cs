using RpsLs.ApplicationService.Models;

namespace RpsLs.ApplicationService.Services;

public interface IScoreRepository
{
    Task AddAsync(ScoreEntry entry);
    Task<IReadOnlyList<ScoreEntry>> GetLastAsync(int count);
    Task ClearAsync();
}
