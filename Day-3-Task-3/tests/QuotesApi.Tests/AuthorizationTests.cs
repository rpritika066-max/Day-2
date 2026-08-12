using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Models.Dtos;

namespace QuotesApi.Tests;

public class AuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostQuote_WithoutQuotesWriteScope_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("TestScheme")
                    .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                        "TestScheme", options =>
                        {
                            options.Identity = new ClaimsIdentity(
                                new Claim[]
                                {
                                    new Claim(ClaimTypes.NameIdentifier, "user1")
                                    // Missing "scope" claim
                                }, "TestAuthType");
                        });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme");

        var request = new CreateQuoteRequest
        {
            Author = "Test Author",
            Text = "Test Quote"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/quotes", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteQuote_NotOwner_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("TestScheme")
                    .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                        "TestScheme", options =>
                        {
                            options.Identity = new ClaimsIdentity(
                                new Claim[]
                                {
                                    new Claim(ClaimTypes.NameIdentifier, "hacker_user")
                                }, "TestAuthType");
                        });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        // Seed a quote owned by someone else
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<QuotesApi.Data.QuoteDbContext>();
            db.Quotes.Add(new QuotesApi.Models.Quote { Id = 999, Author = "Someone", Text = "Quote", UserId = "real_owner" });
            await db.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme");

        // Act
        var response = await client.DeleteAsync("/api/quotes/999");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
