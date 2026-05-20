using MorpionApi_DotNet.Models;
using MorpionApi_DotNet.Players;

namespace MorpionApi_DotNet.Services;

public class MorpionGameService
{
    private const string HumanPlayer = "X";
    private const string BotPlayer = "O";

    private static readonly int[][] WinningCombinations =
    [
        [0, 1, 2],
        [3, 4, 5],
        [6, 7, 8],
        [0, 3, 6],
        [1, 4, 7],
        [2, 5, 8],
        [0, 4, 8],
        [2, 4, 6]
    ];

    private readonly IBotPlayer _botPlayer;
    private readonly object _syncRoot = new();
    private string[]? _cells;
    private bool _isGameOver;
    private int _movesCount;
    private string _statusMessage = "Ton tour (X)";

    public MorpionGameService(IBotPlayer botPlayer)
    {
        _botPlayer = botPlayer;
    }

    public GameDto CreateGame()
    {
        lock (_syncRoot)
        {
            _cells = Enumerable.Repeat(string.Empty, 9).ToArray();
            _isGameOver = false;
            _movesCount = 0;
            _statusMessage = "Ton tour (X)";

            return ToDto();
        }
    }

    public GameDto? GetCurrentGame()
    {
        lock (_syncRoot)
        {
            return _cells is null ? null : ToDto();
        }
    }

    public PlayMoveResult PlayMove(int index)
    {
        lock (_syncRoot)
        {
            if (_cells is null)
            {
                CreateGame();
            }

            if (index is < 0 or > 8)
            {
                return PlayMoveResult.Fail(
                    "L'index doit être compris entre 0 et 8.",
                    StatusCodes.Status400BadRequest);
            }

            if (_isGameOver)
            {
                return PlayMoveResult.Fail(
                    "La partie est déjà terminée. Crée une nouvelle partie.",
                    StatusCodes.Status409Conflict);
            }

            if (!string.IsNullOrEmpty(_cells![index]))
            {
                return PlayMoveResult.Fail(
                    "Cette case est déjà jouée.",
                    StatusCodes.Status409Conflict);
            }

            ApplyMove(index, HumanPlayer);
            if (TryEndGame(HumanPlayer))
            {
                return PlayMoveResult.Ok(ToDto());
            }

            PlayBotTurn();

            return PlayMoveResult.Ok(ToDto());
        }
    }

    private void PlayBotTurn()
    {
        var cells = _cells ?? throw new InvalidOperationException("Aucune partie en cours.");
        var botMoveIndex = _botPlayer.GetNextMoveIndex(cells);
        if (botMoveIndex is null ||
            botMoveIndex < 0 ||
            botMoveIndex >= cells.Length ||
            !string.IsNullOrEmpty(cells[botMoveIndex.Value]))
        {
            botMoveIndex = cells
                .Select((cell, index) => new { cell, index })
                .FirstOrDefault(x => string.IsNullOrEmpty(x.cell))
                ?.index;

            if (botMoveIndex is null)
            {
                return;
            }
        }

        ApplyMove(botMoveIndex.Value, BotPlayer);

        if (!TryEndGame(BotPlayer))
        {
            _statusMessage = "Ton tour (X)";
        }
    }

    private void ApplyMove(int index, string player)
    {
        _cells![index] = player;
        _movesCount++;
    }

    private bool TryEndGame(string player)
    {
        if (HasWinner(player))
        {
            _isGameOver = true;
            _statusMessage = player == HumanPlayer ? "Tu as gagné !" : "Le bot a gagné.";
            return true;
        }

        if (_movesCount == 9)
        {
            _isGameOver = true;
            _statusMessage = "Match nul.";
            return true;
        }

        return false;
    }

    private bool HasWinner(string player)
    {
        foreach (var combo in WinningCombinations)
        {
            if (_cells![combo[0]] == player &&
                _cells[combo[1]] == player &&
                _cells[combo[2]] == player)
            {
                return true;
            }
        }

        return false;
    }

    private GameDto ToDto()
    {
        return new GameDto
        {
            Board = _cells!.ToArray(),
            CurrentPlayer = _isGameOver ? string.Empty : HumanPlayer,
            IsGameOver = _isGameOver,
            MovesCount = _movesCount,
            StatusMessage = _statusMessage
        };
    }
}
