using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;

namespace QuotesApi.Authorization;

public sealed class OwnsQuoteHandler(
    QuoteDbContext db)
    : AuthorizationHandler<OwnsQuoteRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnsQuoteRequirement requirement)
    {
        Console.WriteLine("OWNERSHIP HANDLER EXECUTED");

        var userIdValue =
            context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        Console.WriteLine($"JWT USER ID: {userIdValue}");

        if (!int.TryParse(
                userIdValue,
                out var userId))
        {
            Console.WriteLine("USER ID PARSE FAILED");
            return;
        }


        if (context.Resource is not HttpContext httpContext)
        {
            Console.WriteLine(
                $"RESOURCE TYPE: {context.Resource?.GetType().Name}");

            return;
        }


        var quoteIdValue =
            httpContext.Request.RouteValues["id"]?.ToString();

        Console.WriteLine($"ROUTE QUOTE ID: {quoteIdValue}");


        if (!int.TryParse(
                quoteIdValue,
                out var quoteId))
        {
            Console.WriteLine("QUOTE ID PARSE FAILED");
            return;
        }


        var ownsQuote =
            await db.Quotes
                .AsNoTracking()
                .AnyAsync(q =>
                    q.Id == quoteId &&
                    q.UserId == userId &&
                    !q.IsDeleted);


        Console.WriteLine($"OWNS QUOTE RESULT: {ownsQuote}");


        if (ownsQuote)
        {
            context.Succeed(requirement);
        }
    }
}