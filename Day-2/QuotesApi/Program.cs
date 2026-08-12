using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuotesApi.Authorization;
using QuotesApi.Data;
using QuotesApi.Extensions;
using QuotesApi.Repositories;
using QuotesApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Database
// ------------------------------------------------------------

builder.Services.AddDbContext<QuoteDbContext>(options =>
{
    options.UseSqlite("Data Source=quotes.db");
});

// ------------------------------------------------------------
// Repositories
// ------------------------------------------------------------

builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();

// ------------------------------------------------------------
// Services
// ------------------------------------------------------------

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IClock, SystemClock>();

// ------------------------------------------------------------
// Authentication
// ------------------------------------------------------------

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT key missing from configuration.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });

// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------

builder.Services.AddAuthorization(options =>
{
    // Claim-based policy.
    //
    // User must have:
    // scope = quotes.write
    options.AddPolicy(
        "can-edit-quotes",
        policy =>
        {
            policy.RequireClaim(
                "scope",
                "quotes.write");
        });

    // Custom policy.
    //
    // User must own the quote they are
    // attempting to delete.
    options.AddPolicy(
        "can-delete-own-quote",
        policy =>
        {
            policy.AddRequirements(
                new OwnsQuoteRequirement());
        });
});

// Register custom authorization handler.
builder.Services.AddScoped<OwnsQuoteHandler>();

// ------------------------------------------------------------
// Application
// ------------------------------------------------------------

var app = builder.Build();

// ------------------------------------------------------------
// Middleware
// ------------------------------------------------------------

app.UseAuthentication();

app.UseAuthorization();

// ------------------------------------------------------------
// Endpoints
// ------------------------------------------------------------

app.MapAuthEndpoints();

app.MapQuoteEndpoints();

// ------------------------------------------------------------
// Run
// ------------------------------------------------------------

app.Run();