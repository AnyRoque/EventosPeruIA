using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IPeruIARepository
    {
        Task<IEnumerable<Evento>> GetEventosDisponibles(int cantidad);
        Task<IEnumerable<PreguntaFrecuente>> GetPreguntasFrecuentes();
    }
}
