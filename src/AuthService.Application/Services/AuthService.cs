using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Exceptions;
using AuthService.Application.Extensions;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AuthService.Application.DTOs.Email;

namespace AuthService.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    // --- REGISTRO DE ADMINISTRADOR ---
    public async Task<RegisterResponseDto> RegisterAdminAsync(RegisterAdminDto registerDto)
{
    await ValidateEmailUniqueness(registerDto.Email);

    var userId = UuidGenerator.GenerateUserId();
    var emailVerificationToken = TokenGenerator.GenerateEmailVerificationToken();
    var defaultRole = await GetRoleOrThrow(RoleConstants.ADMIN_ROLE);

    var now = DateTime.UtcNow;

    var user = new User
    {
        Id = userId,
        Name = registerDto.Name,
        Surname = registerDto.Surname,
        Email = registerDto.Email.Trim().ToLowerInvariant(),
        Password = passwordHashService.HashPassword(registerDto.Password),

        // Entidades de relación
        UserEmail = CreateUserEmailEntity(userId, emailVerificationToken),
        UserRoles = [CreateUserRoleEntity(userId, defaultRole.Id)]
    };

    var createdUser = await userRepository.CreateAsync(user);
    SendVerificationEmailInBackground(createdUser, emailVerificationToken);

    return BuildRegisterResponse(createdUser, "Administrador registrado exitosamente.");
}

   public async Task<RegisterResponseDto> RegisterUserAsync(RegisterUserDto registerDto)
{
    await ValidateEmailUniqueness(registerDto.Email);

    var userId = UuidGenerator.GenerateUserId();
    var emailVerificationToken = TokenGenerator.GenerateEmailVerificationToken();
    var defaultRole = await GetRoleOrThrow(RoleConstants.USER_ROLE);
    var now = DateTime.UtcNow;

    var user = new User
    {
        Id = userId,
        Name = registerDto.Name,
        Surname = registerDto.Surname,
        Email = registerDto.Email.Trim().ToLowerInvariant(),
        Password = passwordHashService.HashPassword(registerDto.Password),

        CodePhone = registerDto.CodePhone,
        Phone = registerDto.Phone,
        Address = registerDto.Address,
        UsageType = registerDto.UsageType,

        // Configuración de estado y tokens
        AccountStatus = AccountStatus.Pending, // O asignas el Enum directo
        IsEmailVerified = false,
        VerificationToken = emailVerificationToken,
        TokenExpiresAt = now.AddHours(24), // Expira en 24 horas

        // Fechas de auditoría
        CreatedAt = now,
        UpdatedAt = now,
        LastLogin = null,

        // Relaciones (si las mantienes en tu modelo)
        UserEmail = CreateUserEmailEntity(userId, emailVerificationToken),
        UserRoles = [CreateUserRoleEntity(userId, defaultRole.Id)]
    };

    var createdUser = await userRepository.CreateAsync(user);
    SendVerificationEmailInBackground(createdUser, emailVerificationToken);

    return BuildRegisterResponse(createdUser, "Usuario ha sido registrado exitosamente.");
}
    // --- LOGIN ---
    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        User? user = loginDto.EmailOrUsername.Contains('@')
            ? await userRepository.GetByEmailAsync(loginDto.EmailOrUsername.ToLowerInvariant())
            : await userRepository.GetByNameAsync(loginDto.EmailOrUsername);

        if (user == null)
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        if (!user.IsActive)
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("La cuenta de usuario está desactivada");
        }

        if (!passwordHashService.VerifyPassword(loginDto.Password, user.Password))
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }
        
        if (!user.IsEmailVerified)
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("Debe verificar su correo electrónico antes de iniciar sesión.");
        }

        user.LastLogin = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        // Log de verificación
        logger.LogInformation("Valor en memoria de LastLogin: {LastLogin}", user.LastLogin);

        var userDetails = MapToUserDetailsDto(user);
        logger.LogInformation("Valor en DTO de LastLogin: {LastLogin}", userDetails.LastLogin);

        logger.LogUserLoggedIn();

        var token = jwtTokenService.GenerateToken(user);
        var expiryMinutes = int.Parse(configuration["JwtSettings:ExpiryInMinutes"] ?? "30");

        return new AuthResponseDto
        {
            Success = true,
            Message = "Login exitoso",
            Token = token,
            UserDetails = MapToUserDetailsDto(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    // --- MÉTODOS DE EMAIL Y VERIFICACIÓN ---

    public async Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
    {
        var user = await userRepository.GetByEmailVerificationTokenAsync(verifyEmailDto.Token);
        if (user == null || user.UserEmail == null)
            return new EmailResponseDto { Success = false, Message = "Token inválido o expirado" };

        user.IsEmailVerified = true;
        user.UserEmail.EmailVerified = true;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.AccountStatus = AccountStatus.Active;
        user.UserEmail.EmailVerificationToken = null;
        user.UserEmail.EmailVerificationTokenExpiry = null;

        await userRepository.UpdateAsync(user);
        try { await emailService.SendWelcomeEmailAsync(user.Email, user.Name); }
        catch (Exception ex) { logger.LogError(ex, "Error welcome email"); }

        return new EmailResponseDto { Success = true, Message = "Email verificado exitosamente" };
    }

    // --- ESTE ES EL MÉTODO QUE HACÍA FALTA ---
    public async Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto)
    {
        var user = await userRepository.GetByEmailAsync(resendDto.Email);
        
        if (user == null || user.UserEmail == null)
            return new EmailResponseDto { Success = false, Message = "Usuario no encontrado." };

        if (user.UserEmail.EmailVerified)
            return new EmailResponseDto { Success = false, Message = "Este correo ya ha sido verificado." };

        var newToken = TokenGenerator.GenerateEmailVerificationToken();
        user.UserEmail.EmailVerificationToken = newToken;
        user.UserEmail.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        await userRepository.UpdateAsync(user);
        SendVerificationEmailInBackground(user, newToken);

        return new EmailResponseDto { Success = true, Message = "Se ha reenviado el correo de verificación." };
    }

public async Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
{
    var user = await userRepository.GetByEmailAsync(forgotPasswordDto.Email);

    if (user == null)
    {
        return new EmailResponseDto
        {
            Success = true,
            Message = "Si el email existe, se envió un enlace"
        };
    }

    var resetToken = TokenGenerator.GeneratePasswordResetToken();

    if (user.UserPasswordReset == null)
    {
        user.UserPasswordReset = new UserPasswordReset
        {
            Id = UuidGenerator.GenerateUserId(), 
            UserId = user.Id,
            PasswordResetToken = resetToken,
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1)
        };
    }
    else
    {
        user.UserPasswordReset.PasswordResetToken = resetToken;
        user.UserPasswordReset.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
    }

    await userRepository.UpdateAsync(user);

    try
    {
        await emailService.SendPasswordResetAsync(user.Email, user.Name, resetToken);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error reset email");
    }

    return new EmailResponseDto
    {
        Success = true,
        Message = "Si el email existe, se envió un enlace"
    };
}

    public async Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(resetPasswordDto.Token);
        if (user == null || user.UserPasswordReset == null)
            return new EmailResponseDto { Success = false, Message = "Token inválido" };

        user.Password = passwordHashService.HashPassword(resetPasswordDto.NewPassword);
        user.UserPasswordReset.PasswordResetToken = null;
        user.UserPasswordReset.PasswordResetTokenExpiry = null;

        await userRepository.UpdateAsync(user);
        return new EmailResponseDto { Success = true, Message = "Contraseña actualizada" };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user == null ? null : MapToUserResponseDto(user);
    }

    // --- MÉTODOS PRIVADOS DE APOYO ---

    private async Task ValidateEmailUniqueness(string email)
    {
        if (await userRepository.ExistsByEmailAsync(email))
        {
            logger.LogRegistrationWithExistingEmail();
            throw new BusinessException(ErrorCodes.EMAIL_ALREADY_EXISTS, "El email ya existe");
        }
    }

    private async Task<Role> GetRoleOrThrow(string roleName)
    {
        var role = await roleRepository.GetByNameAsync(roleName);
        return role ?? throw new InvalidOperationException($"Rol '{roleName}' no encontrado.");
    }

    private UserEmail CreateUserEmailEntity(string userId, string token) => new()
    {
        Id = UuidGenerator.GenerateShortUUID(), 
        UserId = userId,
        EmailVerified = false,
        EmailVerificationToken = token,
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
    };

    private Domain.Entities.UserRole CreateUserRoleEntity(string userId, string roleId) => new()
    {
        Id = UuidGenerator.GenerateShortUUID(),
        UserId = userId,
        RoleId = roleId
    };

    private void SendVerificationEmailInBackground(User user, string token)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await emailService.SendEmailVerificationAsync(user.Email, user.Name, token);
                logger.LogInformation("Email de verificación enviado");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al enviar email de verificación a {Email}", user.Email);
            }
        });
    }

    private RegisterResponseDto BuildRegisterResponse(User user, string message) => new()
    {
        Success = true,
        User = MapToUserResponseDto(user),
        Message = $"{message} Por favor, verifica tu email para activar la cuenta.",
        EmailVerificationRequired = true
    };

    private UserResponseDto MapToUserResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Phone = user.Phone ?? string.Empty,
            Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE,
            IsActive = user.IsActive,
            IsEmailVerified = user.UserEmail?.EmailVerified ?? false,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLogin = user.LastLogin
        };
    }

    private UserDetailsDto MapToUserDetailsDto(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE
    };
}