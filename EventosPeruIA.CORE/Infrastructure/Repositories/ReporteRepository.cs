using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories;

// US-11 Datos para los reportes de ventas del organizador
public class ReporteRepository : IReporteRepository
{
    private readonly EventosPeruIAContext _dbContext;

    public ReporteRepository(EventosPeruIAContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Eventos propios del organizador (los borradores no tienen ventas)
    public async Task<IEnumerable<Evento>> GetEventosOrganizador(int idOrganizador, int? idEvento)
    {
        var query = _dbContext.Evento
            .AsNoTracking()
            .Where(e => e.IdOrganizador == idOrganizador && e.Estado != EstadosEvento.Borrador);

        if (idEvento.HasValue)
            query = query.Where(e => e.IdEvento == idEvento.Value);

        return await query.OrderBy(e => e.FechaHoraInicio).ToListAsync();
    }

    // Ventas de sus eventos: compras pagadas y reembolsadas (las rechazadas o pendientes no son ventas)
    public async Task<IEnumerable<DetalleCompra>> GetVentas(int idOrganizador, FiltroReporteDTO filtro)
    {
        var query = _dbContext.DetalleCompra
            .AsNoTracking()
            .Include(d => d.Compra).ThenInclude(c => c.Usuario)
            .Include(d => d.Evento)
            .Include(d => d.Entrada)
            .Where(d => d.Evento.IdOrganizador == idOrganizador
                     && d.Evento.Estado != EstadosEvento.Borrador
                     && (d.Compra.Estado == EstadosCompra.Pagada || d.Compra.Estado == EstadosCompra.Reembolsada));

        if (filtro.IdEvento.HasValue)
            query = query.Where(d => d.IdEvento == filtro.IdEvento.Value);

        if (filtro.Desde.HasValue)
        {
            var desde = filtro.Desde.Value.Date;
            query = query.Where(d => d.Compra.FechaCompra >= desde);
        }

        if (filtro.Hasta.HasValue)
        {
            var hastaExclusivo = filtro.Hasta.Value.Date.AddDays(1);   // incluye todo el día "hasta"
            query = query.Where(d => d.Compra.FechaCompra < hastaExclusivo);
        }

        return await query.OrderByDescending(d => d.Compra.FechaCompra).ToListAsync();
    }
}
