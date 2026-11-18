using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IRecomendacionRepository
    {
        Task<List<Interes>> GetInteresesActivos();
        Task<List<Interes>> GetInteresesUsuario(int idUsuario);
        Task GuardarIntereses(int idUsuario, IReadOnlyCollection<int> idIntereses);
        Task<List<int>> GetCategoriasCompradas(int idUsuario);
        Task<List<int>> GetCategoriasVisitadas(int idUsuario, DateTime desde);
        Task<List<Evento>> GetEventosDisponibles(int idUsuario, DateTime ahora);
    }
}
