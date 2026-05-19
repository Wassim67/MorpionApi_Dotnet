namespace MorpionApi_DotNet.Players;

public interface IBotPlayer
{
    int? GetNextMoveIndex(IReadOnlyList<string> cells);
}
