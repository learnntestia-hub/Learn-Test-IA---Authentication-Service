using AuthService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AuthService.Application.DTOs;

public class RegisterUserDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Surname { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;

    [Required] public string CodePhone { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    [Required] public string Address { get; set; } = string.Empty;

    // Se restringe a los valores del Enum para el tipo de cuenta como (Personal, Estudiante o Empresarial)
    [Required] 
    [EnumDataType(typeof(UsageType), ErrorMessage = "El tipo de uso no es válido.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UsageType UsageType { get; set; }
}