using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class CompraRepository : ICompraRepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public CompraRepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Compra?> GetCompraByCodigoOperacion(Guid codigoOperacion)
        {
            return await ConsultaCompleta()
                .FirstOrDefaultAsync(c => c.CodigoOperacion == codigoOperacion);
        }

        public async Task<Compra?> GetCompraById(int idCompra)
        {
            return await ConsultaCompleta()
                .FirstOrDefaultAsync(c => c.IdCompra == idCompra);
        }

        // "Mis entradas": entradas de compras pagadas o reembolsadas (evento cancelado)
        public async Task<IEnumerable<Entrada>> GetEntradasByUsuario(int idUsuario)
        {
            return await _dbContext.Entrada
                .Include(en => en.DetalleCompra).ThenInclude(d => d.Evento)
                .Include(en => en.DetalleCompra).ThenInclude(d => d.Compra).ThenInclude(c => c.Usuario)
                .Where(en => en.DetalleCompra.Compra.IdUsuario == idUsuario
                          && en.DetalleCompra.Compra.Estado != EstadosCompra.Rechazada)
                .OrderByDescending(en => en.DetalleCompra.Evento.FechaHoraInicio)
                .ThenBy(en => en.NumeroEntrada)
                .ToListAsync();
        }

        // Guarda la compra con su detalle y sus entradas en un solo SaveChanges
        public async Task<bool> CreateCompra(Compra compra)
        {
            await _dbContext.Compra.AddAsync(compra);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }

        private IQueryable<Compra> ConsultaCompleta()
        {
            return _dbContext.Compra
                .Include(c => c.Usuario)
                .Include(c => c.DetalleCompra).ThenInclude(d => d.Evento)
                .Include(c => c.DetalleCompra).ThenInclude(d => d.Entrada);
        }
    }
}
