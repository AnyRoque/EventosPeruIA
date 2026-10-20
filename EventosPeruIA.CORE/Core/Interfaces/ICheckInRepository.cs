using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface ICheckInRepository
    {
        Task<IEnumerable<Evento>> GetEventosParaCheckIn(int idOrganizador, DateTime desde, DateTime hasta);
        Task<Entrada?> GetEntradaByCodigo(Guid codigo);
        Task<bool> MarcarUtilizada(int idEntrada);
        Task<bool> CreateCheckIn(CheckIn checkIn);
        Task<int> ContarIngresados(int idEvento);
        Task<IEnumerable<CheckIn>> GetUltimosIngresos(int idEvento, int cantidad);
    }
}
