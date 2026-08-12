using FluentAssertions;
using QuotesApi.Models;

namespace Tests.Domain;

public class CollectionTests
{
    [Fact]
    public void Empty_name_should_throw()
    {
        Action act = () =>
            new Collection("", "owner");

        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Name_longer_than_80_characters_should_throw()
    {
        var name = new string('a', 81);

        Action act = () =>
            new Collection(name, "owner");

        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Adding_51st_item_should_throw()
    {
        var collection = new Collection(
            "My Collection",
            "owner");

        for (int i = 1; i <= 50; i++)
            collection.AddItem(i, DateTimeOffset.UtcNow);

        Action act = () =>
            collection.AddItem(51, DateTimeOffset.UtcNow);

        act.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Duplicate_quote_id_should_throw()
    {
        var collection = new Collection(
            "My Collection",
            "owner");

        collection.AddItem(1, DateTimeOffset.UtcNow);

        Action act = () =>
            collection.AddItem(1, DateTimeOffset.UtcNow);

        act.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Removing_missing_item_should_throw()
    {
        var collection = new Collection(
            "My Collection",
            "owner");

        Action act = () =>
            collection.RemoveItem(99);

        act.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Adding_then_removing_should_leave_zero_items()
    {
        var collection = new Collection(
            "My Collection",
            "owner");

        collection.AddItem(
            1,
            DateTimeOffset.UtcNow);

        collection.RemoveItem(1);

        collection.Items.Should()
            .BeEmpty();
    }
}