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
                : Results.Problem(
                    title: "Partie introuvable",
                    detail: "Aucune partie en cours.",
                    statusCode: StatusCodes.Status404NotFound));

        morpion.MapPost("/games/current/moves", (PlayMoveRequest request, MorpionGameService gameService) =>
        {
            var result = gameService.PlayMove(request.Index);

            return result.Success
                ? Results.Ok(result.Game)
                : Results.Problem(
                    title: "Coup refusé",
                    detail: result.Error,
                    statusCode: result.StatusCode);
        });

        return app;
    }
}
