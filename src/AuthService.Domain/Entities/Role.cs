using System.ComponentModel.DataAnnotations;

namespace AuthService.Domain.Entities;

public class Role
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } = string.Empty;
 
    [Required(ErrorMessage = "El nombre del rol es requerido")]
    [MaxLength(100,ErrorMessage = "El nombre del rol debe tener menos de 100 caracteres")]
    public string Name { get; set; } = string.Empty;
   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
 
    //Relaciones con UserRole
    public ICollection<UserRole> UserRoles { get; set; }
}