using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models.Dtos;
using QuotesApi.Services;

namespace QuotesApi.Extensions;

public static class AuthEndpointExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");

        group.MapPost("/login", async (
            LoginRequest request,
            QuoteDbContext db,
            ITokenService tokenService) =>
        {
            var user = await db.Users
                .FirstOrDefaultAsync(
                    x => x.Email == request.Email);

            if (user is null ||
                !BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            var accessToken =
                tokenService.CreateAccessToken(user);

            var refreshToken =
                Guid.NewGuid().ToString();

            return Results.Ok(new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                expires_in = 900
            });
        });

        return endpoints;
    }
}