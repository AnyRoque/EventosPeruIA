using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface INotificacionService
    {
        // Para el usuario (campana)
        Task<PaginaDTO<NotificacionDTO>> GetNotificaciones(int idUsuario, bool soloNoLeidas, int pagina);
        Task<ContadorNotificacionesDTO> ContarNoLeidas(int idUsuario);
        Task<ResultadoDTO<object>> MarcarLeida(int idUsuario, int idNotificacion);
        Task<ResultadoDTO<object>> MarcarTodasLeidas(int idUsuario);

        // Para que las usen otras historias
        Task NotificarCompra(int idUsuario, Evento evento, int cantidad, string numeroCompra);
        Task NotificarAgotado(Evento evento);
        Task NotificarCambioProgramacion(Evento evento, DateTime inicioAnterior, string lugarAnterior);
        Task NotificarCancelacion(Evento evento);                 // US-04 Cancelación
        Task NotificarSuspension(Evento evento, string motivo);   // US-16 Moderación
        Task<int> EnviarRecordatorios();                          // proceso automático (24 h antes)
    }
}
