using System;

namespace TA_API.Models.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}