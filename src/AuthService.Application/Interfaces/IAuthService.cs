using AuthService.Application.DTOs;
using AuthService.Application.DTOs.Email;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAdminAsync(RegisterAdminDto registerAdminDto);
    Task<RegisterResponseDto> RegisterUserAsync(RegisterUserDto registerUserDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
    Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto);
    Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<UserResponseDto?> GetUserByIdAsync(string userId);
    Task<UserBlockResponseDto?> BlockUserAsync(string userId);
    Task<UserUnBlockResponseDto?> UnBlockUserAsync (string userId);
}