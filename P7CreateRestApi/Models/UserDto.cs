namespace P7CreateRestApi.Models
{
    // What the API answers about a user: never the password, not even its hash.
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Fullname { get; set; }
        public string? Role { get; set; }
    }
}
