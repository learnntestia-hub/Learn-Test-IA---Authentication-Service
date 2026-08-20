using System.ComponentModel.DataAnnotations;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(5)]
    public string? CodePhone { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public string? Address { get; set; }

    [Required]
    public UsageType UsageType { get; set; }

    [Required]
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Pending;

    public bool IsEmailVerified { get; set; } = false;
    public bool IsActive { get; set; } = false;

    [MaxLength(255)]
    public string? VerificationToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLogin { get; set; }

    // Relaciones
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public UserEmail UserEmail { get; set; } = null!;
    public UserPasswordReset UserPasswordReset { get; set; } = null!;
    public UserProfile UserProfile { get; set; } = null!;
}