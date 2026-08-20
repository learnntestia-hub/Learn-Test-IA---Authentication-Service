using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Services;

public class UserManagementService(IUserRepository users, IRoleRepository roles) : IUserManagementService
{
    public async Task<UserResponseDto> UpdateUserRoleAsync(string userId, string roleName)
    {
        roleName = roleName?.Trim().ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("Invalid userId", nameof(userId));
        
        if (!RoleConstants.AllowedRoles.Contains(roleName))
            throw new InvalidOperationException($"Rol no permitido. Use: {string.Join(", ", RoleConstants.AllowedRoles)}");

        var user = await users.GetByIdAsync(userId) 
                   ?? throw new KeyNotFoundException("Usuario no encontrado");

        var isUserAdmin = user.UserRoles.Any(r => r.Role.Name == RoleConstants.ADMIN_ROLE);
        if (isUserAdmin && roleName != RoleConstants.ADMIN_ROLE)
        {
            var adminCount = await roles.CountUsersInRoleAsync(RoleConstants.ADMIN_ROLE);
            if (adminCount <= 1)
            {
                throw new InvalidOperationException("No se puede eliminar el último administrador del sistema.");
            }
        }

        var role = await roles.GetByNameAsync(roleName)
                    ?? throw new InvalidOperationException($"Rol {roleName} no encontrado en la base de datos");

        await users.UpdateUserRoleAsync(userId, role.Id);

        user = await users.GetByIdAsync(userId)
               ?? throw new KeyNotFoundException("Usuario no encontrado");

        return MapToResponse(user, role.Name);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(string userId)
    {
        return await roles.GetUserRoleNameAsync(userId);
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetUsersByRoleAsync(string roleName)
    {
        roleName = roleName?.Trim().ToUpperInvariant() ?? string.Empty;
        var usersInRole = await roles.GetUsersByRoleAsync(roleName);
        
        return usersInRole.Select(u => MapToResponse(u, roleName)).ToList();
    }

    public async Task<IReadOnlyList<UserResponseAdminDto>> GetAdminsAsync()
    {
        var admins = await roles.GetUsersByRoleAsync(RoleConstants.ADMIN_ROLE);
        
        return admins.Select(u => new UserResponseAdminDto
        {
            Id = u.Id,
            Name = u.Name,
            Surname = u.Surname,
            Email = u.Email,
            Role = RoleConstants.ADMIN_ROLE,
            IsEmailVerified = u.IsEmailVerified,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();
    }

    public async Task<IReadOnlyList<UserResponseUserDto>> GetUsersAsync()
    {
        var users = await roles.GetUsersByRoleAsync(RoleConstants.USER_ROLE);
        
        return users.Select(u => new UserResponseUserDto
        {
            Id = u.Id,
            Name = u.Name,
            Surname = u.Surname,
            Email = u.Email,
            Address = u.Address ?? string.Empty,
            Phone = u.Phone ?? string.Empty,
            UsageType = u.UsageType,
            AccountStatus = u.AccountStatus,
            Role = RoleConstants.USER_ROLE,
            IsEmailVerified = u.IsEmailVerified,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();
    }

    // --- MAPEO PRIVADO ---
    private static UserResponseDto MapToResponse(User u, string roleName)
    {
        return new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Surname = u.Surname, 
            Email = u.Email,
            Address = u.Address ?? string.Empty,
            Phone = u.Phone ?? string.Empty,
            UsageType = u.UsageType,
            AccountStatus = u.AccountStatus,
            Role = roleName,
            IsEmailVerified = u.IsEmailVerified,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        };
    }
}