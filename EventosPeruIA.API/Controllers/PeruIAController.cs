using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;

namespace EventosPeruIA.API.Controllers
{
    // US-14 Asistente conversacional PeruIA (público: funciona sin iniciar sesión)
    [Route("api/[controller]")]
    public class PeruIAController : BaseApiController
    {
        private readonly IPeruIAService _peruIAService;

        public PeruIAController(IPeruIAService peruIAService)
        {
            _peruIAService = peruIAService;
        }

        // POST api/PeruIA/chat
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] PeruIARequestDTO dto)
        {
            return Responder(await _peruIAService.Conversar(dto));
        }
    }
}
