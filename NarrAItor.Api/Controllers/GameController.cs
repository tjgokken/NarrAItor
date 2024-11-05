using Microsoft.AspNetCore.Mvc;
using NarrAItor.Api.Services;
using NarrAItor.Shared.Interfaces;
using NarrAItor.Shared.Models;

namespace NarrAItor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController(IGameService gameService, ILogger<GameController> logger) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<ActionResult<GameState>> GenerateGame(
        [FromQuery] string theme,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var gameState = await gameService.GenerateNewGame(theme, cancellationToken);
            return Ok(gameState);
        }
        catch (OpenAIException ex) when (ex is OpenAITimeoutException)
        {
            logger.LogWarning(ex, "OpenAI request timed out for theme: {Theme}", theme);
            return StatusCode(503, new ApiError(
                "Game generation is taking longer than expected. Please try again.",
                "OPENAI_TIMEOUT"));
        }
        catch (OpenAIException ex)
        {
            logger.LogError(ex, "OpenAI error for theme: {Theme}", theme);
            return StatusCode(500, new ApiError(
                "Failed to generate game content. Please try again.",
                "OPENAI_ERROR"));
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Game generation cancelled for theme: {Theme}", theme);
            return StatusCode(503, new ApiError(
                "Game generation was cancelled. Please try again.",
                "GENERATION_CANCELLED"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error generating game with theme: {Theme}", theme);
            return StatusCode(500, new ApiError(
                "An unexpected error occurred. Please try again.",
                "INTERNAL_ERROR"));
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveGame([FromBody] GameSaveRequest request)
    {
        try
        {
            await gameService.SaveGame(request.GameState, request.SaveName);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save game: {GameId}", request.GameState.GameId);
            return StatusCode(500, "Failed to save game");
        }
    }

    [HttpGet("load/{gameId}")]
    public async Task<ActionResult<GameState>> LoadGame(string gameId)
    {
        try
        {
            var gameState = await gameService.LoadGame(gameId);
            return Ok(gameState);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load game: {GameId}", gameId);
            return StatusCode(500, "Failed to load game");
        }
    }

    [HttpGet("saved")]
    public async Task<ActionResult<List<SavedGameInfo>>> GetSavedGames()
    {
        try
        {
            var savedGames = await gameService.GetSavedGames();
            return Ok(savedGames);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get saved games");
            return StatusCode(500, "Failed to get saved games list");
        }
    }

    [HttpDelete("delete/{gameId}")]
    public async Task<IActionResult> DeleteGame(string gameId)
    {
        try
        {
            await gameService.DeleteGame(gameId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete game: {GameId}", gameId);
            return StatusCode(500, "Failed to delete game");
        }
    }
}

public class GameSaveRequest
{
    public required GameState GameState { get; set; }
    public required string SaveName { get; set; }
}