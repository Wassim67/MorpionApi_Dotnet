using Microsoft.AspNetCore.Identity;
using MorpionApi_DotNet.Auth;
using MorpionApi_DotNet.Models;

namespace MorpionApi_DotNet.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");

        auth.MapPost("/access-token", async (
            RegisterUserDto request,
            UserManager<IdentityUser> userManager,
            JwtTokenService tokenService) =>
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Results.ValidationProblem(result.Errors.ToDictionary(
                    error => error.Code,
                    error => new[] { error.Description }));
            }

            return Results.Ok(tokenService.CreateToken(user));
        });

        auth.MapPut("/access-token", async (
            LoginUserDto request,
            UserManager<IdentityUser> userManager,
            JwtTokenService tokenService) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(tokenService.CreateToken(user));
        });

        return app;
    }
}
