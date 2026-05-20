using MorpionApi_DotNet.Models;

namespace MorpionApi_DotNet.Services;

public class PlayMoveResult
{
    public bool Success { get; init; }
    public GameDto? Game { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }

    public static PlayMoveResult Ok(GameDto game)
    {
        return new PlayMoveResult
        {
            Success = true,
            Game = game,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public static PlayMoveResult Fail(string error, int statusCode)
    {
        return new PlayMoveResult
        {
            Success = false,
            Error = error,
            StatusCode = statusCode
        };
    }
}
