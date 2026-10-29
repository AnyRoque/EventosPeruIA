using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;

namespace EventosPeruIA.API.Controllers
{
    [Route("api/[controller]")]
    public class UsuarioController : BaseApiController
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // US-01 Registro de cuenta
        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroRequestDTO dto)
        {
            var resultado = await _usuarioService.Registrar(dto);
            return Responder(resultado);
        }

        // US-02 Inicio de sesión
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var resultado = await _usuarioService.Login(dto);
            return Responder(resultado);
        }

        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> Perfil()
        {
            var resultado = await _usuarioService.GetPerfil(UsuarioIdActual!.Value);
            return Responder(resultado);
        }

        // US-01: "Quiero publicar eventos" (activa el rol Organizador)
        [Authorize]
        [HttpPost("activar-organizador")]
        public async Task<IActionResult> ActivarOrganizador([FromBody] ActivarOrganizadorDTO dto)
        {
            var resultado = await _usuarioService.ActivarOrganizador(UsuarioIdActual!.Value, dto);
            return Responder(resultado);
        }
    }
}
