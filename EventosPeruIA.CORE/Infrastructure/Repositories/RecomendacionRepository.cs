using System.Data;
using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class RecomendacionRepository : IRecomendacionRepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public RecomendacionRepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<Interes>> GetInteresesActivos() => _dbContext.Interes.AsNoTracking()
            .Where(i => i.Estado).OrderBy(i => i.Nombre).ToListAsync();

        public Task<List<Interes>> GetInteresesUsuario(int idUsuario) => _dbContext.UsuarioInteres.AsNoTracking()
            .Where(ui => ui.IdUsuario == idUsuario && ui.Interes.Estado)
            .Select(ui => ui.Interes).OrderBy(i => i.Nombre).ToListAsync();

        public async Task GuardarIntereses(int idUsuario, IReadOnlyCollection<int> idIntereses)
        {
            // La sustitución es atómica. Serializable evita mezclar dos ediciones concurrentes.
            using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var actuales = await _dbContext.UsuarioInteres.Where(ui => ui.IdUsuario == idUsuario).ToListAsync();
            _dbContext.UsuarioInteres.RemoveRange(actuales.Where(ui => !idIntereses.Contains(ui.IdInteres)));
            var existentes = actuales.Select(ui => ui.IdInteres).ToHashSet();
            foreach (var idInteres in idIntereses.Where(id => !existentes.Contains(id)))
            {
                _dbContext.UsuarioInteres.Add(new UsuarioInteres
                {
                    IdUsuario = idUsuario,
                    IdInteres = idInteres,
                    FechaRegistro = DateTime.Now
                });
            }
            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public Task<List<int>> GetCategoriasCompradas(int idUsuario) => _dbContext.DetalleCompra.AsNoTracking()
            .Where(d => d.Compra.IdUsuario == idUsuario
                && (d.Compra.Estado == EstadosCompra.Pagada || d.Compra.Estado == EstadosCompra.Reembolsada))
            .Select(d => d.Evento.IdCategoria).Distinct().ToListAsync();

        public Task<List<int>> GetCategoriasVisitadas(int idUsuario, DateTime desde) => _dbContext.EventoVisita.AsNoTracking()
            .Where(v => v.IdUsuario == idUsuario && v.FechaVisita >= desde
                && v.Evento.IdOrganizador != idUsuario
                && (v.Evento.Estado == EstadosEvento.Publicado || v.Evento.Estado == EstadosEvento.Finalizado))
            .Select(v => v.Evento.IdCategoria).Distinct().ToListAsync();

        public Task<List<Evento>> GetEventosDisponibles(int idUsuario, DateTime ahora) => _dbContext.Evento.AsNoTracking()
            .Include(e => e.Categoria)
            .Where(e => e.Estado == EstadosEvento.Publicado && e.FechaHoraInicio > ahora
                && e.EntradasVendidas < e.AforoMaximo && e.IdOrganizador != idUsuario
                && !e.DetalleCompra.Any(d => d.Compra.IdUsuario == idUsuario
                    && (d.Compra.Estado == EstadosCompra.Pagada || d.Compra.Estado == EstadosCompra.Reembolsada)))
            .ToListAsync();
    }
}
