using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;

namespace EventosPeruIA.API.Controllers
{
    // US-10 Validación de entradas (check-in)
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Organizador)]
    public class CheckInController : BaseApiController
    {
        private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        // Eventos del día del organizador
        [HttpGet("eventos")]
        public async Task<IActionResult> GetEventosDelDia()
        {
            var eventos = await _checkInService.GetEventosDelDia(UsuarioIdActual!.Value);
            return Ok(eventos);
        }

        // Validar el código QR (o escrito a mano)
        [HttpPost("validar")]
        public async Task<IActionResult> ValidarEntrada([FromBody] CheckInRequestDTO dto)
        {
            return Responder(await _checkInService.ValidarEntrada(UsuarioIdActual!.Value, dto));
        }

        // Contador ingresados / vendidas y últimos ingresos
        [HttpGet("{idEvento}/resumen")]
        public async Task<IActionResult> GetResumen(int idEvento)
        {
            return Responder(await _checkInService.GetResumen(UsuarioIdActual!.Value, idEvento));
        }
    }
}
