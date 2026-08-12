using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace QuotesApi.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddQuotesAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "BearerOrEntra";
            options.DefaultChallengeScheme = "BearerOrEntra";
        })
        .AddJwtBearer("LocalJwt", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "super-secret-key-that-is-at-least-32-chars!"))
            };
        })
        .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"), "EntraId")
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("BearerOrEntra", policy =>
            {
                policy.AuthenticationSchemes.Add("LocalJwt");
                policy.AuthenticationSchemes.Add("EntraId");
                policy.RequireAuthenticatedUser();
            });

            options.AddPolicy("MutatingPolicy", policy =>
            {
                policy.AuthenticationSchemes.Add("LocalJwt");
                policy.AuthenticationSchemes.Add("EntraId");
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "Quotes.Write");
            });
            
            options.DefaultPolicy = options.GetPolicy("BearerOrEntra")!;
        });

        return services;
    }
}
