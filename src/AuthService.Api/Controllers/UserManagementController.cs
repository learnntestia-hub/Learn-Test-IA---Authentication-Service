using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService.Api.Controllers;

/// <summary>
/// Controlador de gestión de usuarios para LearnTestIA.
/// Expone endpoints para listar administradores y usuarios registrados.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    /// <summary>
    /// Obtiene la lista de todos los administradores registrados.
    /// </summary>
    /// <response code="200">Lista de administradores obtenida exitosamente.</response>
    [HttpGet("admins")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetAdmins()
    {
        var admins = await userManagementService.GetAdminsAsync();

        return Ok(new
        {
            success = true,
            message = "Administradores obtenidos exitosamente",
            data = admins
        });
    }

    /// <summary>
    /// Obtiene la lista de todos las cuentas registradas en formato de Estudiante, Personal o Empresarial.
    /// </summary>
    /// <response code="200">Lista de usuarios obtenida exitosamente.</response>
    [HttpGet("users")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetUsers()
    {
        var users = await userManagementService.GetUsersAsync();

        return Ok(new
        {
            success = true,
            message = "Usuarios obtenidos exitosamente",
            data = users
        });
    }
}