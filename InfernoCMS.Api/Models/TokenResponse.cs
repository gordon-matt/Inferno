namespace InfernoCMS.Api.Models
{
    public class TokenResponse
    {
        public string Token { get; set; }

        public string TokenType { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}