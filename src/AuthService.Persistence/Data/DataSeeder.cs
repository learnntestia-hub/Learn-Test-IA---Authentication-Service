using AuthService.Domain.Entities;
using AuthService.Domain.Constants;
using AuthService.Domain.Enums;
using AuthService.Application.Services;
using Microsoft.EntityFrameworkCore;
using UserRoleEntity = AuthService.Domain.Entities.UserRole;

namespace AuthService.Persistence.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new()
                {
                    Id = UuidGenerator.GenerateRoleId(),
                    Name = RoleConstants.ADMIN_ROLE
                },
                new()
                {
                    Id = UuidGenerator.GenerateRoleId(),
                    Name = RoleConstants.USER_ROLE
                }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == RoleConstants.ADMIN_ROLE);

            if (adminRole is not null)
            {
                var passwordHasher = new PasswordHashService();

                var userId = UuidGenerator.GenerateUserId();
                var profileId = UuidGenerator.GenerateUserId();
                var emailId = UuidGenerator.GenerateUserId();
                var userRoleId = UuidGenerator.GenerateUserId();
                var now = DateTime.UtcNow;

                var adminUser = new User
                {
                    Id = userId,
                    Name = "Admin",
                    Surname = "User",
                    Email = "admin@ksports.local",
                    Password = passwordHasher.HashPassword("Admin1234!"),
                    
                    // Campos de contacto y dirección
                    CodePhone = "+502",
                    Phone = "00000000",
                    Address = "Guatemala",

                    // Campos de negocio y estado
                    UsageType = UsageType.Business,
                    AccountStatus = AccountStatus.Active,
                    IsEmailVerified = true,
                    VerificationToken = null,
                    TokenExpiresAt = null,

                    // Fechas de auditoría
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastLogin = now,

                    UserProfile = new UserProfile
                    {
                        Id = profileId,
                        UserId = userId,
                        ProfilePicture = string.Empty
                    },

                    UserEmail = new UserEmail
                    {
                        Id = emailId,
                        UserId = userId,
                        EmailVerified = true,
                        EmailVerificationToken = null,
                        EmailVerificationTokenExpiry = null
                    },

                    UserRoles =
                    [
                        new UserRoleEntity
                        {
                            Id = userRoleId,
                            UserId = userId,
                            RoleId = adminRole.Id,
                            AssignedAt = now,
                            CreatedAt = now,
                            UpdatedAt = now
                        }
                    ]
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}