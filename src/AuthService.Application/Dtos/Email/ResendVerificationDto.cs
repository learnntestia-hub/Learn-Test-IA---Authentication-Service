using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Email;

public class ResendVerificationDto
{
    [Required(ErrorMessage = "El email es requerido.")]
    public string Email { get; set; } = string.Empty;
}