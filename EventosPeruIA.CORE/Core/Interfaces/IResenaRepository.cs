using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IResenaRepository
    {
        Task<IEnumerable<Resena>> GetResenasVisibles(int idEvento);
        Task<Resena?> GetResenaById(int idResena);
        Task<bool> AsistioAlEvento(int idUsuario, int idEvento);
        Task<bool> YaReseno(int idUsuario, int idEvento);
        Task<bool> CreateResena(Resena resena);
        Task<bool> UpdateResena(Resena resena);
        Task<bool> DeleteResena(Resena resena);
    }
}
