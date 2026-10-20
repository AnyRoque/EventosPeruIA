using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class CheckInRepository : ICheckInRepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public CheckInRepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Eventos del organizador que se cruzan con el rango indicado (hoy)
        public async Task<IEnumerable<Evento>> GetEventosParaCheckIn(int idOrganizador, DateTime desde, DateTime hasta)
        {
            return await _dbContext.Evento
                .Where(e => e.IdOrganizador == idOrganizador
                         && e.Estado == EstadosEvento.Publicado
                         && e.FechaHoraInicio < hasta
                         && e.FechaHoraFin > desde)
                .OrderBy(e => e.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task<Entrada?> GetEntradaByCodigo(Guid codigo)
        {
            return await _dbContext.Entrada
                .Include(en => en.CheckIn)
                .Include(en => en.DetalleCompra).ThenInclude(d => d.Compra).ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(en => en.CodigoEntrada == codigo);
        }

        // Solo cambia a "Utilizada" si todavía está "Valida". Si dos dispositivos
        // escanean la misma entrada a la vez, solo uno lo logra.
        public async Task<bool> MarcarUtilizada(int idEntrada)
        {
            var filas = await _dbContext.Entrada
                .Where(en => en.IdEntrada == idEntrada && en.Estado == EstadosEntrada.Valida)
                .ExecuteUpdateAsync(s => s.SetProperty(en => en.Estado, EstadosEntrada.Utilizada));
            return filas == 1;
        }

        public async Task<bool> CreateCheckIn(CheckIn checkIn)
        {
            await _dbContext.CheckIn.AddAsync(checkIn);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<int> ContarIngresados(int idEvento)
        {
            return await _dbContext.CheckIn.CountAsync(c => c.Entrada.DetalleCompra.IdEvento == idEvento);
        }

        public async Task<IEnumerable<CheckIn>> GetUltimosIngresos(int idEvento, int cantidad)
        {
            return await _dbContext.CheckIn
                .Include(c => c.Entrada).ThenInclude(en => en.DetalleCompra).ThenInclude(d => d.Compra).ThenInclude(co => co.Usuario)
                .Where(c => c.Entrada.DetalleCompra.IdEvento == idEvento)
                .OrderByDescending(c => c.FechaHora)
                .Take(cantidad)
                .ToListAsync();
        }
    }
}
