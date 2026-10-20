using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public CategoriaRepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Categoria>> GetCategoriasActivas()
        {
            return await _dbContext.Categoria
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Categoria>> GetCategorias(string? buscar)
        {
            var query = _dbContext.Categoria.Include(c => c.Evento).AsQueryable();
            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(c => c.Nombre.Contains(buscar));
            return await query.OrderBy(c => c.Nombre).ToListAsync();
        }

        public async Task<Categoria?> GetCategoriaById(int id)
        {
            return await _dbContext.Categoria.FirstOrDefaultAsync(c => c.IdCategoria == id);
        }

        public async Task<bool> ExisteNombre(string nombre, int? excluirId = null)
        {
            return await _dbContext.Categoria
                .AnyAsync(c => c.Nombre == nombre && (excluirId == null || c.IdCategoria != excluirId));
        }

        public async Task<int> ContarActivas()
        {
            return await _dbContext.Categoria.CountAsync(c => c.Estado);
        }

        public async Task<int> ContarEventos(int idCategoria)
        {
            return await _dbContext.Evento.CountAsync(e => e.IdCategoria == idCategoria);
        }

        public async Task<bool> CreateCategoria(Categoria categoria)
        {
            await _dbContext.Categoria.AddAsync(categoria);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> UpdateCategoria(Categoria categoria)
        {
            _dbContext.Categoria.Update(categoria);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }
    }
}
