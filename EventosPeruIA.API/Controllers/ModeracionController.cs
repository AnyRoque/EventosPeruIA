using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;

namespace EventosPeruIA.API.Controllers
{
    // US-16 Panel de moderación: solo Administrador (criterio 1)
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Administrador)]
    public class ModeracionController : BaseApiController
    {
        private readonly IModeracionService _moderacionService;

        public ModeracionController(IModeracionService moderacionService)
        {
            _moderacionService = moderacionService;
        }

        // ---------- Pestaña Eventos ----------
        // GET api/Moderacion/eventos?buscar=&estado=Publicado&pagina=1
        [HttpGet("eventos")]
        public async Task<IActionResult> GetEventos([FromQuery] ModeracionFiltroDTO filtro)
            => Responder(await _moderacionService.GetEventos(filtro));

        [HttpPatch("eventos/{id}/suspender")]
        public async Task<IActionResult> SuspenderEvento(int id, [FromBody] ModeracionMotivoDTO dto)
            => Responder(await _moderacionService.SuspenderEvento(UsuarioIdActual!.Value, id, dto));

        [HttpPatch("eventos/{id}/reactivar")]
        public async Task<IActionResult> ReactivarEvento(int id, [FromBody] ModeracionReactivarDTO dto)
            => Responder(await _moderacionService.ReactivarEvento(UsuarioIdActual!.Value, id, dto));

        // ---------- Pestaña Usuarios ----------
        // GET api/Moderacion/usuarios?buscar=&estado=Activo&pagina=1
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios([FromQuery] ModeracionFiltroDTO filtro)
            => Responder(await _moderacionService.GetUsuarios(filtro));

        [HttpPatch("usuarios/{id}/suspender")]
        public async Task<IActionResult> SuspenderUsuario(int id, [FromBody] ModeracionMotivoDTO dto)
            => Responder(await _moderacionService.SuspenderUsuario(UsuarioIdActual!.Value, id, dto));

        [HttpPatch("usuarios/{id}/reactivar")]
        public async Task<IActionResult> ReactivarUsuario(int id, [FromBody] ModeracionReactivarDTO dto)
            => Responder(await _moderacionService.ReactivarUsuario(UsuarioIdActual!.Value, id, dto));

        // ---------- Pestaña Reseñas ----------
        // GET api/Moderacion/resenas?buscar=&estado=Visible&pagina=1   (estado: Visible / Oculta)
        [HttpGet("resenas")]
        public async Task<IActionResult> GetResenas([FromQuery] ModeracionFiltroDTO filtro)
            => Responder(await _moderacionService.GetResenas(filtro));

        [HttpPatch("resenas/{id}/ocultar")]
        public async Task<IActionResult> OcultarResena(int id, [FromBody] ModeracionMotivoDTO dto)
            => Responder(await _moderacionService.OcultarResena(UsuarioIdActual!.Value, id, dto));

        // ---------- Historial (criterio 10) ----------
        // GET api/Moderacion/historial?accion=SuspenderEvento&desde=2026-10-01&hasta=2026-11-30&pagina=1
        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial([FromQuery] HistorialFiltroDTO filtro)
            => Responder(await _moderacionService.GetHistorial(filtro));
    }
}
