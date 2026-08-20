using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AuthService.Domain.Entities;

    public class UserProfile
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; }

    [Required]
    [MaxLength(16)]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public string ProfilePicture { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}