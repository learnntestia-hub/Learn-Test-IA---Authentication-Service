using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using AuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    // =========================================================================
    // Métodos de creación y mutación
    // =========================================================================

    public async Task<User> CreateAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return (await GetByIdAsync(user.Id))!;
    }

    public async Task<User> UpdateAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return (await GetByIdAsync(user.Id))!;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return false;

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateUserRoleAsync(string userId, string roleId)
    {
        var existingRoles = await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        context.UserRoles.RemoveRange(existingRoles);

        var newUserRole = new AuthService.Domain.Entities.UserRole
        {
            Id = UuidGenerator.GenerateUserId(),
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.UserRoles.Add(newUserRole);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // Métodos de consulta por identificadores y credenciales
    // =========================================================================

    public async Task<User?> GetByIdAsync(string id)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email));
    }

    public async Task<User?> GetByNameAsync(string name)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Name, name));
    }

    public async Task<User?> GetBySurnameAsync(string surname)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Surname, surname));
    }

    public async Task<User?> GetByPhoneAsync(string phone)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(u => u.Phone != null && EF.Functions.ILike(u.Phone, phone));
    }

    public async Task<User?> GetByAddressAsync(string address)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(u => u.Address != null && EF.Functions.ILike(u.Address, address));
    }

    // =========================================================================
    // Métodos de consulta por tokens y estados
    // =========================================================================

    public async Task<User?> GetByVerificationTokenAsync(string token)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(u => u.VerificationToken == token);
    }

    public async Task<User?> GetByEmailVerificationTokenAsync(string token)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(u => u.UserEmail != null &&
                                      u.UserEmail.EmailVerificationToken == token);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(u => u.UserPasswordReset != null &&
                                      u.UserPasswordReset.PasswordResetToken == token);
    }

    public async Task<IReadOnlyList<User>> GetByUsageTypeAsync(UsageType usageType)
    {
        return await BaseQuery()
            .Where(u => u.UsageType == usageType)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<User>> GetByAccountStatusAsync(AccountStatus accountStatus)
    {
        return await BaseQuery()
            .Where(u => u.AccountStatus == accountStatus)
            .ToListAsync();
    }

    // =========================================================================
    // Métodos de existencia / validación rápida
    // =========================================================================

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await context.Users
            .AnyAsync(u => EF.Functions.ILike(u.Email, email));
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await context.Users
            .AnyAsync(u => EF.Functions.ILike(u.Name, name));
    }

    public async Task<bool> ExistsBySurnameAsync(string surname)
    {
        return await context.Users
            .AnyAsync(u => EF.Functions.ILike(u.Surname, surname));
    }

    public async Task<bool> ExistsByPhoneAsync(string phone)
    {
        return await context.Users
            .AnyAsync(u => u.Phone != null && EF.Functions.ILike(u.Phone, phone));
    }

    // =========================================================================
    // Helper privado para reutilizar los Include
    // =========================================================================

    private IQueryable<User> BaseQuery()
    {
        return context.Users
            .Include(u => u.UserProfile)
            .Include(u => u.UserEmail)
            .Include(u => u.UserPasswordReset)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);
    }
}