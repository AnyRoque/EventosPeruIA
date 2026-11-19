using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosPeruIA.API.Controllers
{
    // US-03 Imagen de portada del evento
    // POST   api/Evento/{idEvento}/imagen   (form-data, campo "imagen")
    // DELETE api/Evento/{idEvento}/imagen
    [Route("api/Evento/{idEvento:int}/imagen")]
    [Authorize(Roles = Roles.Organizador)]
    public class EventoImagenController : BaseApiController
    {
        private readonly IImagenService _imagenService;
        private readonly IWebHostEnvironment _env;

        public EventoImagenController(IImagenService imagenService, IWebHostEnvironment env)
        {
            _imagenService = imagenService;
            _env = env;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> SubirImagen(int idEvento, IFormFile? imagen)
        {
            return Responder(await _imagenService.SubirImagenEvento(UsuarioIdActual!.Value, idEvento, imagen, CarpetaWeb));
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarImagen(int idEvento)
        {
            return Responder(await _imagenService.EliminarImagenEvento(UsuarioIdActual!.Value, idEvento, CarpetaWeb));
        }

        // Carpeta wwwroot del proyecto API (la que publica UseStaticFiles)
        private string CarpetaWeb => _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
    }
}
