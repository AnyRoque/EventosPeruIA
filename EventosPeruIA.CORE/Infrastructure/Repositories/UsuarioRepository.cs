using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public UsuarioRepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Usuario?> GetUsuarioByCorreo(string correo)
        {
            return await _dbContext.Usuario
                .Include(u => u.UsuarioRol).ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);
        }

        public async Task<Usuario?> GetUsuarioById(int idUsuario)
        {
            return await _dbContext.Usuario
                .Include(u => u.UsuarioRol).ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }

        public async Task<bool> ExisteCorreo(string correo)
        {
            return await _dbContext.Usuario.AnyAsync(u => u.Correo == correo);
        }

        public async Task<bool> ExisteNombreOrganizador(string nombreOrganizador)
        {
            return await _dbContext.Usuario.AnyAsync(u => u.NombreOrganizador == nombreOrganizador);
        }

        public async Task<bool> CreateUsuario(Usuario usuario, IEnumerable<int> idRoles)
        {
            foreach (var idRol in idRoles)
            {
                usuario.UsuarioRol.Add(new UsuarioRol { IdRol = idRol, FechaAsignacion = DateTime.Now });
            }

            await _dbContext.Usuario.AddAsync(usuario);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> ActivarOrganizador(int idUsuario, string nombreOrganizador)
        {
            var usuario = await _dbContext.Usuario
                .Include(u => u.UsuarioRol)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario == null) return false;

            usuario.NombreOrganizador = nombreOrganizador;
            if (!usuario.UsuarioRol.Any(ur => ur.IdRol == Roles.IdOrganizador))
            {
                usuario.UsuarioRol.Add(new UsuarioRol { IdRol = Roles.IdOrganizador, FechaAsignacion = DateTime.Now });
            }

            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }
    }
}
