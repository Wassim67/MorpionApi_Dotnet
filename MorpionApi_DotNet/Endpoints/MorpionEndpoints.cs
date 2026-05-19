using MorpionApi_DotNet.Models;
using MorpionApi_DotNet.Services;

namespace MorpionApi_DotNet.Endpoints;

public static class MorpionEndpoints
{
    public static IEndpointRouteBuilder MapMorpionEndpoints(this IEndpointRouteBuilder app)
    {
        var morpion = app.MapGroup("/api/morpion");

        morpion.MapPost("/games", (MorpionGameService gameService) =>
            Results.Created("/api/morpion/games/current", gameService.CreateGame()));

        morpion.MapGet("/games/current", (MorpionGameService gameService) =>
            gameService.GetCurrentGame() is { } game
                ? Results.Ok(game)
                : Results.NotFound(new { message = "Aucune partie en cours." }));

        morpion.MapPost("/games/current/moves", (PlayMoveRequest request, MorpionGameService gameService) =>
        {
            var result = gameService.PlayMove(request.Index);

            return result.Success
                ? Results.Ok(result.Game)
                : Results.BadRequest(new { message = result.Error });
        });

        return app;
    }
}
