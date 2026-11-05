using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;

namespace EventosPeruIA.API.Controllers
{
    // US-06 Compra de entradas con código QR / voucher digital
    [Route("api/[controller]")]
    [Authorize]
    public class CompraController : BaseApiController
    {
        private readonly ICompraService _compraService;

        public CompraController(ICompraService compraService)
        {
            _compraService = compraService;
        }

        [HttpPost]
        public async Task<IActionResult> Comprar([FromBody] CompraCreateDTO dto)
        {
            return Responder(await _compraService.ComprarEntradas(UsuarioIdActual!.Value, dto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompra(int id)
        {
            return Responder(await _compraService.GetCompra(UsuarioIdActual!.Value, id));
        }

        // "Mis entradas" con su código QR
        [HttpGet("mis-entradas")]
        public async Task<IActionResult> GetMisEntradas()
        {
            var entradas = await _compraService.GetMisEntradas(UsuarioIdActual!.Value);
            return Ok(entradas);
        }

        [HttpGet("entradas/{idEntrada:int}/pdf")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
        public async Task<IActionResult> DescargarEntrada(int idEntrada)
        {
            if (!UsuarioIdActual.HasValue) return Unauthorized();
            var resultado = await _compraService.GetEntradaPdf(UsuarioIdActual.Value, idEntrada);
            if (!resultado.Exito) return Responder(resultado);
            return File(resultado.Datos!.Contenido, resultado.Datos.TipoContenido, resultado.Datos.Nombre);
        }

        [HttpGet("{id:int}/pdf")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
        public async Task<IActionResult> DescargarCompra(int id)
        {
            if (!UsuarioIdActual.HasValue) return Unauthorized();
            var resultado = await _compraService.GetCompraPdf(UsuarioIdActual.Value, id);
            if (!resultado.Exito) return Responder(resultado);
            return File(resultado.Datos!.Contenido, resultado.Datos.TipoContenido, resultado.Datos.Nombre);
        }
    }
}
