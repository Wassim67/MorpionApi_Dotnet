namespace MorpionApi_DotNet.Models;

public class GameDto
{
    public required string[] Board { get; init; }
    public required string CurrentPlayer { get; init; }
    public bool IsGameOver { get; init; }
    public int MovesCount { get; init; }
    public required string StatusMessage { get; init; }
}
