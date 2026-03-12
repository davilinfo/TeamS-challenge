using Moq;
using RpsLs.ApplicationService.Models;
using RpsLs.ApplicationService.Services;
using Xunit;

namespace RpsLs.Tests;

public class GameServiceTests
{
    // Helper: creates a GameService with a fixed random number and a default no-op score repository
    private static GameService CreateService(int randomNumber) =>
        CreateServiceWithRepo(randomNumber).sut;

    private static (GameService sut, Mock<IRandomService> mockRandom, Mock<IScoreRepository> mockRepo)
        CreateServiceWithRepo(int randomNumber)
    {
        var mockRandom = new Mock<IRandomService>();
        mockRandom.Setup(s => s.GetRandomNumberAsync()).ReturnsAsync(randomNumber);

        var mockRepo = new Mock<IScoreRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<ScoreEntry>())).Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.GetLastAsync(It.IsAny<int>())).ReturnsAsync(new List<ScoreEntry>().AsReadOnly());
        mockRepo.Setup(r => r.ClearAsync()).Returns(Task.CompletedTask);

        return (new GameService(mockRandom.Object, mockRepo.Object), mockRandom, mockRepo);
    }

    // ── GetAllChoices ────────────────────────────────────────────────────────

    [Fact]
    public void GetAllChoices_Returns_Five_Choices()
    {
        var sut = CreateService(1);
        Assert.Equal(5, sut.GetAllChoices().Count);
    }

    [Theory]
    [InlineData(1, "rock")]
    [InlineData(2, "paper")]
    [InlineData(3, "scissors")]
    [InlineData(4, "lizard")]
    [InlineData(5, "spock")]
    public void GetAllChoices_HasCorrectNameForEachId(int id, string expectedName)
    {
        var sut = CreateService(1);
        Assert.Equal(expectedName, sut.GetAllChoices().Single(c => c.Id == id).Name);
    }

    [Fact]
    public void GetAllChoices_AllIdsAreUnique()
    {
        var sut = CreateService(1);
        var choices = sut.GetAllChoices();
        Assert.Equal(choices.Count, choices.Select(c => c.Id).Distinct().Count());
    }

    [Fact]
    public void GetAllChoices_AllNamesAreNonEmpty()
    {
        var sut = CreateService(1);
        Assert.All(sut.GetAllChoices(), c => Assert.False(string.IsNullOrWhiteSpace(c.Name)));
    }

    // ── GetRandomChoice ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(1,   1)]   // (1-1)   % 5 = 0 → rock   (1)
    [InlineData(2,   2)]   // (2-1)   % 5 = 1 → paper  (2)
    [InlineData(3,   3)]   // (3-1)   % 5 = 2 → scissors (3)
    [InlineData(4,   4)]   // (4-1)   % 5 = 3 → lizard (4)
    [InlineData(5,   5)]   // (5-1)   % 5 = 4 → spock  (5)
    [InlineData(6,   1)]   // (6-1)   % 5 = 0 → rock   (1)  wraps
    [InlineData(10,  5)]   // (10-1)  % 5 = 4 → spock  (5)
    [InlineData(11,  1)]   // (11-1)  % 5 = 0 → rock   (1)
    [InlineData(100, 5)]   // (100-1) % 5 = 4 → spock  (5)  upper boundary
    public async Task GetRandomChoice_MapsRandomNumberToExpectedChoiceId(int randomNumber, int expectedChoiceId)
    {
        var sut = CreateService(randomNumber);
        var choice = await sut.GetRandomChoiceAsync();
        Assert.Equal(expectedChoiceId, choice.Id);
    }

    [Fact]
    public async Task GetRandomChoice_ReturnedChoiceExistsInGetAllChoices()
    {
        var sut = CreateService(42);
        var random = await sut.GetRandomChoiceAsync();
        Assert.Contains(sut.GetAllChoices(), c => c.Id == random.Id && c.Name == random.Name);
    }

    // ── Win outcomes ─────────────────────────────────────────────────────────

    [Theory]
    // Rock(1) crushes Scissors(3) and Lizard(4)
    [InlineData(1, 3)]
    [InlineData(1, 4)]
    // Paper(2) covers Rock(1) and disproves Spock(5)
    [InlineData(2, 1)]
    [InlineData(2, 5)]
    // Scissors(3) cuts Paper(2) and decapitates Lizard(4)
    [InlineData(3, 2)]
    [InlineData(3, 4)]
    // Lizard(4) poisons Spock(5) and eats Paper(2)
    [InlineData(4, 5)]
    [InlineData(4, 2)]
    // Spock(5) vaporizes Rock(1) and smashes Scissors(3)
    [InlineData(5, 1)]
    [InlineData(5, 3)]
    public async Task Play_ReturnsWin_WhenPlayerChoiceBeatsComputer(int playerChoice, int computerChoice)
    {
        var sut = CreateService(computerChoice);
        var result = await sut.PlayAsync(playerChoice);
        Assert.Equal("win", result.Results);
        Assert.Equal(playerChoice, result.Player);
        Assert.Equal(computerChoice, result.Computer);
    }

    // ── Lose outcomes ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(3, 1)]   // Rock beats Scissors
    [InlineData(4, 1)]   // Rock beats Lizard
    [InlineData(1, 2)]   // Paper beats Rock
    [InlineData(5, 2)]   // Paper beats Spock
    [InlineData(2, 3)]   // Scissors beats Paper
    [InlineData(4, 3)]   // Scissors beats Lizard
    [InlineData(5, 4)]   // Lizard beats Spock
    [InlineData(2, 4)]   // Lizard beats Paper
    [InlineData(1, 5)]   // Spock beats Rock
    [InlineData(3, 5)]   // Spock beats Scissors
    public async Task Play_ReturnsLose_WhenComputerChoiceBeatsPlayer(int playerChoice, int computerChoice)
    {
        var sut = CreateService(computerChoice);
        var result = await sut.PlayAsync(playerChoice);
        Assert.Equal("lose", result.Results);
        Assert.Equal(playerChoice, result.Player);
        Assert.Equal(computerChoice, result.Computer);
    }

    // ── Tie outcomes ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Play_ReturnsTie_WhenBothPlayersChooseTheSame(int choice)
    {
        var sut = CreateService(choice);
        var result = await sut.PlayAsync(choice);
        Assert.Equal("tie", result.Results);
    }

    // ── Invalid input ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task Play_ThrowsArgumentOutOfRange_WhenChoiceIdIsOutOfRange(int invalidChoice)
    {
        var sut = CreateService(1);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.PlayAsync(invalidChoice));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Play_InvalidChoice_DoesNotCallRepository(int invalidChoice)
    {
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.PlayAsync(invalidChoice));
        mockRepo.Verify(r => r.AddAsync(It.IsAny<ScoreEntry>()), Times.Never);
    }

    // ── Recorded entry content ────────────────────────────────────────────────

    [Fact]
    public async Task Play_RecordedEntry_HasCorrectPlayerAndComputerNames()
    {
        // computer=rock(1), player=paper(2)
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);
        ScoreEntry? captured = null;
        mockRepo.Setup(r => r.AddAsync(It.IsAny<ScoreEntry>()))
            .Callback<ScoreEntry>(e => captured = e)
            .Returns(Task.CompletedTask);

        await sut.PlayAsync(2);

        Assert.NotNull(captured);
        Assert.Equal("paper", captured!.PlayerName);
        Assert.Equal("rock", captured.ComputerName);
    }

    [Fact]
    public async Task Play_RecordedEntry_HasResultMatchingReturnedResult()
    {
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);
        ScoreEntry? captured = null;
        mockRepo.Setup(r => r.AddAsync(It.IsAny<ScoreEntry>()))
            .Callback<ScoreEntry>(e => captured = e)
            .Returns(Task.CompletedTask);

        var result = await sut.PlayAsync(2);

        Assert.NotNull(captured);
        Assert.Equal(result.Results, captured!.Result);
    }

    [Fact]
    public async Task Play_RecordedEntry_HasRecentPlayedAt()
    {
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);
        ScoreEntry? captured = null;
        mockRepo.Setup(r => r.AddAsync(It.IsAny<ScoreEntry>()))
            .Callback<ScoreEntry>(e => captured = e)
            .Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;
        await sut.PlayAsync(1);

        Assert.NotNull(captured);
        Assert.InRange(captured!.PlayedAt, before, DateTime.UtcNow);
    }

    [Fact]
    public async Task Play_RecordedEntry_IdIsZero_BeforeRepositoryAssigns()
    {
        // GameService passes Id=0; the repository (DB) is responsible for assigning the real Id
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);
        ScoreEntry? captured = null;
        mockRepo.Setup(r => r.AddAsync(It.IsAny<ScoreEntry>()))
            .Callback<ScoreEntry>(e => captured = e)
            .Returns(Task.CompletedTask);

        await sut.PlayAsync(1);

        Assert.NotNull(captured);
        Assert.Equal(0, captured!.Id);
    }

    // ── Scoreboard delegation ────────────────────────────────────────────────

    [Fact]
    public async Task GetScoreboard_ReturnsEmpty_WhenNoPlaysOccurred()
    {
        var sut = CreateService(1);
        var board = await sut.GetScoreboardAsync();
        Assert.Empty(board);
    }

    [Fact]
    public async Task GetScoreboard_RecordsResultAfterPlay()
    {
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);
        var captured = new List<ScoreEntry>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<ScoreEntry>()))
            .Callback<ScoreEntry>(captured.Add)
            .Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.GetLastAsync(10))
            .ReturnsAsync(() => captured.AsReadOnly());

        await sut.PlayAsync(2);
        var board = await sut.GetScoreboardAsync();

        Assert.Single(board);
        Assert.Equal("win", board[0].Result);
        Assert.Equal(2, board[0].Player);
        Assert.Equal(1, board[0].Computer);
    }

    [Fact]
    public async Task GetScoreboard_RequestsAtMostTenEntries()
    {
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);

        for (int i = 0; i < 15; i++)
            await sut.PlayAsync(1);
        await sut.GetScoreboardAsync();

        mockRepo.Verify(r => r.AddAsync(It.IsAny<ScoreEntry>()), Times.Exactly(15));
        mockRepo.Verify(r => r.GetLastAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetScoreboard_PassesThroughRepositoryResultUnchanged()
    {
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);
        var entries = new List<ScoreEntry>
        {
            new(2, "tie", 1, "rock",  1, "rock",  DateTime.UtcNow),
            new(1, "win", 2, "paper", 1, "rock",  DateTime.UtcNow.AddMinutes(-1)),
        }.AsReadOnly();
        mockRepo.Setup(r => r.GetLastAsync(10)).ReturnsAsync(entries);

        var board = await sut.GetScoreboardAsync();

        Assert.Equal("tie", board[0].Result);
        Assert.Equal("win", board[1].Result);
    }

    // ── Reset delegation ─────────────────────────────────────────────────────

    [Fact]
    public async Task ResetScoreboard_DelegatesClearToRepository()
    {
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);
        await sut.ResetScoreboardAsync();
        mockRepo.Verify(r => r.ClearAsync(), Times.Once);
    }

    [Fact]
    public async Task ResetScoreboard_AllowsNewEntriesAfterReset()
    {
        var (sut, _, mockRepo) = CreateServiceWithRepo(1);

        await sut.PlayAsync(1);
        await sut.ResetScoreboardAsync();
        await sut.PlayAsync(2);

        mockRepo.Verify(r => r.ClearAsync(), Times.Once);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<ScoreEntry>()), Times.Exactly(2));
    }
}
