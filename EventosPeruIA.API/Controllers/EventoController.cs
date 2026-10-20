using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;

namespace EventosPeruIA.API.Controllers
{
    [Route("api/[controller]")]
    public class EventoController : BaseApiController
    {
        private readonly IEventoService _eventoService;

        public EventoController(IEventoService eventoService)
        {
            _eventoService = eventoService;
        }

        // US-02 Búsqueda y filtrado (público)
        // GET api/Evento?palabra=rock&idCategoria=1&desde=2026-11-01&hasta=2026-11-30&departamento=Lima&distrito=Barranco&pagina=1
        [HttpGet]
        public async Task<IActionResult> BuscarEventos([FromQuery] EventoFiltroDTO filtro)
        {
            return Responder(await _eventoService.BuscarEventos(filtro));
        }

        // US-03 Detalle del evento (público; si hay sesión, muestra "Ya tienes N entradas" / "Es tu evento")
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            return Responder(await _eventoService.GetDetalle(id, UsuarioIdActual));
        }

        // US-07 Cupos disponibles (el frontend lo consulta cada pocos segundos)
        [HttpGet("{id}/disponibilidad")]
        public async Task<IActionResult> GetDisponibilidad(int id)
        {
            return Responder(await _eventoService.GetDisponibilidad(id));
        }

        // US-09 Mis eventos
        [Authorize(Roles = Roles.Organizador)]
        [HttpGet("mis-eventos")]
        public async Task<IActionResult> GetMisEventos()
        {
            var eventos = await _eventoService.GetMisEventos(UsuarioIdActual!.Value);
            return Ok(eventos);
        }

        // US-09 Crear evento (publicar = true) o guardar borrador (publicar = false)
        [Authorize(Roles = Roles.Organizador)]
        [HttpPost]
        public async Task<IActionResult> CreateEvento([FromBody] EventoCreateDTO dto)
        {
            return Responder(await _eventoService.CreateEvento(UsuarioIdActual!.Value, dto));
        }

        // US-09 Editar evento (incluye las restricciones de aforo de US-07)
        [Authorize(Roles = Roles.Organizador)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvento(int id, [FromBody] EventoUpdateDTO dto)
        {
            return Responder(await _eventoService.UpdateEvento(UsuarioIdActual!.Value, id, dto));
        }

        // US-09 Publicar un borrador
        [Authorize(Roles = Roles.Organizador)]
        [HttpPatch("{id}/publicar")]
        public async Task<IActionResult> PublicarEvento(int id)
        {
            return Responder(await _eventoService.PublicarEvento(UsuarioIdActual!.Value, id));
        }

        // US-09 Eliminar un borrador
        [Authorize(Roles = Roles.Organizador)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvento(int id)
        {
            return Responder(await _eventoService.DeleteEvento(UsuarioIdActual!.Value, id));
        }
    }
}
