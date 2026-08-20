using Microsoft.AspNetCore.Mvc;
using AuthService.Application.Interfaces;

namespace AuthService.Api.Controllers;

/// <summary>
/// Controlador de pruebas para el servicio de mensajeria de LearnTestIA.
/// Permite validar la conectividad con el servidor SMTP y el renderizado de plantillas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailTestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Envia un correo electronico de bienvenida de prueba.
    /// </summary>
    /// <remarks>
    /// Utiliza este endpoint para verificar que los correos no esten cayendo en SPAM 
    /// y que el nombre del agricultor se visualice correctamente en la plantilla.
    /// </remarks>
    /// <param name="email">Direccion de correo del destinatario.</param>
    /// <param name="name">Nombre que aparecera en el saludo del mensaje.</param>
    /// <response code="200">El correo ha sido encolado y enviado exitosamente.</response>
    /// <response code="400">Si el formato del correo es invalido.</response>
    /// <response code="500">Error interno al intentar conectar con el servidor de correos.</response>
    [HttpPost("send-welcome")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestWelcome(string email, string name)
    {
        await _emailService.SendWelcomeEmailAsync(email, name);
        return Ok(new { message = $"¡exito! Correo enviado a {email}" });
    }
}