using FluentAssertions;
using QuotesApi.Models.Dtos;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Quotes.Tests.Unit.Validators;

public class RequestValidatorTests
{
    private static bool ValidateModel(object model, out List<ValidationResult> results)
    {
        var ctx = new ValidationContext(model);
        results = new List<ValidationResult>();
        return Validator.TryValidateObject(model, ctx, results, true);
    }

    [Fact]
    public void CreateQuoteRequest_ValidModel_ReturnsTrue()
    {
        // Arrange
        var request = new CreateQuoteRequest { Author = "Valid Author", Text = "Valid Text" };

        // Act
        var isValid = ValidateModel(request, out var results);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "Valid Text", "Author")]
    [InlineData("", "Valid Text", "Author")]
    [InlineData("Valid Author", null, "Text")]
    [InlineData("Valid Author", "", "Text")]
    public void CreateQuoteRequest_MissingFields_ReturnsFalse(string? author, string? text, string expectedErrorField)
    {
        // Arrange
        var request = new CreateQuoteRequest { Author = author!, Text = text! };

        // Act
        var isValid = ValidateModel(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(expectedErrorField);
    }

    [Fact]
    public void CreateQuoteRequest_AuthorTooLong_ReturnsFalse()
    {
        // Arrange
        var request = new CreateQuoteRequest { Author = new string('A', 101), Text = "Valid Text" };

        // Act
        var isValid = ValidateModel(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain("Author");
    }

    [Fact]
    public void CreateQuoteRequest_TextTooLong_ReturnsFalse()
    {
        // Arrange
        var request = new CreateQuoteRequest { Author = "Valid Author", Text = new string('A', 1001) };

        // Act
        var isValid = ValidateModel(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain("Text");
    }

    [Fact]
    public void CreateCollectionRequest_ValidModel_ReturnsTrue()
    {
        // Arrange
        var request = new CreateCollectionRequest { Name = "My Collection", OwnerId = "user123" };

        // Act
        var isValid = ValidateModel(request, out var results);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "user123", "Name")]
    [InlineData("", "user123", "Name")]
    [InlineData("My Collection", null, "OwnerId")]
    [InlineData("My Collection", "", "OwnerId")]
    public void CreateCollectionRequest_MissingFields_ReturnsFalse(string? name, string? ownerId, string expectedErrorField)
    {
        // Arrange
        var request = new CreateCollectionRequest { Name = name!, OwnerId = ownerId! };

        // Act
        var isValid = ValidateModel(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(expectedErrorField);
    }

    [Fact]
    public void CreateCollectionRequest_NameTooLong_ReturnsFalse()
    {
        // Arrange
        var request = new CreateCollectionRequest { Name = new string('A', 101), OwnerId = "user123" };

        // Act
        var isValid = ValidateModel(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain("Name");
    }
}
