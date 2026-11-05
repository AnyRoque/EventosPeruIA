using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;

namespace EventosPeruIA.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class RecomendacionController : BaseApiController
    {
        private readonly IRecomendacionService _recomendacionService;

        public RecomendacionController(IRecomendacionService recomendacionService)
        {
            _recomendacionService = recomendacionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecomendaciones()
        {
            if (!UsuarioIdActual.HasValue) return Unauthorized();
            return Responder(await _recomendacionService.GetRecomendaciones(UsuarioIdActual.Value));
        }

        [HttpGet("intereses")]
        public async Task<IActionResult> GetIntereses()
        {
            return Ok(await _recomendacionService.GetIntereses());
        }

        [HttpGet("mis-intereses")]
        public async Task<IActionResult> GetMisIntereses()
        {
            if (!UsuarioIdActual.HasValue) return Unauthorized();
            return Responder(await _recomendacionService.GetMisIntereses(UsuarioIdActual.Value));
        }

        [HttpPut("mis-intereses")]
        public async Task<IActionResult> GuardarIntereses([FromBody] UsuarioInteresesUpdateDTO dto)
        {
            if (!UsuarioIdActual.HasValue) return Unauthorized();
            return Responder(await _recomendacionService.GuardarIntereses(UsuarioIdActual.Value, dto));
        }
    }
}
