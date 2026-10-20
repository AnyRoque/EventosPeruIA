using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetUsuarioByCorreo(string correo);
        Task<Usuario?> GetUsuarioById(int idUsuario);
        Task<bool> ExisteCorreo(string correo);
        Task<bool> ExisteNombreOrganizador(string nombreOrganizador);
        Task<bool> CreateUsuario(Usuario usuario, IEnumerable<int> idRoles);
        Task<bool> ActivarOrganizador(int idUsuario, string nombreOrganizador);
    }
}
