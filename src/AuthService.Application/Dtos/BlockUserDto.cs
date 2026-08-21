using System.ComponentModel.DataAnnotations;
namespace AuthService.Application.DTOs;

public class BlockUserDto {
    
    [Required(ErrorMessage = "El userId es requerido")]
    public string UserId { get; set; } = string.Empty;

}