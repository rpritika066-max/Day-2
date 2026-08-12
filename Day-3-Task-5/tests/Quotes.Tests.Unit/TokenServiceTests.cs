using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using QuotesApi.Data;
using QuotesApi.Models;
using QuotesApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Quotes.Tests.Unit.Services;

public class TokenServiceTests
{
    private readonly IConfiguration _config;
    private readonly QuoteDbContext _dbContext;
    private readonly IClock _clock;
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _config = Substitute.For<IConfiguration>();
        _config["Jwt:Key"].Returns("supersecretkey12345678901234567890");
        _config["Jwt:Issuer"].Returns("test-issuer");
        _config["Jwt:Audience"].Returns("test-audience");

        var options = new DbContextOptionsBuilder<QuoteDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new QuoteDbContext(options);

        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        _sut = new TokenService(_config, _dbContext, _clock);
    }

    [Fact]
    public void GenerateJwt_ValidInput_ReturnsTokenAndJwtId()
    {
        // Act
        var token = _sut.GenerateJwt("testuser", out var jwtId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        jwtId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ValidInput_ReturnsRefreshToken()
    {
        // Act
        var token = _sut.GenerateRefreshToken("testuser", "jwt123");

        // Assert
        token.Should().NotBeNull();
        token.UserId.Should().Be("testuser");
        token.JwtId.Should().Be("jwt123");
        token.CreationDate.Should().Be(_clock.UtcNow.UtcDateTime);
        token.ExpiryDate.Should().Be(_clock.UtcNow.UtcDateTime.AddMonths(1));
        token.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateAndProcessRefreshTokenAsync_TokenNotFound_ReturnsNull()
    {
        // Act
        var result = await _sut.ValidateAndProcessRefreshTokenAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAndProcessRefreshTokenAsync_TokenExpired_ReturnsNull()
    {
        // Arrange
        var token = new RefreshToken
        {
            UserId = "user1",
            Token = "expiredToken",
            ExpiryDate = _clock.UtcNow.UtcDateTime.AddMinutes(-1), // Expired
            JwtId = "jwt1"
        };
        _dbContext.RefreshTokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.ValidateAndProcessRefreshTokenAsync("expiredToken");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAndProcessRefreshTokenAsync_TokenUsed_RevokesAllAndReturnsNull()
    {
        // Arrange
        var token1 = new RefreshToken { UserId = "user1", Token = "usedToken", Used = true, JwtId = "jwt1", ExpiryDate = _clock.UtcNow.UtcDateTime.AddMonths(1) };
        var token2 = new RefreshToken { UserId = "user1", Token = "validToken", Used = false, JwtId = "jwt2", ExpiryDate = _clock.UtcNow.UtcDateTime.AddMonths(1) };
        _dbContext.RefreshTokens.AddRange(token1, token2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.ValidateAndProcessRefreshTokenAsync("usedToken");

        // Assert
        result.Should().BeNull(); // Chain revoked
        
        var allTokens = await _dbContext.RefreshTokens.Where(x => x.UserId == "user1").ToListAsync();
        allTokens.Should().AllSatisfy(t => t.Invalidated.Should().BeTrue());
    }

    [Fact]
    public async Task ValidateAndProcessRefreshTokenAsync_TokenInvalidated_RevokesAllAndReturnsNull()
    {
        // Arrange
        var token1 = new RefreshToken { UserId = "user1", Token = "invalidatedToken", Invalidated = true, JwtId = "jwt1", ExpiryDate = _clock.UtcNow.UtcDateTime.AddMonths(1) };
        var token2 = new RefreshToken { UserId = "user1", Token = "validToken", Used = false, JwtId = "jwt2", ExpiryDate = _clock.UtcNow.UtcDateTime.AddMonths(1) };
        _dbContext.RefreshTokens.AddRange(token1, token2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.ValidateAndProcessRefreshTokenAsync("invalidatedToken");

        // Assert
        result.Should().BeNull(); // Chain revoked
        
        var allTokens = await _dbContext.RefreshTokens.Where(x => x.UserId == "user1").ToListAsync();
        allTokens.Should().AllSatisfy(t => t.Invalidated.Should().BeTrue());
    }

    [Fact]
    public async Task ValidateAndProcessRefreshTokenAsync_ValidToken_ReturnsTokenAndMarksAsUsed()
    {
        // Arrange
        var tokenString = "validToken123";
        var token = new RefreshToken
        {
            UserId = "user1",
            Token = tokenString,
            ExpiryDate = _clock.UtcNow.UtcDateTime.AddMonths(1),
            JwtId = "jwt1"
        };
        _dbContext.RefreshTokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.ValidateAndProcessRefreshTokenAsync(tokenString);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(tokenString);
        result.Used.Should().BeTrue();
    }
}
