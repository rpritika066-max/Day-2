namespace QuotesApi.Models;

public class Quote
{
    private Quote()
    {
    }

    private Quote(
        string author,
        string text,
        int userId)
    {
        Author = author;
        Text = text;
        UserId = userId;
    }

    public int Id { get; private set; }

    public string Author { get; private set; } = string.Empty;

    public string Text { get; private set; } = string.Empty;

    public bool IsDeleted { get; private set; }

    public int UserId { get; private set; }

    public User User { get; private set; } = null!;

    public static Quote Create(
        string author,
        string text,
        int userId)
    {
        if (string.IsNullOrWhiteSpace(author) ||
            author.Length > 200)
        {
            throw new ArgumentException(
                "Author must be between 1 and 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(text) ||
            text.Length > 1000)
        {
            throw new ArgumentException(
                "Text must be between 1 and 1000 characters.");
        }

        if (userId <= 0)
        {
            throw new ArgumentException(
                "User ID must be valid.");
        }

        return new Quote(
            author,
            text,
            userId);
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}