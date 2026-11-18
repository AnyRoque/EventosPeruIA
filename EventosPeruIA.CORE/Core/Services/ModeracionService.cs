using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;

namespace EventosPeruIA.CORE.Core.Services
{
    // US-16 Moderación de eventos, usuarios y reseñas
    public class ModeracionService : IModeracionService
    {
        private const int TamanoPagina = 20;

        // Valores permitidos en la columna Moderacion.Accion (CHECK de la BD)
        private const string SuspenderEventoAccion = "SuspenderEvento";
        private const string ReactivarEventoAccion = "ReactivarEvento";
        private const string SuspenderUsuarioAccion = "SuspenderUsuario";
        private const string ReactivarUsuarioAccion = "ReactivarUsuario";
        private const string OcultarResenaAccion = "OcultarResena";

        private readonly IModeracionRepository _moderacionRepository;
        private readonly INotificacionService _notificacionService;

        public ModeracionService(IModeracionRepository moderacionRepository, INotificacionService notificacionService)
        {
            _moderacionRepository = moderacionRepository;
            _notificacionService = notificacionService;
        }

        // ===================== Pestañas del panel =====================

        public async Task<ResultadoDTO<PaginaDTO<EventoModeracionDTO>>> GetEventos(ModeracionFiltroDTO filtro)
        {
            var (eventos, total) = await _moderacionRepository.GetEventos(filtro, TamanoPagina);
            var ahora = DateTime.Now;
            var items = eventos.Select(e => new EventoModeracionDTO
            {
                IdEvento = e.IdEvento,
                Nombre = e.Nombre,
                Organizador = e.Organizador.NombreOrganizador ?? $"{e.Organizador.Nombre} {e.Organizador.Apellido}",
                FechaHoraInicio = e.FechaHoraInicio,
                Categoria = e.Categoria.Nombre,
                Estado = e.Estado,
                PuedeSuspender = e.Estado == EstadosEvento.Publicado,
                PuedeReactivar = e.Estado == EstadosEvento.Suspendido && e.FechaHoraInicio > ahora
            });
            return ResultadoDTO<PaginaDTO<EventoModeracionDTO>>.Ok(Paginar(items, total, filtro.Pagina));
        }

        public async Task<ResultadoDTO<PaginaDTO<UsuarioModeracionDTO>>> GetUsuarios(ModeracionFiltroDTO filtro)
        {
            var (usuarios, total) = await _moderacionRepository.GetUsuarios(filtro, TamanoPagina);
            var items = usuarios.Select(u => new UsuarioModeracionDTO
            {
                IdUsuario = u.IdUsuario,
                Nombre = $"{u.Nombre} {u.Apellido}",
                Correo = u.Correo,
                Roles = u.UsuarioRol.Select(ur => ur.Rol.Nombre).ToList(),
                Estado = u.Estado,
                FechaRegistro = u.FechaRegistro
            });
            return ResultadoDTO<PaginaDTO<UsuarioModeracionDTO>>.Ok(Paginar(items, total, filtro.Pagina));
        }

        public async Task<ResultadoDTO<PaginaDTO<ResenaModeracionDTO>>> GetResenas(ModeracionFiltroDTO filtro)
        {
            var (resenas, total) = await _moderacionRepository.GetResenas(filtro, TamanoPagina);
            var items = resenas.Select(r => new ResenaModeracionDTO
            {
                IdResena = r.IdResena,
                Evento = r.Evento.Nombre,
                Usuario = $"{r.Usuario.Nombre} {r.Usuario.Apellido}",
                Calificacion = r.Calificacion,
                Comentario = r.Comentario,
                Estado = r.Visible ? "Visible" : "Oculta",
                FechaPublicacion = r.FechaPublicacion
            });
            return ResultadoDTO<PaginaDTO<ResenaModeracionDTO>>.Ok(Paginar(items, total, filtro.Pagina));
        }

        // Criterio 10: historial de acciones
        public async Task<ResultadoDTO<PaginaDTO<HistorialModeracionDTO>>> GetHistorial(HistorialFiltroDTO filtro)
        {
            if (filtro.Desde.HasValue && filtro.Hasta.HasValue && filtro.Hasta.Value.Date < filtro.Desde.Value.Date)
                return ResultadoDTO<PaginaDTO<HistorialModeracionDTO>>.Error(400, "La fecha final debe ser mayor o igual a la fecha inicial.");

            var (acciones, total) = await _moderacionRepository.GetHistorial(filtro, TamanoPagina);
            var items = acciones.Select(m => new HistorialModeracionDTO
            {
                IdModeracion = m.IdModeracion,
                FechaAccion = m.FechaAccion,
                Administrador = $"{m.Administrador.Nombre} {m.Administrador.Apellido}",
                Accion = m.Accion,
                TipoElemento = m.IdEventoAfectado != null ? "Evento" : m.IdUsuarioAfectado != null ? "Usuario" : "Reseña",
                ElementoAfectado = m.EventoAfectado?.Nombre
                                   ?? (m.UsuarioAfectado != null ? $"{m.UsuarioAfectado.Nombre} {m.UsuarioAfectado.Apellido} ({m.UsuarioAfectado.Correo})" : null)
                                   ?? (m.ResenaAfectada != null ? $"Reseña #{m.ResenaAfectada.IdResena} de «{m.ResenaAfectada.Evento.Nombre}»" : string.Empty),
                Motivo = m.Motivo
            });
            return ResultadoDTO<PaginaDTO<HistorialModeracionDTO>>.Ok(Paginar(items, total, filtro.Pagina));
        }

        // ===================== Eventos =====================

        // Criterio 4: deja de aparecer en la búsqueda y no se venden entradas
        // (la búsqueda y la compra solo aceptan eventos "Publicado"); las entradas vendidas siguen válidas.
        public async Task<ResultadoDTO<object>> SuspenderEvento(int idAdmin, int idEvento, ModeracionMotivoDTO dto)
        {
            var evento = await _moderacionRepository.GetEventoById(idEvento);
            if (evento == null) return ResultadoDTO<object>.Error(404, "Evento no encontrado.");
            if (evento.Estado != EstadosEvento.Publicado)
                return ResultadoDTO<object>.Error(400, "Solo se pueden suspender eventos publicados.");

            var motivo = dto.Motivo.Trim();
            evento.Estado = EstadosEvento.Suspendido;
            evento.FechaModificacion = DateTime.Now;
            await _moderacionRepository.GuardarConHistorial(NuevaAccion(idAdmin, SuspenderEventoAccion, motivo, idEvento: idEvento));

            // Criterio 9: avisa al organizador y a los compradores
            await _notificacionService.NotificarSuspension(evento, motivo);

            return ResultadoDTO<object>.Ok(null, "Evento suspendido. Se notificó al organizador y a los compradores.");
        }

        // Criterio 5: solo si su fecha no ha pasado
        public async Task<ResultadoDTO<object>> ReactivarEvento(int idAdmin, int idEvento, ModeracionReactivarDTO dto)
        {
            var evento = await _moderacionRepository.GetEventoById(idEvento);
            if (evento == null) return ResultadoDTO<object>.Error(404, "Evento no encontrado.");
            if (evento.Estado != EstadosEvento.Suspendido)
                return ResultadoDTO<object>.Error(400, "Solo se pueden reactivar eventos suspendidos.");
            if (evento.FechaHoraInicio <= DateTime.Now)
                return ResultadoDTO<object>.Error(400, "No se puede reactivar un evento cuya fecha ya pasó.");

            evento.Estado = EstadosEvento.Publicado;
            evento.FechaModificacion = DateTime.Now;
            await _moderacionRepository.GuardarConHistorial(
                NuevaAccion(idAdmin, ReactivarEventoAccion, MotivoReactivacion(dto, "Evento reactivado por el administrador."), idEvento: idEvento));

            return ResultadoDTO<object>.Ok(null, "Evento reactivado. Vuelve a aparecer en la búsqueda.");
        }

        // ===================== Usuarios =====================

        // Criterio 7: un usuario suspendido no puede iniciar sesión (ya lo valida el login);
        // el administrador no puede suspender su propia cuenta.
        public async Task<ResultadoDTO<object>> SuspenderUsuario(int idAdmin, int idUsuario, ModeracionMotivoDTO dto)
        {
            if (idUsuario == idAdmin)
                return ResultadoDTO<object>.Error(400, "No puedes suspender tu propia cuenta.");

            var usuario = await _moderacionRepository.GetUsuarioById(idUsuario);
            if (usuario == null) return ResultadoDTO<object>.Error(404, "Usuario no encontrado.");
            if (usuario.Estado == "Suspendido")
                return ResultadoDTO<object>.Error(400, "El usuario ya está suspendido.");

            var motivo = dto.Motivo.Trim();
            usuario.Estado = "Suspendido";
            usuario.MotivoSuspension = motivo;
            await _moderacionRepository.GuardarConHistorial(NuevaAccion(idAdmin, SuspenderUsuarioAccion, motivo, idUsuario: idUsuario));

            // Criterio 9: notificación con el motivo
            await _notificacionService.NotificarSuspensionUsuario(idUsuario, motivo);

            return ResultadoDTO<object>.Ok(null, "Usuario suspendido. Ya no podrá iniciar sesión.");
        }

        public async Task<ResultadoDTO<object>> ReactivarUsuario(int idAdmin, int idUsuario, ModeracionReactivarDTO dto)
        {
            var usuario = await _moderacionRepository.GetUsuarioById(idUsuario);
            if (usuario == null) return ResultadoDTO<object>.Error(404, "Usuario no encontrado.");
            if (usuario.Estado != "Suspendido")
                return ResultadoDTO<object>.Error(400, "El usuario no está suspendido.");

            usuario.Estado = "Activo";
            usuario.MotivoSuspension = null;
            await _moderacionRepository.GuardarConHistorial(
                NuevaAccion(idAdmin, ReactivarUsuarioAccion, MotivoReactivacion(dto, "Usuario reactivado por el administrador."), idUsuario: idUsuario));

            return ResultadoDTO<object>.Ok(null, "Usuario reactivado. Ya puede iniciar sesión.");
        }

        // ===================== Reseñas =====================

        // Criterio 8: una reseña oculta deja de mostrarse en la página del evento
        public async Task<ResultadoDTO<object>> OcultarResena(int idAdmin, int idResena, ModeracionMotivoDTO dto)
        {
            var resena = await _moderacionRepository.GetResenaById(idResena);
            if (resena == null) return ResultadoDTO<object>.Error(404, "Reseña no encontrada.");
            if (!resena.Visible) return ResultadoDTO<object>.Error(400, "La reseña ya está oculta.");

            var motivo = dto.Motivo.Trim();
            resena.Visible = false;
            await _moderacionRepository.GuardarConHistorial(NuevaAccion(idAdmin, OcultarResenaAccion, motivo, idResena: idResena));

            // Criterio 9: avisa al autor de la reseña
            await _notificacionService.NotificarResenaOculta(resena.IdUsuario, resena.IdEvento, resena.Evento.Nombre, motivo);

            return ResultadoDTO<object>.Ok(null, "Reseña ocultada. Ya no se muestra en la página del evento.");
        }

        // ===================== Métodos de apoyo =====================

        private static Moderacion NuevaAccion(int idAdmin, string accion, string motivo,
            int? idEvento = null, int? idUsuario = null, int? idResena = null) => new()
        {
            IdAdministrador = idAdmin,
            IdEventoAfectado = idEvento,
            IdUsuarioAfectado = idUsuario,
            IdResenaAfectada = idResena,
            Accion = accion,
            Motivo = motivo,
            FechaAccion = DateTime.Now
        };

        // La BD exige un motivo de al menos 10 caracteres en el historial
        private static string MotivoReactivacion(ModeracionReactivarDTO dto, string porDefecto) =>
            string.IsNullOrWhiteSpace(dto.Motivo) || dto.Motivo.Trim().Length < 10 ? porDefecto : dto.Motivo.Trim();

        private static PaginaDTO<T> Paginar<T>(IEnumerable<T> items, int total, int pagina) => new()
        {
            Items = items.ToList(),
            Total = total,
            Pagina = pagina,
            TamanoPagina = TamanoPagina,
            TotalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina)
        };
    }
}
