using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public class UserBlockResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UsageType UsageType { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}