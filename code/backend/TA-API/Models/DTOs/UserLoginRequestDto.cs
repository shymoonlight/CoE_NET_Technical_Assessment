namespace TA_API.Models.DTOs
{
    public class UserLoginRequestDto
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
