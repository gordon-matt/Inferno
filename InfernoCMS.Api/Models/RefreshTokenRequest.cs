using System.ComponentModel.DataAnnotations;

namespace InfernoCMS.Api.Models
{
    public class RefreshTokenRequest
    {
        [Required]
        public string ApiKey { get; set; }

        /// <summary>
        /// The existing (possibly expired) JWT issued by a previous call to /auth/login.
        /// </summary>
        [Required]
        public string Token { get; set; }
    }
}