using RpsLs.ApplicationService.Models;

namespace RpsLs.ApplicationService.Services;

public interface IGameService
{
    IReadOnlyList<Choice> GetAllChoices();
    Task<Choice> GetRandomChoiceAsync();
    Task<PlayResult> PlayAsync(int playerChoiceId);
    Task<IReadOnlyList<ScoreEntry>> GetScoreboardAsync();
    Task ResetScoreboardAsync();
}
