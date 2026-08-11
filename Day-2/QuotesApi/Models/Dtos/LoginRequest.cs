namespace QuotesApi.Models.Dtos;

public record LoginRequest(
    string Email,
    string Password);