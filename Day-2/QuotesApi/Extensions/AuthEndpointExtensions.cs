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


        // LOGIN
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

            var refreshTokenHash =
                BCrypt.Net.BCrypt.HashPassword(
                    refreshToken);


            db.RefreshTokens.Add(
                new QuotesApi.Models.RefreshToken
                {
                    Token = refreshTokenHash,
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                });


            await db.SaveChangesAsync();


            return Results.Ok(new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                expires_in = 900
            });
        });



        // REFRESH TOKEN ROTATION
        group.MapPost("/refresh", async (
            RefreshRequest request,
            QuoteDbContext db,
            ITokenService tokenService) =>
        {
            var tokens = await db.RefreshTokens
                .Include(x => x.User)
                .ToListAsync();


            var storedToken = tokens.FirstOrDefault(
                x => BCrypt.Net.BCrypt.Verify(
                    request.RefreshToken,
                    x.Token));


            if (storedToken is null)
            {
                return Results.Unauthorized();
            }


            // Reuse detection
            if (storedToken.RevokedAt is not null)
            {
                return Results.Unauthorized();
            }


            // Expiry check
            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return Results.Unauthorized();
            }



            var newRefreshToken =
                Guid.NewGuid().ToString();


            var newRefreshTokenHash =
                BCrypt.Net.BCrypt.HashPassword(
                    newRefreshToken);



            // Rotate old token
            storedToken.RevokedAt =
                DateTime.UtcNow;


            storedToken.ReplacedByToken =
                newRefreshTokenHash;



            // Store new token
            db.RefreshTokens.Add(
                new QuotesApi.Models.RefreshToken
                {
                    Token = newRefreshTokenHash,
                    UserId = storedToken.UserId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                });



            await db.SaveChangesAsync();



            var accessToken =
                tokenService.CreateAccessToken(
                    storedToken.User);



            return Results.Ok(new
            {
                access_token = accessToken,
                refresh_token = newRefreshToken,
                expires_in = 900
            });

        });



        return endpoints;
    }
}