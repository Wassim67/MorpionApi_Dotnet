namespace MorpionApi_DotNet.Players;

public class RandomBotPlayer : IBotPlayer
{
    private readonly Random _random = new();

    public int? GetNextMoveIndex(IReadOnlyList<string> cells)
    {
        var emptyIndexes = cells
            .Select((cell, index) => new { cell, index })
            .Where(x => string.IsNullOrEmpty(x.cell))
            .Select(x => x.index)
            .ToList();

        if (emptyIndexes.Count == 0)
        {
            return null;
        }

        return emptyIndexes[_random.Next(emptyIndexes.Count)];
    }
}
