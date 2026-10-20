using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IEventoService
    {
        Task<ResultadoDTO<PaginaDTO<EventoListDTO>>> BuscarEventos(EventoFiltroDTO filtro);
        Task<ResultadoDTO<EventoDetalleDTO>> GetDetalle(int idEvento, int? idUsuario);
        Task<ResultadoDTO<DisponibilidadDTO>> GetDisponibilidad(int idEvento);
        Task<IEnumerable<MiEventoDTO>> GetMisEventos(int idOrganizador);
        Task<ResultadoDTO<EventoDetalleDTO>> CreateEvento(int idOrganizador, EventoCreateDTO dto);
        Task<ResultadoDTO<EventoDetalleDTO>> UpdateEvento(int idOrganizador, int idEvento, EventoUpdateDTO dto);
        Task<ResultadoDTO<EventoDetalleDTO>> PublicarEvento(int idOrganizador, int idEvento);
        Task<ResultadoDTO<object>> DeleteEvento(int idOrganizador, int idEvento);
    }
}
