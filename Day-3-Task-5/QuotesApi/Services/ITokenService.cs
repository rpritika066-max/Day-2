using QuotesApi.Models;

namespace QuotesApi.Services;

public interface ITokenService
{
    string GenerateJwt(string username, out string jwtId);
    RefreshToken GenerateRefreshToken(string username, string jwtId);
    Task<bool> RevokeAllUserTokensAsync(string userId);
    Task<RefreshToken?> ValidateAndProcessRefreshTokenAsync(string refreshTokenString);
}
