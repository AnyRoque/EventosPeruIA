using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface ICheckInService
    {
        Task<IEnumerable<EventoCheckInDTO>> GetEventosDelDia(int idOrganizador);
        Task<ResultadoDTO<CheckInResultadoDTO>> ValidarEntrada(int idOrganizador, CheckInRequestDTO dto);
        Task<ResultadoDTO<CheckInResumenDTO>> GetResumen(int idOrganizador, int idEvento);
    }
}
