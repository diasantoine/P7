namespace P7CreateRestApi.Models
{
    public record LoginResponse(string Token, DateTime ExpiresAt);
}
