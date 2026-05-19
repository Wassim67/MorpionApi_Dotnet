using MorpionApi_DotNet.Models;

namespace MorpionApi_DotNet.Services;

public class PlayMoveResult
{
    public bool Success { get; init; }
    public GameDto? Game { get; init; }
    public string? Error { get; init; }

    public static PlayMoveResult Ok(GameDto game)
    {
        return new PlayMoveResult
        {
            Success = true,
            Game = game
        };
    }

    public static PlayMoveResult Fail(string error)
    {
        return new PlayMoveResult
        {
            Success = false,
            Error = error
        };
    }
}
