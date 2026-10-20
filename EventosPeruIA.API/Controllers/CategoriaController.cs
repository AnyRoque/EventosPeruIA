using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;

namespace EventosPeruIA.API.Controllers
{
    // US-04 Gestión de categorías de eventos
    [Route("api/[controller]")]
    public class CategoriaController : BaseApiController
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        // Categorías activas (para el formulario de eventos y los filtros de búsqueda)
        [HttpGet]
        public async Task<IActionResult> GetCategoriasActivas()
        {
            var categorias = await _categoriaService.GetCategoriasActivas();
            return Ok(categorias);
        }

        // Tabla del administrador: todas, con estado y cantidad de eventos
        [Authorize(Roles = Roles.Administrador)]
        [HttpGet("admin")]
        public async Task<IActionResult> GetCategorias([FromQuery] string? buscar)
        {
            var categorias = await _categoriaService.GetCategorias(buscar);
            return Ok(categorias);
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPost]
        public async Task<IActionResult> CreateCategoria([FromBody] CategoriaCreateDTO dto)
        {
            return Responder(await _categoriaService.CreateCategoria(dto));
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoria(int id, [FromBody] CategoriaUpdateDTO dto)
        {
            return Responder(await _categoriaService.UpdateCategoria(id, dto));
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> DesactivarCategoria(int id, [FromQuery] bool confirmar = false)
        {
            return Responder(await _categoriaService.DesactivarCategoria(id, confirmar));
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPatch("{id}/reactivar")]
        public async Task<IActionResult> ReactivarCategoria(int id)
        {
            return Responder(await _categoriaService.ReactivarCategoria(id));
        }
    }
}
