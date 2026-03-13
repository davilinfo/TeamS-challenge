using Microsoft.AspNetCore.Mvc;
using Moq;
using RpsLs.Api.Controllers;
using RpsLs.ApplicationService.Services;
using Xunit;

// The controller proxies ApplicationService models through Ok(), so we assert on those types.
using AppChoice     = RpsLs.ApplicationService.Models.Choice;
using AppPlayResult = RpsLs.ApplicationService.Models.PlayResult;
using AppScoreEntry = RpsLs.ApplicationService.Models.ScoreEntry;
using ApiPlayRequest = RpsLs.Api.Models.PlayRequest;
using Microsoft.Extensions.Logging;

namespace RpsLs.Tests;

public class GameControllerTests
{
    private static (GameController controller, Mock<IGameService> mockService) Create()
    {
        var mock = new Mock<IGameService>();
        var mockLogger = new Mock<ILogger<GameController>>();
        return (new GameController(mock.Object, mockLogger.Object), mock);
    }

    // Reads the anonymous { error = "..." } object returned by bad-request responses
    private static string? GetErrorMessage(object? value) =>
        value?.GetType().GetProperty("error")?.GetValue(value) as string;

    // ── GetChoices ────────────────────────────────────────────────────────────

    [Fact]
    public void GetChoices_Returns200_WithChoiceList()
    {
        var (ctrl, svc) = Create();
        var choices = new List<AppChoice> { new(1, "rock"), new(2, "paper") }.AsReadOnly();
        svc.Setup(s => s.GetAllChoices()).Returns(choices);

        var result = ctrl.GetChoices();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(choices, ok.Value);
    }

    [Fact]
    public void GetChoices_Returns200_WithEmptyList_WhenServiceReturnsNone()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.GetAllChoices()).Returns(new List<AppChoice>().AsReadOnly());

        var result = ctrl.GetChoices();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Empty((IEnumerable<AppChoice>)ok.Value!);
    }

    [Fact]
    public void GetChoices_CallsServiceOnce()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.GetAllChoices()).Returns(new List<AppChoice>().AsReadOnly());

        ctrl.GetChoices();

        svc.Verify(s => s.GetAllChoices(), Times.Once);
    }

    // ── GetRandomChoice ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetRandomChoice_Returns200_WithChoice()
    {
        var (ctrl, svc) = Create();
        var choice = new AppChoice(3, "scissors");
        svc.Setup(s => s.GetRandomChoiceAsync()).ReturnsAsync(choice);

        var result = await ctrl.GetRandomChoice();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(choice, ok.Value);
    }

    [Fact]
    public async Task GetRandomChoice_CallsServiceOnce()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.GetRandomChoiceAsync()).ReturnsAsync(new AppChoice(1, "rock"));

        await ctrl.GetRandomChoice();

        svc.Verify(s => s.GetRandomChoiceAsync(), Times.Once);
    }

    // ── Play ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("win")]
    [InlineData("lose")]
    [InlineData("tie")]
    public async Task Play_Returns200_WithPlayResult_ForEachOutcome(string outcome)
    {
        var (ctrl, svc) = Create();
        var playResult = new AppPlayResult(outcome, 2, 1);
        svc.Setup(s => s.PlayAsync(2)).ReturnsAsync(playResult);

        var result = await ctrl.Play(new ApiPlayRequest(2));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(playResult, ok.Value);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Play_PassesPlayerChoiceToService(int playerChoice)
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.PlayAsync(playerChoice)).ReturnsAsync(new AppPlayResult("tie", playerChoice, playerChoice));

        await ctrl.Play(new ApiPlayRequest(playerChoice));

        svc.Verify(s => s.PlayAsync(playerChoice), Times.Once);
    }

    [Fact]
    public async Task Play_Returns400_WhenModelStateIsInvalid()
    {
        var (ctrl, _) = Create();
        ctrl.ModelState.AddModelError("Player", "The field Player must be between 1 and 5.");

        var result = await ctrl.Play(new ApiPlayRequest(0));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Play_DoesNotCallService_WhenModelStateIsInvalid()
    {
        var (ctrl, svc) = Create();
        ctrl.ModelState.AddModelError("Player", "Required");

        await ctrl.Play(new ApiPlayRequest(0));

        svc.Verify(s => s.PlayAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Play_Returns400_WhenServiceThrowsArgumentOutOfRange()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.PlayAsync(It.IsAny<int>()))
           .ThrowsAsync(new ArgumentOutOfRangeException("playerChoiceId", "Choice must be between 1 and 5."));

        var result = await ctrl.Play(new ApiPlayRequest(2));

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Choice must be between 1 and 5.", GetErrorMessage(bad.Value));
    }

    [Fact]
    public async Task Play_Returns200_NotBadRequest_WhenModelStateIsValid()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.PlayAsync(1)).ReturnsAsync(new AppPlayResult("tie", 1, 1));

        var result = await ctrl.Play(new ApiPlayRequest(1));

        Assert.IsType<OkObjectResult>(result);
    }

    // ── GetScoreboard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetScoreboard_Returns200_WithEntries()
    {
        var (ctrl, svc) = Create();
        var entries = new List<AppScoreEntry>
        {
            new(1, "win",  2, "paper", 1, "rock", DateTime.UtcNow),
            new(2, "lose", 1, "rock",  2, "paper", DateTime.UtcNow),
        }.AsReadOnly();
        svc.Setup(s => s.GetScoreboardAsync()).ReturnsAsync(entries);

        var result = await ctrl.GetScoreboard();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(entries, ok.Value);
    }

    [Fact]
    public async Task GetScoreboard_Returns200_WithEmptyList_WhenNoEntries()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.GetScoreboardAsync()).ReturnsAsync(new List<AppScoreEntry>().AsReadOnly());

        var result = await ctrl.GetScoreboard();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Empty((IEnumerable<AppScoreEntry>)ok.Value!);
    }

    [Fact]
    public async Task GetScoreboard_Returns400_WithErrorMessage_WhenServiceThrows()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.GetScoreboardAsync()).ThrowsAsync(new Exception("DB error"));

        var result = await ctrl.GetScoreboard();

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("DB error", GetErrorMessage(bad.Value));
    }

    [Fact]
    public async Task GetScoreboard_CallsServiceOnce()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.GetScoreboardAsync()).ReturnsAsync(new List<AppScoreEntry>().AsReadOnly());

        await ctrl.GetScoreboard();

        svc.Verify(s => s.GetScoreboardAsync(), Times.Once);
    }

    // ── ResetScoreboard ───────────────────────────────────────────────────────

    [Fact]
    public async Task ResetScoreboard_Returns204_OnSuccess()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.ResetScoreboardAsync()).Returns(Task.CompletedTask);

        var result = await ctrl.ResetScoreboard();

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ResetScoreboard_CallsServiceOnce()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.ResetScoreboardAsync()).Returns(Task.CompletedTask);

        await ctrl.ResetScoreboard();

        svc.Verify(s => s.ResetScoreboardAsync(), Times.Once);
    }

    [Fact]
    public async Task ResetScoreboard_Returns400_WithErrorMessage_WhenServiceThrows()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.ResetScoreboardAsync()).ThrowsAsync(new Exception("Reset failed"));

        var result = await ctrl.ResetScoreboard();

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Reset failed", GetErrorMessage(bad.Value));
    }

    [Fact]
    public async Task ResetScoreboard_DoesNotReturnOk_OnSuccess()
    {
        var (ctrl, svc) = Create();
        svc.Setup(s => s.ResetScoreboardAsync()).Returns(Task.CompletedTask);

        var result = await ctrl.ResetScoreboard();

        Assert.IsNotType<OkObjectResult>(result);
    }
}
