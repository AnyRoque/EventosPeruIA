using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.API.Controllers
{
    // Funciones comunes para todos los controllers
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        // Id del usuario que viene en el token (claim "UserId"); null si no inició sesión
        protected int? UsuarioIdActual
        {
            get
            {
                var claim = User.FindFirst("UserId")?.Value;
                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        // Convierte el resultado del servicio en la respuesta HTTP: { mensaje, datos }
        protected IActionResult Responder<T>(ResultadoDTO<T> resultado)
        {
            return StatusCode(resultado.Codigo, new { mensaje = resultado.Mensaje, datos = resultado.Datos });
        }
    }
}
