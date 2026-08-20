using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Interfaces;

public interface IUserRepository
{
    // Métodos de creación y mutación
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(string id);
    Task UpdateUserRoleAsync(string userId, string roleId);

    // Métodos de consulta por identificadores y credenciales
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByNameAsync(string name);
    Task<User?> GetBySurnameAsync(string surname);
    Task<User?> GetByPhoneAsync(string phone);
    Task<User?> GetByAddressAsync(string address);

    // Métodos de consulta por tokens y estados
    Task<User?> GetByVerificationTokenAsync(string token);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task<IReadOnlyList<User>> GetByUsageTypeAsync(UsageType usageType);
    Task<IReadOnlyList<User>> GetByAccountStatusAsync(AccountStatus accountStatus);
    Task<User?> GetByEmailVerificationTokenAsync(string token);

    // Métodos de existencia / validación rápida
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsBySurnameAsync(string surname);
    Task<bool> ExistsByPhoneAsync(string phone);
}