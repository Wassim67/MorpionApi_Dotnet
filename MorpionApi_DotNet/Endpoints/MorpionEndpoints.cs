using System.Security.Claims;
using MorpionApi_DotNet.Models;
using MorpionApi_DotNet.Services;

namespace MorpionApi_DotNet.Endpoints;

public static class MorpionEndpoints
{
    public static IEndpointRouteBuilder MapMorpionEndpoints(this IEndpointRouteBuilder app)
    {
        var morpion = app.MapGroup("/api/morpion")
            .RequireAuthorization();

        morpion.MapPost("/games", async (
            ClaimsPrincipal user,
            MorpionGameService gameService,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var game = await gameService.CreateGameAsync(userId, cancellationToken);
            return Results.Created("/api/morpion/games/current", game);
        });

        morpion.MapGet("/games/current", async (
            ClaimsPrincipal user,
            MorpionGameService gameService,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            return await gameService.GetCurrentGameAsync(userId, cancellationToken) is { } game
                ? Results.Ok(game)
                : Results.Problem(
                    title: "Partie introuvable",
                    detail: "Aucune partie en cours pour cet utilisateur.",
                    statusCode: StatusCodes.Status404NotFound);
        });

        morpion.MapPost("/games/current/moves", async (
            PlayMoveRequest request,
            ClaimsPrincipal user,
            MorpionGameService gameService,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var result = await gameService.PlayMoveAsync(userId, request.Index, cancellationToken);

            return result.Success
                ? Results.Ok(result.Game)
                : Results.Problem(
                    title: "Coup refuse",
                    detail: result.Error,
                    statusCode: result.StatusCode);
        });

        return app;
    }

    private static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
