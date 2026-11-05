using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosPeruIA.API.Controllers
{
    // US-12 Calificación y reseñas de eventos
    [Route("api/[controller]")]
    public class ResenaController : BaseApiController
    {
        private readonly IResenaService _resenaService;

        public ResenaController(IResenaService resenaService)
        {
            _resenaService = resenaService;
        }

        // Promedio, total y reseñas visibles de un evento (público)
        [HttpGet("evento/{idEvento}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerResenasPorEvento(int idEvento)
        {
            return Responder(await _resenaService.ObtenerResenasPorEvento(idEvento));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Asistente)]
        public async Task<IActionResult> CrearResena([FromBody] CrearResenaDTO dto)
        {
            return Responder(await _resenaService.CrearResena(UsuarioIdActual!.Value, dto));
        }

        [HttpPut("{idResena}")]
        [Authorize(Roles = Roles.Asistente)]
        public async Task<IActionResult> EditarResena(int idResena, [FromBody] EditarResenaDTO dto)
        {
            return Responder(await _resenaService.EditarResena(UsuarioIdActual!.Value, idResena, dto));
        }

        [HttpDelete("{idResena}")]
        [Authorize(Roles = Roles.Asistente)]
        public async Task<IActionResult> EliminarResena(int idResena)
        {
            return Responder(await _resenaService.EliminarResena(UsuarioIdActual!.Value, idResena));
        }
    }
}
