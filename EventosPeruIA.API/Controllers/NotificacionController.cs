using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.Interfaces;

namespace EventosPeruIA.API.Controllers
{
    // US-15 Notificaciones (campana)
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacionController : BaseApiController
    {
        private readonly INotificacionService _notificacionService;

        public NotificacionController(INotificacionService notificacionService)
        {
            _notificacionService = notificacionService;
        }

        // GET api/Notificacion?soloNoLeidas=false&pagina=1
        [HttpGet]
        public async Task<IActionResult> GetNotificaciones([FromQuery] bool soloNoLeidas = false, [FromQuery] int pagina = 1)
        {
            var notificaciones = await _notificacionService.GetNotificaciones(UsuarioIdActual!.Value, soloNoLeidas, pagina);
            return Ok(notificaciones);
        }

        // Número de la campana (el frontend lo consulta cada 30 segundos, sin recargar la página)
        [HttpGet("no-leidas")]
        public async Task<IActionResult> ContarNoLeidas()
        {
            var contador = await _notificacionService.ContarNoLeidas(UsuarioIdActual!.Value);
            return Ok(contador);
        }

        // Al abrir una notificación
        [HttpPatch("{id}/leer")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            return Responder(await _notificacionService.MarcarLeida(UsuarioIdActual!.Value, id));
        }

        // "Marcar todas como leídas"
        [HttpPatch("leer-todas")]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            return Responder(await _notificacionService.MarcarTodasLeidas(UsuarioIdActual!.Value));
        }
    }
}
