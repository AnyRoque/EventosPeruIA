using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;

namespace EventosPeruIA.CORE.Core.Services
{
    // US-12 Calificación y reseñas de eventos
    public class ResenaService : IResenaService
    {
        private const int DiasParaResenar = 30;

        private readonly IResenaRepository _resenaRepository;
        private readonly IEventoRepository _eventoRepository;

        public ResenaService(IResenaRepository resenaRepository, IEventoRepository eventoRepository)
        {
            _resenaRepository = resenaRepository;
            _eventoRepository = eventoRepository;
        }

        // Criterio 5: promedio de estrellas, total y comentarios del más reciente al más antiguo
        public async Task<ResultadoDTO<ResumenResenasEventoDTO>> ObtenerResenasPorEvento(int idEvento)
        {
            var evento = await _eventoRepository.GetEventoById(idEvento);
            if (evento == null)
                return ResultadoDTO<ResumenResenasEventoDTO>.Error(404, "El evento no existe.");

            var resenas = (await _resenaRepository.GetResenasVisibles(idEvento)).Select(Mapear).ToList();

            return ResultadoDTO<ResumenResenasEventoDTO>.Ok(new ResumenResenasEventoDTO
            {
                TotalResenas = resenas.Count,
                PromedioCalificacion = resenas.Count == 0 ? 0 : Math.Round(resenas.Average(r => r.Calificacion), 1),
                Resenas = resenas
            });
        }

        public async Task<ResultadoDTO<ResenaDTO>> CrearResena(int idUsuario, CrearResenaDTO dto)
        {
            var comentario = dto.Comentario.Trim();
            if (comentario.Length < 10)
                return ResultadoDTO<ResenaDTO>.Error(400, "El comentario debe tener entre 10 y 500 caracteres.");

            var evento = await _eventoRepository.GetEventoById(dto.IdEvento);
            if (evento == null)
                return ResultadoDTO<ResenaDTO>.Error(404, "El evento no existe.");

            // Criterio 3: ni eventos cancelados ni el propio evento del organizador
            if (evento.Estado == EstadosEvento.Cancelado)
                return ResultadoDTO<ResenaDTO>.Error(400, "No se pueden calificar eventos cancelados.");
            if (evento.IdOrganizador == idUsuario)
                return ResultadoDTO<ResenaDTO>.Error(400, "No puedes calificar tu propio evento.");

            // Criterios 1 y 2: una vez finalizado y hasta 30 días después
            var ahora = DateTime.Now;
            if (evento.FechaHoraFin > ahora)
                return ResultadoDTO<ResenaDTO>.Error(400, "Podrás calificar el evento cuando haya finalizado.");
            if (ahora > evento.FechaHoraFin.AddDays(DiasParaResenar))
                return ResultadoDTO<ResenaDTO>.Error(400, $"Solo se puede dejar una reseña hasta {DiasParaResenar} días después del evento.");

            // Criterio 1: solo quienes asistieron (entrada "Utilizada")
            if (!await _resenaRepository.AsistioAlEvento(idUsuario, dto.IdEvento))
                return ResultadoDTO<ResenaDTO>.Error(403, "Solo pueden calificar los usuarios que asistieron al evento.");

            // Criterio 2: una sola reseña por evento
            if (await _resenaRepository.YaReseno(idUsuario, dto.IdEvento))
                return ResultadoDTO<ResenaDTO>.Error(400, "Ya dejaste una reseña para este evento.");

            var resena = new Resena
            {
                IdEvento = dto.IdEvento,
                IdUsuario = idUsuario,
                Calificacion = (byte)dto.Calificacion,
                Comentario = comentario,
                Visible = true,
                FechaPublicacion = ahora
            };
            await _resenaRepository.CreateResena(resena);

            var creada = await _resenaRepository.GetResenaById(resena.IdResena);
            // Criterio 7
            return ResultadoDTO<ResenaDTO>.Ok(creada == null ? null : Mapear(creada), "Gracias por tu reseña", 201);
        }

        // Criterio 6: solo su propia reseña
        public async Task<ResultadoDTO<ResenaDTO>> EditarResena(int idUsuario, int idResena, EditarResenaDTO dto)
        {
            var comentario = dto.Comentario.Trim();
            if (comentario.Length < 10)
                return ResultadoDTO<ResenaDTO>.Error(400, "El comentario debe tener entre 10 y 500 caracteres.");

            var resena = await _resenaRepository.GetResenaById(idResena);
            var error = ValidarPropia(resena, idUsuario, "editar");
            if (error != null) return ResultadoDTO<ResenaDTO>.Error(error.Value.Codigo, error.Value.Mensaje);

            resena!.Calificacion = (byte)dto.Calificacion;
            resena.Comentario = comentario;
            resena.FechaModificacion = DateTime.Now;
            await _resenaRepository.UpdateResena(resena);

            return ResultadoDTO<ResenaDTO>.Ok(Mapear(resena), "Reseña actualizada.");
        }

        public async Task<ResultadoDTO<object>> EliminarResena(int idUsuario, int idResena)
        {
            var resena = await _resenaRepository.GetResenaById(idResena);
            var error = ValidarPropia(resena, idUsuario, "eliminar");
            if (error != null) return ResultadoDTO<object>.Error(error.Value.Codigo, error.Value.Mensaje);

            await _resenaRepository.DeleteResena(resena!);
            return ResultadoDTO<object>.Ok(null, "Reseña eliminada.");
        }

        // ===================== Métodos de apoyo =====================

        // Una reseña ocultada por el administrador (US-16) queda en su historial: no se edita ni se elimina
        private static (int Codigo, string Mensaje)? ValidarPropia(Resena? resena, int idUsuario, string accion)
        {
            if (resena == null) return (404, "La reseña no existe.");
            if (resena.IdUsuario != idUsuario) return (403, $"Solo puedes {accion} tu propia reseña.");
            if (!resena.Visible) return (400, "Esta reseña fue ocultada por el administrador y ya no se puede modificar.");
            return null;
        }

        private static ResenaDTO Mapear(Resena r) => new()
        {
            IdResena = r.IdResena,
            IdEvento = r.IdEvento,
            IdUsuario = r.IdUsuario,
            NombreUsuario = r.Usuario == null ? string.Empty : $"{r.Usuario.Nombre} {r.Usuario.Apellido}",
            Calificacion = r.Calificacion,
            Comentario = r.Comentario,
            FechaCreacion = r.FechaPublicacion,
            FechaModificacion = r.FechaModificacion
        };
    }
}
