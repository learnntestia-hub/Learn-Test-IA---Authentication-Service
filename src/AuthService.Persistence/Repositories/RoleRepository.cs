using AuthService.Domain.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Repositories;

public class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await context.Roles
        .Include(r => r.UserRoles) // Se incluye la colección de UserRoles relacionada con el rol
        .FirstOrDefaultAsync(r => r.Name == roleName); // Se busca el rol por su nombre utilizando FirstOrDefaultAsync, lo que devuelve null si no se encuentra ningún rol con ese nombre
    }

    public async Task<int> CountUsersInRoleAsync(string roleName)
    {
        return await context.UserRoles
        .Where(ur => ur.Role.Name == roleName) // Se filtran los UserRoles para contar solo aquellos que están asociados con el rol especificado por su nombre
        .CountAsync(); // Se cuenta el número de UserRoles que cumplen con la condición utilizando
    }

    public async Task<IReadOnlyList<User>> GetUsersByRoleAsync(string roleName)
{
    // 1. Empezamos directamente desde los Usuarios
    var users = await context.Users
        .Include(u => u.UserProfile) // Incluye el perfil directamente
        .Include(u => u.UserEmail)   // Incluye el email directamente
        .Include(u => u.UserRoles)   // Incluye la tabla intermedia de roles
            .ThenInclude(ur => ur.Role) // Incluye el detalle del rol de esa intermedia
        .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName)) // 2. Filtramos los usuarios que pertenezcan a ese rol
        .ToListAsync();

    // 3. Retornamos la lista directamente (C# hace el cast implícito a IReadOnlyList)
    return users;
}

    public async Task<IReadOnlyList<string>> GetUserRoleNameAsync(string userId)
    {
        return await context.UserRoles
        .Where(ur => ur.UserId == userId) // Se filtran los UserRoles para obtener solo aquellos que están asociados con el usuario especificado por su ID
        .Select(ur => ur.Role.Name) // Se seleccionan los nombres de los roles relacionados con los UserRoles filtrados utilizando Select para proyectar solo la propiedad Name de cada Role relacionada con los UserRoles
        .ToListAsync() // Se convierte el resultado a una lista utilizando ToListAsync, lo que devuelve una lista de nombres de roles que cumplen con la condición de estar asociados con el usuario especificado por su ID
        .ContinueWith(t => (IReadOnlyList<string>)t.Result); // Se convierte el resultado a IReadOnlyList<string> utilizando ContinueWith para proyectar el resultado de la tarea a un tipo de lista de solo lectura
    }
}