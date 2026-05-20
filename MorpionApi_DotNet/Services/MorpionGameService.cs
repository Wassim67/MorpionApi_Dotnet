using Microsoft.EntityFrameworkCore;
using MorpionApi_DotNet.Data;
using MorpionApi_DotNet.Entities;
using MorpionApi_DotNet.Models;
using MorpionApi_DotNet.Players;

namespace MorpionApi_DotNet.Services;

public class MorpionGameService
{
    private const string HumanPlayer = "X";
    private const string BotPlayer = "O";
    private const char EmptyCell = '-';

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
    private readonly AppDbContext _dbContext;

    public MorpionGameService(IBotPlayer botPlayer, AppDbContext dbContext)
    {
        _botPlayer = botPlayer;
        _dbContext = dbContext;
    }

    public async Task<GameDto> CreateGameAsync(string userId, CancellationToken cancellationToken = default)
    {
        var game = new GameEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Board = new string(EmptyCell, 9),
            IsGameOver = false,
            MovesCount = 0,
            StatusMessage = "Ton tour (X)",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(game);
    }

    public async Task<GameDto?> GetCurrentGameAsync(string userId, CancellationToken cancellationToken = default)
    {
        var game = await GetCurrentGameEntityAsync(userId, cancellationToken);
        return game is null ? null : ToDto(game);
    }

    public async Task<PlayMoveResult> PlayMoveAsync(
        string userId,
        int index,
        CancellationToken cancellationToken = default)
    {
        var game = await GetCurrentGameEntityAsync(userId, cancellationToken);
        if (game is null)
        {
            await CreateGameAsync(userId, cancellationToken);
            game = await GetCurrentGameEntityAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("La partie n'a pas pu etre creee.");
        }

        if (index is < 0 or > 8)
        {
            return PlayMoveResult.Fail(
                "L'index doit etre compris entre 0 et 8.",
                StatusCodes.Status400BadRequest);
        }

        if (game.IsGameOver)
        {
            return PlayMoveResult.Fail(
                "La partie est deja terminee. Cree une nouvelle partie.",
                StatusCodes.Status409Conflict);
        }

        var cells = ToCells(game.Board);
        if (!string.IsNullOrEmpty(cells[index]))
        {
            return PlayMoveResult.Fail(
                "Cette case est deja jouee.",
                StatusCodes.Status409Conflict);
        }

        ApplyMove(game, cells, index, HumanPlayer);
        if (!TryEndGame(game, cells, HumanPlayer))
        {
            PlayBotTurn(game, cells);
        }

        game.Board = ToBoard(cells);
        game.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PlayMoveResult.Ok(ToDto(game));
    }

    private async Task<GameEntity?> GetCurrentGameEntityAsync(string userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Games
            .Where(game => game.UserId == userId)
            .OrderByDescending(game => game.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void PlayBotTurn(GameEntity game, string[] cells)
    {
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

        ApplyMove(game, cells, botMoveIndex.Value, BotPlayer);

        if (!TryEndGame(game, cells, BotPlayer))
        {
            game.StatusMessage = "Ton tour (X)";
        }
    }

    private static void ApplyMove(GameEntity game, string[] cells, int index, string player)
    {
        cells[index] = player;
        game.MovesCount++;
    }

    private static bool TryEndGame(GameEntity game, string[] cells, string player)
    {
        if (HasWinner(cells, player))
        {
            game.IsGameOver = true;
            game.StatusMessage = player == HumanPlayer ? "Tu as gagne !" : "Le bot a gagne.";
            return true;
        }

        if (game.MovesCount == 9)
        {
            game.IsGameOver = true;
            game.StatusMessage = "Match nul.";
            return true;
        }

        return false;
    }

    private static bool HasWinner(string[] cells, string player)
    {
        foreach (var combo in WinningCombinations)
        {
            if (cells[combo[0]] == player &&
                cells[combo[1]] == player &&
                cells[combo[2]] == player)
            {
                return true;
            }
        }

        return false;
    }

    private static GameDto ToDto(GameEntity game)
    {
        return new GameDto
        {
            Board = ToCells(game.Board),
            CurrentPlayer = game.IsGameOver ? string.Empty : HumanPlayer,
            IsGameOver = game.IsGameOver,
            MovesCount = game.MovesCount,
            StatusMessage = game.StatusMessage
        };
    }

    private static string[] ToCells(string board)
    {
        return board
            .Select(cell => cell == EmptyCell ? string.Empty : cell.ToString())
            .ToArray();
    }

    private static string ToBoard(string[] cells)
    {
        return string.Concat(cells.Select(cell => string.IsNullOrEmpty(cell) ? EmptyCell : cell[0]));
    }
}
