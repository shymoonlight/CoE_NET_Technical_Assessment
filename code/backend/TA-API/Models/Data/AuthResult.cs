using TA_API.Models.DTOs;

namespace TA_API.Models.Data
{
    public class AuthResult
    {
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public UserResponseDto User { get; set; } = null!;
    }
}
