using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using Microsoft.AspNetCore.Http;

namespace EventosPeruIA.CORE.Core.Services
{
    // US-03 Imagen de portada del evento: opcional, JPG o PNG, máximo 2 MB
    public class ImagenService : IImagenService
    {
        private readonly IEventoRepository _eventoRepository;

        public ImagenService(IEventoRepository eventoRepository)
        {
            _eventoRepository = eventoRepository;
        }

        public async Task<ResultadoDTO<string>> SubirImagenEvento(int idOrganizador, int idEvento, IFormFile? imagen, string carpetaWeb)
        {
            var (evento, error) = await ValidarEvento(idOrganizador, idEvento);
            if (error != null) return ResultadoDTO<string>.Error(error.Value.Codigo, error.Value.Mensaje);

            if (imagen == null || imagen.Length == 0)
                return ResultadoDTO<string>.Error(400, "Selecciona una imagen.");
            if (imagen.Length > Imagenes.TamanoMaximo)
                return ResultadoDTO<string>.Error(400, "La imagen debe pesar como máximo 2 MB.");

            // Se valida la extensión y también el contenido real del archivo (no basta con renombrarlo a .jpg)
            var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
            if (extension == ".jpeg") extension = ".jpg";
            if ((extension != ".jpg" && extension != ".png") || !await EsJpgOPng(imagen, extension))
                return ResultadoDTO<string>.Error(400, "La imagen debe estar en formato JPG o PNG.");

            // Nombre único: evento + fecha y hora + código aleatorio
            var nombreArchivo = $"evento{idEvento}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..6]}{extension}";
            var carpeta = CarpetaFisica(carpetaWeb);
            Directory.CreateDirectory(carpeta);
            var rutaArchivo = Path.Combine(carpeta, nombreArchivo);

            await using (var stream = new FileStream(rutaArchivo, FileMode.CreateNew))
            {
                await imagen.CopyToAsync(stream);
            }

            var anterior = evento!.ImagenUrl;
            evento.ImagenUrl = nombreArchivo;            // en la BD solo se guarda el nombre del archivo
            evento.FechaModificacion = DateTime.Now;

            if (!await _eventoRepository.UpdateEvento(evento))
            {
                File.Delete(rutaArchivo);
                return ResultadoDTO<string>.Error(500, "No se pudo guardar la imagen. Inténtalo nuevamente.");
            }

            BorrarArchivo(carpeta, anterior);
            return ResultadoDTO<string>.Ok(Imagenes.Ruta(nombreArchivo), "Imagen guardada correctamente.");
        }

        public async Task<ResultadoDTO<object>> EliminarImagenEvento(int idOrganizador, int idEvento, string carpetaWeb)
        {
            var (evento, error) = await ValidarEvento(idOrganizador, idEvento);
            if (error != null) return ResultadoDTO<object>.Error(error.Value.Codigo, error.Value.Mensaje);

            if (string.IsNullOrWhiteSpace(evento!.ImagenUrl))
                return ResultadoDTO<object>.Error(400, "El evento no tiene imagen.");

            var anterior = evento.ImagenUrl;
            evento.ImagenUrl = null;
            evento.FechaModificacion = DateTime.Now;
            await _eventoRepository.UpdateEvento(evento);

            BorrarArchivo(CarpetaFisica(carpetaWeb), anterior);
            return ResultadoDTO<object>.Ok(null, "Imagen eliminada.");
        }

        // ===================== Métodos de apoyo =====================

        // Mismas reglas que editar el evento: solo su organizador y mientras se pueda editar
        private async Task<(Evento? Evento, (int Codigo, string Mensaje)? Error)> ValidarEvento(int idOrganizador, int idEvento)
        {
            var evento = await _eventoRepository.GetEventoById(idEvento);
            if (evento == null) return (null, (404, "Evento no encontrado."));
            if (evento.IdOrganizador != idOrganizador) return (null, (403, "Solo el organizador del evento puede cambiar su imagen."));
            if (evento.Estado == EstadosEvento.Suspendido)
                return (null, (400, "El evento está suspendido por el administrador y no se puede editar."));
            if (evento.Estado == EstadosEvento.Cancelado || evento.Estado == EstadosEvento.Finalizado || evento.FechaHoraInicio <= DateTime.Now)
                return (null, (400, "No se pueden editar eventos cancelados o finalizados."));
            return (evento, null);
        }

        private static string CarpetaFisica(string carpetaWeb) =>
            Path.Combine(carpetaWeb, Imagenes.CarpetaEventos.Replace('/', Path.DirectorySeparatorChar));

        // Revisa los primeros bytes del archivo: JPG empieza con FF D8 FF y PNG con 89 50 4E 47
        private static async Task<bool> EsJpgOPng(IFormFile imagen, string extension)
        {
            var cabecera = new byte[4];
            await using var stream = imagen.OpenReadStream();
            var leidos = await stream.ReadAsync(cabecera);
            if (leidos < 4) return false;

            return extension == ".jpg"
                ? cabecera[0] == 0xFF && cabecera[1] == 0xD8 && cabecera[2] == 0xFF
                : cabecera[0] == 0x89 && cabecera[1] == 0x50 && cabecera[2] == 0x4E && cabecera[3] == 0x47;
        }

        // Solo borra archivos de la carpeta de imágenes (nunca una URL externa ni otra ruta)
        private static void BorrarArchivo(string carpeta, string? archivo)
        {
            if (string.IsNullOrWhiteSpace(archivo) || archivo.Contains('/') || archivo.Contains('\\')) return;
            var ruta = Path.Combine(carpeta, archivo);
            if (File.Exists(ruta)) File.Delete(ruta);
        }
    }
}
