using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public class UserResponseUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CodePhone { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public UsageType UsageType { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}