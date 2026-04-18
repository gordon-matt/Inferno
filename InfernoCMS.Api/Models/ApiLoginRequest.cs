using System.ComponentModel.DataAnnotations;

namespace InfernoCMS.Api.Models
{
    public class ApiLoginRequest
    {
        [Required]
        public string ApiKey { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; }

        [Required]
        [StringLength(256, MinimumLength = 1)]
        public string Password { get; set; }
    }
}