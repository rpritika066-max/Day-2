using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuotesApi.Data;
using QuotesApi.Models;
using QuotesApi.Services;

namespace QuotesApi.Extensions;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public static class AuthEndpointExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");

        group.MapPost("/login", async (LoginRequest request, ITokenService tokenService, QuoteDbContext dbContext) =>
        {
            // Simple mock login
            if (request.Username != "testuser" || request.Password != "password")
                return Results.Unauthorized();

            var jwt = tokenService.GenerateJwt(request.Username, out var jwtId);
            var refreshToken = tokenService.GenerateRefreshToken(request.Username, jwtId);

            dbContext.RefreshTokens.Add(refreshToken);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Token = jwt, RefreshToken = refreshToken.Token });
        });

        group.MapPost("/refresh", async (RefreshRequest request, ITokenService tokenService, QuoteDbContext dbContext) =>
        {
            var storedRefreshToken = await tokenService.ValidateAndProcessRefreshTokenAsync(request.RefreshToken);

            if (storedRefreshToken == null)
                return Results.Unauthorized();

            // Generate new tokens
            var jwt = tokenService.GenerateJwt(storedRefreshToken.UserId, out var newJwtId);
            var newRefreshToken = tokenService.GenerateRefreshToken(storedRefreshToken.UserId, newJwtId);

            dbContext.RefreshTokens.Add(newRefreshToken);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Token = jwt, RefreshToken = newRefreshToken.Token });
        });

        return endpoints;
    }
}
