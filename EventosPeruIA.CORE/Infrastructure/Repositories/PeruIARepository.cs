using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class PeruIARepository : IPeruIARepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public PeruIARepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Solo eventos publicados, que aún no empiezan y con cupos (criterio 7).
        // Se consulta en cada pregunta, así los cambios se ven al instante (criterio 8).
        public async Task<IEnumerable<Evento>> GetEventosDisponibles(int cantidad)
        {
            var ahora = DateTime.Now;
            return await _dbContext.Evento
                .AsNoTracking()
                .Include(e => e.Categoria)
                .Where(e => e.Estado == EstadosEvento.Publicado
                         && e.FechaHoraInicio > ahora
                         && e.EntradasVendidas < e.AforoMaximo)
                .OrderBy(e => e.FechaHoraInicio)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<PreguntaFrecuente>> GetPreguntasFrecuentes()
        {
            return await _dbContext.PreguntaFrecuente
                .AsNoTracking()
                .Where(p => p.Estado)
                .ToListAsync();
        }
    }
}
