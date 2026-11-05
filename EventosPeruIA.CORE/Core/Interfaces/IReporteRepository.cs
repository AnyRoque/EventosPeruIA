using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces;

public interface IReporteRepository
{
    Task<IEnumerable<Evento>> GetEventosOrganizador(int idOrganizador, int? idEvento);
    Task<IEnumerable<DetalleCompra>> GetVentas(int idOrganizador, FiltroReporteDTO filtro);
}
