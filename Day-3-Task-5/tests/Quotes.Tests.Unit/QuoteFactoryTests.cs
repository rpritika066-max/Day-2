using FluentAssertions;
using QuotesApi.Models;
using System;
using Xunit;

namespace Quotes.Tests.Unit.Models;

public class QuoteFactoryTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsQuote()
    {
        // Arrange
        var author = "Test Author";
        var text = "Test Text";
        var userId = "user123";

        // Act
        var result = Quote.Create(author, text, userId);

        // Assert
        result.Should().NotBeNull();
        result.Author.Should().Be(author);
        result.Text.Should().Be(text);
        result.UserId.Should().Be(userId);
    }

    [Theory]
    [InlineData("", "Valid text", "user123", "author")]
    [InlineData(" ", "Valid text", "user123", "author")]
    [InlineData(null, "Valid text", "user123", "author")]
    [InlineData("Valid author", "", "user123", "text")]
    [InlineData("Valid author", " ", "user123", "text")]
    [InlineData("Valid author", null, "user123", "text")]
    [InlineData("Valid author", "Valid text", "", "userId")]
    [InlineData("Valid author", "Valid text", " ", "userId")]
    [InlineData("Valid author", "Valid text", null, "userId")]
    public void Create_InvalidInputs_ThrowsArgumentException(string author, string text, string userId, string expectedParamName)
    {
        // Act
        Action act = () => Quote.Create(author, text, userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName(expectedParamName);
    }
}
