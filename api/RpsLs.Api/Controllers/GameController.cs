using Microsoft.AspNetCore.Mvc;
using RpsLs.Api.Models;
using RpsLs.ApplicationService.Services;

namespace RpsLs.Api.Controllers;

[ApiController]
[Route("")]
[Produces("application/json")]
public class GameController(IGameService gameService) : ControllerBase
{
    /// <summary>Returns all available choices.</summary>
    [HttpGet("choices")]
    [ProducesResponseType<IReadOnlyList<Choice>>(StatusCodes.Status200OK)]
    public IActionResult GetChoices() => Ok(gameService.GetAllChoices());

    /// <summary>Returns a single randomly generated choice.</summary>
    [HttpGet("choice")]
    [ProducesResponseType<Choice>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRandomChoice() =>
        Ok(await gameService.GetRandomChoiceAsync());

    /// <summary>Play a round against the computer.</summary>
    [HttpPost("play")]
    [ProducesResponseType<PlayResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Play([FromBody] PlayRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await gameService.PlayAsync(request.Player);
            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Returns the 10 most recent game results.</summary>
    [HttpGet("scoreboard")]
    [ProducesResponseType<IReadOnlyList<ScoreEntry>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScoreboard(){
        try{
            return Ok(await gameService.GetScoreboardAsync());
        }catch(Exception ex){
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Resets the scoreboard.</summary>
    [HttpDelete("scoreboard")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetScoreboard()
    {
        try{
            await gameService.ResetScoreboardAsync();
            return NoContent();
        }catch(Exception ex){
            return BadRequest(new { error = ex.Message});
        }
    }
}
