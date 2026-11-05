using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventosPeruIA.CORE.Core.Services
{
    public class ResenaService : IResenaService
    {
        private readonly EventosPeruIAContext _dbContext;

        public ResenaService(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResultadoDTO<List<ResenaDTO>>> ObtenerResenasPorEvento(int idEvento)
        {
            var existeEvento = await _dbContext.Evento.AnyAsync(e => e.IdEvento == idEvento);
            if (!existeEvento)
                return ResultadoDTO<List<ResenaDTO>>.Error(404, "El evento especificado no existe.");

            var resenas = await _dbContext.Resena
                .Where(r => r.IdEvento == idEvento && r.Visible)
                .OrderByDescending(r => r.FechaPublicacion)
                .Select(r => new ResenaDTO
                {
                    IdResena = r.IdResena,
                    IdEvento = r.IdEvento,
                    NombreUsuario = r.Usuario.Nombre + " " + r.Usuario.Apellido,
                    Calificacion = r.Calificacion,
                    Comentario = r.Comentario,
                    FechaCreacion = r.FechaPublicacion
                })
                .ToListAsync();

            return ResultadoDTO<List<ResenaDTO>>.Ok(resenas, "Reseñas obtenidas correctamente.");
        }

        public async Task<ResultadoDTO<bool>> CrearResena(int idUsuario, CrearResenaDTO dto)
        {
            var validacion = ValidarContenido(dto.Calificacion, dto.Comentario);
            if (validacion != null)
                return ResultadoDTO<bool>.Error(400, validacion);

            var evento = await _dbContext.Evento.FindAsync(dto.IdEvento);
            if (evento == null)
                return ResultadoDTO<bool>.Error(404, "El evento especificado no existe.");

            var ahora = DateTime.Now;
            if (ahora < evento.FechaHoraFin)
                return ResultadoDTO<bool>.Error(400, "Solo puedes dejar una reseña una vez que el evento haya finalizado.");

            if (ahora > evento.FechaHoraFin.AddDays(30))
                return ResultadoDTO<bool>.Error(400, "Solo se pueden publicar reseñas hasta 30 días después de finalizado el evento.");

            var tieneEntradaUtilizada = await _dbContext.Entrada
                .AnyAsync(e => e.DetalleCompra.IdEvento == dto.IdEvento
                    && e.DetalleCompra.Compra.IdUsuario == idUsuario
                    && e.Estado == EstadosEntrada.Utilizada);

            if (!tieneEntradaUtilizada)
                return ResultadoDTO<bool>.Error(403, "Solo pueden dejar una reseña los usuarios que asistieron al evento.");

            var yaReseno = await _dbContext.Resena
                .AnyAsync(r => r.IdEvento == dto.IdEvento && r.IdUsuario == idUsuario);

            if (yaReseno)
                return ResultadoDTO<bool>.Error(400, "Ya has publicado una reseña para este evento.");

            _dbContext.Resena.Add(new Resena
            {
                IdEvento = dto.IdEvento,
                IdUsuario = idUsuario,
                Calificacion = (byte)dto.Calificacion,
                Comentario = dto.Comentario!.Trim(),
                Visible = true,
                FechaPublicacion = ahora
            });

            await _dbContext.SaveChangesAsync();
            return ResultadoDTO<bool>.Ok(true, "Gracias por tu reseña");
        }

        public async Task<ResultadoDTO<bool>> EditarResena(int idUsuario, int idResena, EditarResenaDTO dto)
        {
            var validacion = ValidarContenido(dto.Calificacion, dto.Comentario);
            if (validacion != null)
                return ResultadoDTO<bool>.Error(400, validacion);

            var resena = await _dbContext.Resena.FindAsync(idResena);
            if (resena == null)
                return ResultadoDTO<bool>.Error(404, "La reseña no existe.");

            if (resena.IdUsuario != idUsuario)
                return ResultadoDTO<bool>.Error(403, "Solo puedes editar tu propia reseña.");

            resena.Calificacion = (byte)dto.Calificacion;
            resena.Comentario = dto.Comentario!.Trim();
            resena.FechaModificacion = DateTime.Now;

            await _dbContext.SaveChangesAsync();
            return ResultadoDTO<bool>.Ok(true, "Reseña actualizada exitosamente.");
        }

        public async Task<ResultadoDTO<bool>> EliminarResena(int idUsuario, int idResena)
        {
            var resena = await _dbContext.Resena.FindAsync(idResena);
            if (resena == null)
                return ResultadoDTO<bool>.Error(404, "La reseña no existe.");

            if (resena.IdUsuario != idUsuario)
                return ResultadoDTO<bool>.Error(403, "Solo puedes eliminar tu propia reseña.");

            _dbContext.Resena.Remove(resena);
            await _dbContext.SaveChangesAsync();
            return ResultadoDTO<bool>.Ok(true, "Reseña eliminada exitosamente.");
        }

        private static string? ValidarContenido(int calificacion, string? comentario)
        {
            if (calificacion < 1 || calificacion > 5)
                return "La calificación debe estar entre 1 y 5 estrellas.";

            var texto = comentario?.Trim();
            if (string.IsNullOrWhiteSpace(texto) || texto.Length < 10 || texto.Length > 500)
                return "El comentario es obligatorio y debe tener entre 10 y 500 caracteres.";

            return null;
        }
    }
}