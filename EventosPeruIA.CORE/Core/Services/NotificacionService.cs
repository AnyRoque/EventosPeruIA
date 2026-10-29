using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;

namespace EventosPeruIA.CORE.Core.Services
{
    // US-15 Notificaciones de eventos próximos y cambios de programación
    public class NotificacionService : INotificacionService
    {
        private const int TamanoPagina = 20;
        private readonly INotificacionRepository _notificacionRepository;

        public NotificacionService(INotificacionRepository notificacionRepository)
        {
            _notificacionRepository = notificacionRepository;
        }

        // ===================== Campana del usuario =====================

        public async Task<PaginaDTO<NotificacionDTO>> GetNotificaciones(int idUsuario, bool soloNoLeidas, int pagina)
        {
            if (pagina < 1) pagina = 1;
            var (lista, total) = await _notificacionRepository.GetNotificaciones(idUsuario, soloNoLeidas, pagina, TamanoPagina);

            return new PaginaDTO<NotificacionDTO>
            {
                Items = lista.Select(n => new NotificacionDTO
                {
                    IdNotificacion = n.IdNotificacion,
                    Tipo = n.Tipo,
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    IdEvento = n.IdEvento,
                    Leida = n.Leida,
                    FechaCreacion = n.FechaCreacion
                }).ToList(),
                Total = total,
                Pagina = pagina,
                TamanoPagina = TamanoPagina,
                TotalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina)
            };
        }

        // El frontend lo consulta cada 30 segundos para actualizar el número de la campana sin recargar
        public async Task<ContadorNotificacionesDTO> ContarNoLeidas(int idUsuario)
        {
            return new ContadorNotificacionesDTO { NoLeidas = await _notificacionRepository.ContarNoLeidas(idUsuario) };
        }

        public async Task<ResultadoDTO<object>> MarcarLeida(int idUsuario, int idNotificacion)
        {
            var ok = await _notificacionRepository.MarcarLeida(idUsuario, idNotificacion);
            return ok
                ? ResultadoDTO<object>.Ok(null, "Notificación marcada como leída.")
                : ResultadoDTO<object>.Error(404, "Notificación no encontrada.");
        }

        public async Task<ResultadoDTO<object>> MarcarTodasLeidas(int idUsuario)
        {
            var cantidad = await _notificacionRepository.MarcarTodasLeidas(idUsuario);
            return ResultadoDTO<object>.Ok(new { marcadas = cantidad }, "Todas las notificaciones fueron marcadas como leídas.");
        }

        // ===================== Avisos que generan otras historias =====================

        // Confirmación de compra (US-06)
        public async Task NotificarCompra(int idUsuario, Evento evento, int cantidad, string numeroCompra)
        {
            var texto = cantidad == 1 ? "1 entrada" : $"{cantidad} entradas";
            await Guardar(new[]
            {
                Nueva(idUsuario, evento.IdEvento, "Compra", "Compra confirmada",
                    $"Tu compra de {texto} para «{evento.Nombre}» fue realizada con éxito. N° de compra: {numeroCompra}.")
            });
        }

        // Aviso al organizador cuando se venden todas las entradas
        public async Task NotificarAgotado(Evento evento)
        {
            await Guardar(new[]
            {
                Nueva(evento.IdOrganizador, evento.IdEvento, "Agotado", "¡Tu evento se agotó!",
                    $"Se vendieron todas las entradas de «{evento.Nombre}».")
            });
        }

        // Cambio de fecha, hora o lugar (US-03): muestra el dato anterior y el nuevo
        public async Task NotificarCambioProgramacion(Evento evento, DateTime inicioAnterior, string lugarAnterior)
        {
            var lugarNuevo = Lugar(evento);
            var cambios = new List<string>();
            if (inicioAnterior != evento.FechaHoraInicio)
                cambios.Add($"Horario. Antes: {inicioAnterior:dd/MM HH:mm} – Ahora: {evento.FechaHoraInicio:dd/MM HH:mm}.");
            if (lugarAnterior != lugarNuevo)
                cambios.Add($"Lugar. Antes: {lugarAnterior} – Ahora: {lugarNuevo}.");
            if (cambios.Count == 0) return;   // solo cambió la hora de fin

            var titulo = cambios.Count == 2 ? "Cambio de horario y lugar" : (cambios[0].StartsWith("Horario") ? "Cambio de horario" : "Cambio de lugar");
            var mensaje = Recortar($"«{evento.Nombre}» tiene cambios. {string.Join(" ", cambios)}");

            var compradores = await _notificacionRepository.GetCompradores(evento.IdEvento);
            await Guardar(compradores.Select(c => Nueva(c.IdUsuario, evento.IdEvento, "Cambio", titulo, mensaje)));
        }

        // Cancelación (la llama US-04): motivo, entradas anuladas, monto devuelto y hasta 3 eventos similares
        public async Task NotificarCancelacion(Evento evento)
        {
            var compradores = await _notificacionRepository.GetCompradores(evento.IdEvento);
            if (!compradores.Any()) return;

            var similares = await _notificacionRepository.GetEventosSimilares(evento.IdCategoria, evento.IdEvento, 3);
            var sugerencia = similares.Any()
                ? " Te pueden interesar: " + string.Join(", ", similares.Select(s => $"«{s.Nombre}» ({s.FechaHoraInicio:dd/MM})")) + "."
                : string.Empty;

            await Guardar(compradores.Select(c =>
            {
                var entradas = c.Entradas == 1 ? "Tu entrada fue anulada" : $"Tus {c.Entradas} entradas fueron anuladas";
                var devuelto = c.Monto > 0 ? $" y se te devolvió S/ {c.Monto:0.00}" : string.Empty;
                return Nueva(c.IdUsuario, evento.IdEvento, "Cancelacion", "Evento cancelado",
                    Recortar($"«{evento.Nombre}» fue cancelado. Motivo: {evento.MotivoCancelacion}. {entradas}{devuelto}.{sugerencia}"));
            }));
        }

        // Suspensión por el administrador (la llama US-16): avisa al organizador y a los compradores
        public async Task NotificarSuspension(Evento evento, string motivo)
        {
            var avisos = new List<Notificacion>
            {
                Nueva(evento.IdOrganizador, evento.IdEvento, "Suspension", "Tu evento fue suspendido",
                    Recortar($"«{evento.Nombre}» fue suspendido por el administrador. Motivo: {motivo}"))
            };

            var compradores = await _notificacionRepository.GetCompradores(evento.IdEvento);
            avisos.AddRange(compradores.Select(c => Nueva(c.IdUsuario, evento.IdEvento, "Suspension", "Evento suspendido",
                $"«{evento.Nombre}» fue suspendido temporalmente. Tus entradas siguen siendo válidas.")));

            await Guardar(avisos);
        }

        // Recordatorio 24 horas antes del inicio (lo ejecuta RecordatorioWorker cada 15 minutos).
        // Nunca se envía dos veces el mismo recordatorio.
        public async Task<int> EnviarRecordatorios()
        {
            var ahora = DateTime.Now;
            var pendientes = await _notificacionRepository.GetPendientesRecordatorio(ahora, ahora.AddHours(24));

            var avisos = pendientes.Select(p => Nueva(p.IdUsuario, p.Evento.IdEvento, "Recordatorio", "Tu evento es pronto",
                $"«{p.Evento.Nombre}» empieza el {p.Evento.FechaHoraInicio:dd/MM} a las {p.Evento.FechaHoraInicio:HH:mm} en {Lugar(p.Evento)}.")).ToList();

            await Guardar(avisos);
            return avisos.Count;
        }

        // ===================== Métodos de apoyo =====================

        private static Notificacion Nueva(int idUsuario, int? idEvento, string tipo, string titulo, string mensaje) => new()
        {
            IdUsuario = idUsuario,
            IdEvento = idEvento,
            Tipo = tipo,
            Titulo = titulo,
            Mensaje = mensaje,
            Leida = false,
            FechaCreacion = DateTime.Now
        };

        private async Task Guardar(IEnumerable<Notificacion> notificaciones)
        {
            var lista = notificaciones.ToList();
            if (lista.Count > 0) await _notificacionRepository.CreateNotificaciones(lista);
        }

        public static string Lugar(Evento e) => $"{e.Direccion}, {e.Distrito}";

        // La columna Mensaje admite 1000 caracteres
        private static string Recortar(string texto) => texto.Length <= 1000 ? texto : texto.Substring(0, 997) + "...";
    }
}
