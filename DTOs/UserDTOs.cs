using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto();
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public string Role { get; set; } = "Employee";
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Role { get; set; }
        public bool? Status { get; set; }
    }
}
