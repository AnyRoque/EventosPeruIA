using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface INotificacionRepository
    {
        Task<(IEnumerable<Notificacion> Notificaciones, int Total)> GetNotificaciones(int idUsuario, bool soloNoLeidas, int pagina, int tamanoPagina);
        Task<int> ContarNoLeidas(int idUsuario);
        Task<bool> MarcarLeida(int idUsuario, int idNotificacion);
        Task<int> MarcarTodasLeidas(int idUsuario);
        Task<bool> CreateNotificaciones(IEnumerable<Notificacion> notificaciones);

        Task<IEnumerable<CompradorEventoDTO>> GetCompradores(int idEvento);
        Task<IEnumerable<Evento>> GetEventosSimilares(int idCategoria, int idEventoExcluido, int cantidad);
        Task<IEnumerable<(int IdUsuario, Evento Evento)>> GetPendientesRecordatorio(DateTime desde, DateTime hasta);
    }
}
