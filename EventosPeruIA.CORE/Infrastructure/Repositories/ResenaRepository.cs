using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class ResenaRepository : IResenaRepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public ResenaRepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Solo las visibles (las ocultadas por el administrador no se muestran), de la más reciente a la más antigua
        public async Task<IEnumerable<Resena>> GetResenasVisibles(int idEvento)
        {
            return await _dbContext.Resena
                .AsNoTracking()
                .Include(r => r.Usuario)
                .Where(r => r.IdEvento == idEvento && r.Visible)
                .OrderByDescending(r => r.FechaPublicacion)
                .ToListAsync();
        }

        public async Task<Resena?> GetResenaById(int idResena)
        {
            return await _dbContext.Resena
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.IdResena == idResena);
        }

        // Asistió = tiene al menos una entrada marcada como "Utilizada" en el check-in
        public async Task<bool> AsistioAlEvento(int idUsuario, int idEvento)
        {
            return await _dbContext.Entrada
                .AnyAsync(en => en.DetalleCompra.IdEvento == idEvento
                             && en.DetalleCompra.Compra.IdUsuario == idUsuario
                             && en.Estado == EstadosEntrada.Utilizada);
        }

        public async Task<bool> YaReseno(int idUsuario, int idEvento)
        {
            return await _dbContext.Resena.AnyAsync(r => r.IdUsuario == idUsuario && r.IdEvento == idEvento);
        }

        public async Task<bool> CreateResena(Resena resena)
        {
            await _dbContext.Resena.AddAsync(resena);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        // La reseña ya viene rastreada desde GetResenaById: basta con guardar los cambios
        public async Task<bool> UpdateResena(Resena resena)
        {
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteResena(Resena resena)
        {
            _dbContext.Resena.Remove(resena);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
