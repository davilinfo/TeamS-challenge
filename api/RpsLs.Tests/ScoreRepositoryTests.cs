using Microsoft.EntityFrameworkCore;
using RpsLs.ApplicationService.Models;
using RpsLs.Infra.Data;
using RpsLs.Infra.Repositories;
using Xunit;

namespace RpsLs.Tests;

public class ScoreRepositoryTests
{
    // Each test gets its own isolated in-memory database
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ScoreEntry MakeEntry(
        string result = "win",
        int player = 2,
        string playerName = "paper",
        int computer = 1,
        string computerName = "rock",
        DateTime? playedAt = null) =>
        new(0, result, player, playerName, computer, computerName, playedAt ?? DateTime.UtcNow);

    // ── AddAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsEntry()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);

        await repo.AddAsync(MakeEntry());

        Assert.Equal(1, ctx.Scores.Count());
    }

    [Fact]
    public async Task AddAsync_AssignsNonZeroId()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);

        await repo.AddAsync(MakeEntry());

        Assert.True(ctx.Scores.Single().Id > 0);
    }

    [Fact]
    public async Task AddAsync_PreservesAllFields()
    {
        var playedAt = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);

        await repo.AddAsync(new ScoreEntry(0, "lose", 3, "scissors", 1, "rock", playedAt));

        var stored = ctx.Scores.Single();
        Assert.Equal("lose",     stored.Result);
        Assert.Equal(3,          stored.Player);
        Assert.Equal("scissors", stored.PlayerName);
        Assert.Equal(1,          stored.Computer);
        Assert.Equal("rock",     stored.ComputerName);
        Assert.Equal(playedAt,   stored.PlayedAt);
    }

    [Fact]
    public async Task AddAsync_MultipleEntries_EachGetUniqueId()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);

        await repo.AddAsync(MakeEntry());
        await repo.AddAsync(MakeEntry());
        await repo.AddAsync(MakeEntry());

        var ids = ctx.Scores.Select(s => s.Id).ToList();
        Assert.Equal(3, ids.Distinct().Count());
    }

    // ── GetLastAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLastAsync_ReturnsEmpty_WhenNoEntries()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);

        var result = await repo.GetLastAsync(10);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLastAsync_ReturnsEntriesOrderedByPlayedAtDescending()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);
        var now = DateTime.UtcNow;

        await repo.AddAsync(MakeEntry(playedAt: now.AddMinutes(-5)));
        await repo.AddAsync(MakeEntry(playedAt: now));
        await repo.AddAsync(MakeEntry(playedAt: now.AddMinutes(-2)));

        var result = await repo.GetLastAsync(10);

        Assert.Equal(now,                 result[0].PlayedAt);
        Assert.Equal(now.AddMinutes(-2),  result[1].PlayedAt);
        Assert.Equal(now.AddMinutes(-5),  result[2].PlayedAt);
    }

    [Fact]
    public async Task GetLastAsync_RespectsCountLimit()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);
        var now = DateTime.UtcNow;

        for (int i = 0; i < 15; i++)
            await repo.AddAsync(MakeEntry(playedAt: now.AddSeconds(i)));

        var result = await repo.GetLastAsync(10);

        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task GetLastAsync_ReturnsAll_WhenCountExceedsStoredEntries()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);

        await repo.AddAsync(MakeEntry());
        await repo.AddAsync(MakeEntry());

        var result = await repo.GetLastAsync(10);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetLastAsync_ReturnsNewestEntries_WhenLimitApplied()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);
        var now = DateTime.UtcNow;

        for (int i = 0; i < 5; i++)
            await repo.AddAsync(MakeEntry(result: i < 3 ? "old" : "new", playedAt: now.AddSeconds(i)));

        var result = await repo.GetLastAsync(2);

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("new", e.Result));
    }

    [Fact]
    public async Task GetLastAsync_MapsAllFieldsToScoreEntry()
    {
        var playedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);

        await repo.AddAsync(new ScoreEntry(0, "tie", 5, "spock", 5, "spock", playedAt));

        var entry = (await repo.GetLastAsync(1))[0];
        Assert.True(entry.Id > 0);
        Assert.Equal("tie",   entry.Result);
        Assert.Equal(5,       entry.Player);
        Assert.Equal("spock", entry.PlayerName);
        Assert.Equal(5,       entry.Computer);
        Assert.Equal("spock", entry.ComputerName);
        Assert.Equal(playedAt, entry.PlayedAt);
    }

    [Theory]
    [InlineData("win")]
    [InlineData("lose")]
    [InlineData("tie")]
    public async Task GetLastAsync_PreservesResultValue(string result)
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);

        await repo.AddAsync(MakeEntry(result: result));

        var entry = (await repo.GetLastAsync(1))[0];
        Assert.Equal(result, entry.Result);
    }

    // ── ClearAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ClearAsync_RemovesAllEntries()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);
        await repo.AddAsync(MakeEntry());
        await repo.AddAsync(MakeEntry());

        await repo.ClearAsync();

        Assert.Equal(0, ctx.Scores.Count());
    }

    [Fact]
    public async Task ClearAsync_OnEmptyTable_DoesNotThrow()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);

        var ex = await Record.ExceptionAsync(() => repo.ClearAsync());

        Assert.Null(ex);
    }

    [Fact]
    public async Task ClearAsync_AllowsNewEntriesAfterClear()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);
        await repo.AddAsync(MakeEntry("win"));
        await repo.ClearAsync();

        await repo.AddAsync(MakeEntry("tie"));
        var result = await repo.GetLastAsync(10);

        Assert.Single(result);
        Assert.Equal("tie", result[0].Result);
    }

    [Fact]
    public async Task ClearAsync_GetLastAsync_ReturnsEmpty_AfterClear()
    {
        await using var ctx = CreateContext();
        var repo = new ScoreRepository(ctx);
        for (int i = 0; i < 5; i++)
            await repo.AddAsync(MakeEntry());

        await repo.ClearAsync();

        Assert.Empty(await repo.GetLastAsync(10));
    }
}
