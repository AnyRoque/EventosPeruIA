using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IModeracionRepository
    {
        Task<(IEnumerable<Evento> Eventos, int Total)> GetEventos(ModeracionFiltroDTO filtro, int tamanoPagina);
        Task<(IEnumerable<Usuario> Usuarios, int Total)> GetUsuarios(ModeracionFiltroDTO filtro, int tamanoPagina);
        Task<(IEnumerable<Resena> Resenas, int Total)> GetResenas(ModeracionFiltroDTO filtro, int tamanoPagina);
        Task<(IEnumerable<Moderacion> Acciones, int Total)> GetHistorial(HistorialFiltroDTO filtro, int tamanoPagina);

        Task<Evento?> GetEventoById(int idEvento);
        Task<Usuario?> GetUsuarioById(int idUsuario);
        Task<Resena?> GetResenaById(int idResena);

        // Guarda el cambio del elemento y la fila del historial en un solo SaveChanges
        Task<bool> GuardarConHistorial(Moderacion accion);
    }
}
