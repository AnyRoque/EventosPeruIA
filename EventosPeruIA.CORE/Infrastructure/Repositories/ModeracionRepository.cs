using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class ModeracionRepository : IModeracionRepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public ModeracionRepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ===================== Listas con buscador y filtro por estado =====================

        public async Task<(IEnumerable<Evento> Eventos, int Total)> GetEventos(ModeracionFiltroDTO filtro, int tamanoPagina)
        {
            var query = _dbContext.Evento
                .AsNoTracking()
                .Include(e => e.Organizador)
                .Include(e => e.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            {
                var texto = filtro.Buscar.Trim();
                query = query.Where(e => e.Nombre.Contains(texto)
                                      || (e.Organizador.NombreOrganizador != null && e.Organizador.NombreOrganizador.Contains(texto)));
            }
            if (!string.IsNullOrWhiteSpace(filtro.Estado))
                query = query.Where(e => e.Estado == filtro.Estado);

            var total = await query.CountAsync();
            var lista = await query
                .OrderByDescending(e => e.FechaHoraInicio)
                .Skip((filtro.Pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
            return (lista, total);
        }

        public async Task<(IEnumerable<Usuario> Usuarios, int Total)> GetUsuarios(ModeracionFiltroDTO filtro, int tamanoPagina)
        {
            var query = _dbContext.Usuario
                .AsNoTracking()
                .Include(u => u.UsuarioRol).ThenInclude(ur => ur.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            {
                var texto = filtro.Buscar.Trim();
                query = query.Where(u => u.Nombre.Contains(texto) || u.Apellido.Contains(texto) || u.Correo.Contains(texto));
            }
            if (!string.IsNullOrWhiteSpace(filtro.Estado))
                query = query.Where(u => u.Estado == filtro.Estado);

            var total = await query.CountAsync();
            var lista = await query
                .OrderByDescending(u => u.FechaRegistro)
                .Skip((filtro.Pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
            return (lista, total);
        }

        public async Task<(IEnumerable<Resena> Resenas, int Total)> GetResenas(ModeracionFiltroDTO filtro, int tamanoPagina)
        {
            var query = _dbContext.Resena
                .AsNoTracking()
                .Include(r => r.Usuario)
                .Include(r => r.Evento)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            {
                var texto = filtro.Buscar.Trim();
                query = query.Where(r => r.Comentario.Contains(texto) || r.Evento.Nombre.Contains(texto));
            }
            // Estado de una reseña: "Visible" u "Oculta"
            if (filtro.Estado == "Visible") query = query.Where(r => r.Visible);
            if (filtro.Estado == "Oculta") query = query.Where(r => !r.Visible);

            var total = await query.CountAsync();
            var lista = await query
                .OrderByDescending(r => r.FechaPublicacion)
                .Skip((filtro.Pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
            return (lista, total);
        }

        public async Task<(IEnumerable<Moderacion> Acciones, int Total)> GetHistorial(HistorialFiltroDTO filtro, int tamanoPagina)
        {
            var query = _dbContext.Moderacion
                .AsNoTracking()
                .Include(m => m.Administrador)
                .Include(m => m.UsuarioAfectado)
                .Include(m => m.EventoAfectado)
                .Include(m => m.ResenaAfectada).ThenInclude(r => r!.Evento)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Accion))
                query = query.Where(m => m.Accion == filtro.Accion);
            if (filtro.Desde.HasValue)
                query = query.Where(m => m.FechaAccion >= filtro.Desde.Value.Date);
            if (filtro.Hasta.HasValue)
            {
                var hasta = filtro.Hasta.Value.Date.AddDays(1);
                query = query.Where(m => m.FechaAccion < hasta);
            }

            var total = await query.CountAsync();
            var lista = await query
                .OrderByDescending(m => m.FechaAccion)
                .Skip((filtro.Pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
            return (lista, total);
        }

        // ===================== Elementos a moderar =====================

        public async Task<Evento?> GetEventoById(int idEvento)
        {
            return await _dbContext.Evento.FirstOrDefaultAsync(e => e.IdEvento == idEvento);
        }

        public async Task<Usuario?> GetUsuarioById(int idUsuario)
        {
            return await _dbContext.Usuario
                .Include(u => u.UsuarioRol).ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }

        public async Task<Resena?> GetResenaById(int idResena)
        {
            return await _dbContext.Resena
                .Include(r => r.Evento)
                .FirstOrDefaultAsync(r => r.IdResena == idResena);
        }

        public async Task<bool> GuardarConHistorial(Moderacion accion)
        {
            await _dbContext.Moderacion.AddAsync(accion);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }
    }
}
