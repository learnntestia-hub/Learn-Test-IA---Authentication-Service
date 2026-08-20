using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs;

public class RegisterAdminDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Surname { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}