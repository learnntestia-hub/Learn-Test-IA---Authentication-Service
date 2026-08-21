using System;
using AuthService.Application.DTOs;
using AuthService.Application.DTOs.Email;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService.Api.Controllers;

/// <summary>
/// Controlador de identidad para LearnTestIA
/// Gestiona el registro de Administradores, cuentas personales, estudiantiles y Empresariales
/// </summary>

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController ( IAuthService authService) : ControllerBase {
        /// <summary>
        /// Recupera el perfil del usuario autenticado mediante el token JWT.
        /// </summary>
        /// <response code="200">Perfil recuperado con exito.</response>
        /// <response code="401">Token ausente o no valido.</response>
        /// <response code="404">El usuario no existe en la base de datos.</response>  
    
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetProfile()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c =>
            c.Type == "sub" ||
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            return Unauthorized();

        var user = await authService.GetUserByIdAsync(userIdClaim.Value);
        if (user == null)
            return NotFound();

        return Ok(new
        {
            success = true,
            message = "Perfil obtenido exitosamente",
            data = user
        });
    }

/// ---------------------------------------------------------------------

    /// <summary>
    /// Busca un perfil de usuario por su ID unico.
    /// </summary>
    /// <param name="request">DTO que contiene el identificador del usuario.</param>
    /// <response code="200">Datos del usuario encontrados.</response>
    /// <response code="400">Si el UserId no fue proporcionado.</response>
    /// <response code="404">Si el usuario no fue localizado.</response>
    [HttpPost("profile/by-id")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetProfileById([FromBody] GetProfileByIdDto request)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return BadRequest(new
            {
                success = false,
                message = "El userId es requerido"
            });
        }

        var user = await authService.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Usuario no encontrado"
            });
        }

        return Ok(new
        {
            success = true,
            message = "Perfil obtenido exitosamente",
            data = user
        });
    }

    /// <summary>
    /// Registra a un nuevo Usuario en la plataforma LearnTestIA.
    /// </summary>
    /// <remarks>
    /// Este endpoint utiliza **Multipart Form Data**.
    /// </remarks>
    /// <param name="registerDto">Informacion detallada del agricultor.</param>
    /// <response code="201">Agricultor registrado exitosamente.</response>
    [HttpPost("register")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromForm] RegisterUserDto registerDto)
    {
        var result = await authService.RegisterUserAsync(registerDto);
        return StatusCode(201, result);
    }

    /// <summary>
    /// Registra a un nuevo Administrador del sistema.
    /// </summary>
    /// <remarks>
    /// Requiere privilegios previos para ser invocado en entornos productivos.
    /// </remarks>
    /// <param name="registerDto">Datos basicos del administrador.</param>
    /// <response code="201">Administrador creado correctamente.</response>
    [HttpPost("register/admin")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RegisterResponseDto>> RegisterAdmin([FromForm] RegisterAdminDto registerDto)
    {
        var result = await authService.RegisterAdminAsync(registerDto);
        return StatusCode(201, result);
    }

    /// <summary>
    /// Autentica a un usuario y genera un token de acceso.
    /// </summary>
    /// <param name="loginDto">Credenciales de acceso (Email y Contraseña).</param>
    /// <response code="200">Login exitoso. Retorna el token JWT y datos basicos del usuario.</response>
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await authService.LoginAsync(loginDto);
        return Ok(result);
    }

    /// <summary>
    /// Verifica la cuenta mediante el codigo enviado al correo electronico.
    /// </summary>
    [HttpPost("verify-email")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmailResponseDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        var result = await authService.VerifyEmailAsync(verifyEmailDto);
        return Ok(result);
    }


    /// <summary>
    /// Reenvia un nuevo codigo de verificacion a la direccion de correo proporcionada.
    /// </summary>
    /// <response code="200">Codigo reenviado con exito.</response>
    /// <response code="400">Si el usuario ya esta verificado.</response>
    /// <response code="404">Si el correo no pertenece a ningun usuario.</response>
    /// <response code="503">Error en el servicio de mensajeria.</response>
    [HttpPost("resend-verification")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<EmailResponseDto>> ResendVerification([FromBody] ResendVerificationDto resendDto)
    {
        var result = await authService.ResendVerificationEmailAsync(resendDto);

        if (!result.Success)
        {
            if (result.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            if (result.Message.Contains("ya ha sido verificado", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("ya verificado", StringComparison.OrdinalIgnoreCase))
                return BadRequest(result);

            return StatusCode(503, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Inicia el proceso de recuperacion de contraseña enviando un token al correo.
    /// </summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<EmailResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await authService.ForgotPasswordAsync(forgotPasswordDto);

        if (!result.Success)
            return StatusCode(503, result);

        return Ok(result);
    }

    /// <summary>
    /// Establece una nueva contraseña utilizando el token de recuperacion.
    /// </summary>
    [HttpPost("reset-password")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmailResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await authService.ResetPasswordAsync(resetPasswordDto);
        return Ok(result);
    }

    /// <summary>
    /// Bloquea la cuenta de un usuario.
    /// </summary>
    /// <param name="request">DTO con el ID del usuario a bloquear.</param>
    /// <response code="200">Usuario bloqueado exitosamente.</response>
    /// <response code="400">Si el UserId no fue proporcionado.</response>
    /// <response code="404">Si el usuario no existe.</response>
    [HttpPost("block")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> BlockUser([FromBody] BlockUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new
            {
                success = false,
                message = "El userId es requerido"
            });
        }

        try
        {
            var response = await authService.BlockUserAsync(request.UserId);
            
            return Ok(new
            {
                success = true,
                message = "Usuario bloqueado exitosamente",
                data = response
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Desbloquea la cuenta de un usuario.
    /// </summary>
    /// <param name="request">DTO con el ID del usuario a desbloquear.</param>
    /// <response code="200">Usuario desbloqueado exitosamente.</response>
    /// <response code="400">Si el UserId no fue proporcionado.</response>
    /// <response code="404">Si el usuario no existe.</response>
    [HttpPost("unblock")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> UnBlockUser([FromBody] UnBlockUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new
            {
                success = false,
                message = "El userId es requerido"
            });
        }

        try
        {
            var response = await authService.UnBlockUserAsync(request.UserId);
            
            return Ok(new
            {
                success = true,
                message = "Usuario desbloqueado exitosamente",
                data = response
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

}