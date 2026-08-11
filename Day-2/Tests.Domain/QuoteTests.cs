using FluentAssertions;
using QuotesApi.Models;

namespace Tests.Domain;

public class QuoteTests
{
    [Fact]
    public void Empty_author_should_throw()
    {
        Action act = () =>
            Quote.Create("", "Valid quote text");

        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Author_longer_than_200_characters_should_throw()
    {
        var author = new string('a', 201);

        Action act = () =>
            Quote.Create(author, "Valid quote text");

        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Empty_text_should_throw()
    {
        Action act = () =>
            Quote.Create("Author", "");

        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Text_longer_than_1000_characters_should_throw()
    {
        var text = new string('a', 1001);

        Action act = () =>
            Quote.Create("Author", text);

        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Valid_quote_should_be_created()
    {
        var quote = Quote.Create(
            "Albert Einstein",
            "Life is like riding a bicycle.");

        quote.Author.Should()
            .Be("Albert Einstein");

        quote.Text.Should()
            .Be("Life is like riding a bicycle.");

        quote.IsDeleted.Should()
            .BeFalse();
    }

    [Fact]
    public void Soft_delete_should_mark_quote_as_deleted()
    {
        var quote = Quote.Create(
            "Author",
            "Quote text");

        quote.SoftDelete();

        quote.IsDeleted.Should()
            .BeTrue();
    }
}