using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IEventoRepository
    {
        Task<(IEnumerable<Evento> Eventos, int Total)> BuscarEventos(EventoFiltroDTO filtro, int tamanoPagina);
        Task<Evento?> GetEventoById(int idEvento);
        Task<Evento?> GetEventoConResenas(int idEvento);
        Task<IEnumerable<Evento>> GetEventosByOrganizador(int idOrganizador);
        Task<bool> CreateEvento(Evento evento);
        Task<bool> UpdateEvento(Evento evento);
        Task<bool> DeleteEvento(Evento evento);
        Task<int> ContarCompradores(int idEvento);
        Task<int> ContarEntradasUsuario(int idEvento, int idUsuario);
        Task RegistrarVisita(int idEvento, int idUsuario);
        Task<bool> DescontarCupos(int idEvento, int cantidad);
    }
}
