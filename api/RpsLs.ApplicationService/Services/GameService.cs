using RpsLs.ApplicationService.Models;

namespace RpsLs.ApplicationService.Services;

/// <summary>
/// Core game logic for Rock Paper Scissors Lizard Spock.
/// The wins dictionary maps each choice to the set of choices it defeats.
/// </summary>
public class GameService(IRandomService randomService, IScoreRepository scoreRepository) : IGameService
{
    private static readonly int _rockChoiceId = 1;
    private static readonly int _paperChoiceId = 2;
    private static readonly int _scissorsChoiceId = 3;
    private static readonly int _lizardChoiceId = 4;
    private static readonly int _spockChoiceId = 5;
    private static readonly string _rockChoiceName = "rock";
    private static readonly string _paperChoiceName = "paper";
    private static readonly string _scissorsChoiceName = "scissors";
    private static readonly string _lizardChoiceName = "lizard";
    private static readonly string _spockChoiceName = "spock";
    private static readonly string _tieResult = "tie";
    private static readonly string _winResult = "win";
    private static readonly string _loseResult = "lose";

    // 1=Rock, 2=Paper, 3=Scissors, 4=Lizard, 5=Spock
    private static readonly IReadOnlyList<Choice> Choices =
    [
        new(_rockChoiceId, _rockChoiceName),
        new(_paperChoiceId, _paperChoiceName),
        new(_scissorsChoiceId, _scissorsChoiceName),
        new(_lizardChoiceId, _lizardChoiceName),
        new(_spockChoiceId, _spockChoiceName),
    ];

    // Key beats all values in its set
    private static readonly Dictionary<int, HashSet<int>> Wins = new()
    {
        [_rockChoiceId] = [_scissorsChoiceId, _lizardChoiceId],       // Rock     crushes Scissors, crushes Lizard
        [_paperChoiceId] = [_rockChoiceId, _spockChoiceId],           // Paper    covers  Rock,     disproves Spock
        [_scissorsChoiceId] = [_paperChoiceId, _lizardChoiceId],      // Scissors cuts    Paper,    decapitates Lizard
        [_lizardChoiceId] = [_spockChoiceId, _paperChoiceId],         // Lizard   poisons Spock,    eats Paper
        [_spockChoiceId] = [_rockChoiceId, _scissorsChoiceId],        // Spock    vaporizes Rock,   smashes Scissors
    };

    private readonly int _mod5 = 5;
    private readonly int _randomNumberChoiceLessOne = 1;
    private readonly string _playerChoiceRangeMsg = "Choice must be between 1 and 5.";

    public IReadOnlyList<Choice> GetAllChoices() => Choices;

    public async Task<Choice> GetRandomChoiceAsync()
    {
        var number = await randomService.GetRandomNumberAsync();
        var index = (number - _randomNumberChoiceLessOne) % _mod5; // maps 1-100 -> 0-4
        return Choices[index];
    }

    public async Task<PlayResult> PlayAsync(int playerChoiceId)
    {
        if (!Wins.ContainsKey(playerChoiceId))
            throw new ArgumentOutOfRangeException(nameof(playerChoiceId), _playerChoiceRangeMsg);

        var computer = await GetRandomChoiceAsync();
        var result = DetermineResult(playerChoiceId, computer.Id);

        await RecordScoreAsync(result, playerChoiceId, computer.Id);

        return new PlayResult(result, playerChoiceId, computer.Id);
    }

    public Task<IReadOnlyList<ScoreEntry>> GetScoreboardAsync() =>
        scoreRepository.GetLastAsync(10);

    public Task ResetScoreboardAsync() =>
        scoreRepository.ClearAsync();

    private static string DetermineResult(int player, int computer)
    {
        if (player == computer) return _tieResult;

        if (Wins[player].Contains(computer))
        {
            return _winResult;
        }
        else
        {
            return Wins[computer].Contains(player) ? _loseResult : _tieResult;
        }
    }

    private async Task RecordScoreAsync(string result, int playerId, int computerId)
    {
        var playerName = Choices.First(c => c.Id == playerId).Name;
        var computerName = Choices.First(c => c.Id == computerId).Name;
        await scoreRepository.AddAsync(new ScoreEntry(0, result, playerId, playerName, computerId, computerName, DateTime.UtcNow));
    }
}
