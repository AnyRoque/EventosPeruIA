using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class EventoRepository : IEventoRepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public EventoRepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        // US-02: solo eventos publicados que aún no terminan
        public async Task<(IEnumerable<Evento> Eventos, int Total)> BuscarEventos(EventoFiltroDTO filtro, int tamanoPagina)
        {
            var ahora = DateTime.Now;
            var query = _dbContext.Evento
                .Include(e => e.Categoria)
                .Where(e => e.Estado == EstadosEvento.Publicado && e.FechaHoraFin > ahora);

            if (!string.IsNullOrWhiteSpace(filtro.Palabra))
            {
                // Sin importar mayúsculas ni tildes
                var palabra = filtro.Palabra.Trim();
                query = query.Where(e =>
                    EF.Functions.Collate(e.Nombre, "Latin1_General_CI_AI").Contains(palabra) ||
                    EF.Functions.Collate(e.Descripcion, "Latin1_General_CI_AI").Contains(palabra));
            }

            if (filtro.IdCategoria.HasValue)
                query = query.Where(e => e.IdCategoria == filtro.IdCategoria.Value);

            if (filtro.Desde.HasValue)
                query = query.Where(e => e.FechaHoraInicio >= filtro.Desde.Value.Date);

            if (filtro.Hasta.HasValue)
            {
                var hastaFinDelDia = filtro.Hasta.Value.Date.AddDays(1);
                query = query.Where(e => e.FechaHoraInicio < hastaFinDelDia);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Departamento))
                query = query.Where(e => e.Departamento == filtro.Departamento);

            if (!string.IsNullOrWhiteSpace(filtro.Distrito))
                query = query.Where(e => e.Distrito == filtro.Distrito);

            var total = await query.CountAsync();
            var eventos = await query
                .OrderBy(e => e.FechaHoraInicio)
                .Skip((filtro.Pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            return (eventos, total);
        }

        public async Task<Evento?> GetEventoById(int idEvento)
        {
            return await _dbContext.Evento
                .Include(e => e.Categoria)
                .Include(e => e.Organizador)
                .FirstOrDefaultAsync(e => e.IdEvento == idEvento);
        }

        public async Task<Evento?> GetEventoConResenas(int idEvento)
        {
            return await _dbContext.Evento
                .Include(e => e.Categoria)
                .Include(e => e.Organizador)
                .Include(e => e.Resena.Where(r => r.Visible)).ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(e => e.IdEvento == idEvento);
        }

        public async Task<IEnumerable<Evento>> GetEventosByOrganizador(int idOrganizador)
        {
            return await _dbContext.Evento
                .Where(e => e.IdOrganizador == idOrganizador)
                .OrderByDescending(e => e.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task<bool> CreateEvento(Evento evento)
        {
            await _dbContext.Evento.AddAsync(evento);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> UpdateEvento(Evento evento)
        {
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteEvento(Evento evento)
        {
            _dbContext.Evento.Remove(evento);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }

        // Compradores con entradas vigentes (para avisar cambios)
        public async Task<int> ContarCompradores(int idEvento)
        {
            return await _dbContext.DetalleCompra
                .Where(d => d.IdEvento == idEvento && d.Compra.Estado == EstadosCompra.Pagada)
                .Select(d => d.Compra.IdUsuario)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> ContarEntradasUsuario(int idEvento, int idUsuario)
        {
            return await _dbContext.Entrada
                .CountAsync(en => en.DetalleCompra.IdEvento == idEvento
                               && en.DetalleCompra.Compra.IdUsuario == idUsuario
                               && en.Estado != EstadosEntrada.Anulada);
        }

        public async Task RegistrarVisita(int idEvento, int idUsuario)
        {
            await _dbContext.EventoVisita.AddAsync(new EventoVisita
            {
                IdEvento = idEvento,
                IdUsuario = idUsuario,
                FechaVisita = DateTime.Now
            });
            await _dbContext.SaveChangesAsync();
        }

        // Control de aforo (US-06 / US-07): descuenta los cupos en una sola operación
        // en la base de datos, solo si todavía alcanzan. Si dos personas compran los
        // últimos cupos al mismo tiempo, solo una lo logra (devuelve true).
        public async Task<bool> DescontarCupos(int idEvento, int cantidad)
        {
            var ahora = DateTime.Now;
            var filas = await _dbContext.Evento
                .Where(e => e.IdEvento == idEvento
                         && e.Estado == EstadosEvento.Publicado
                         && e.FechaHoraInicio > ahora
                         && e.EntradasVendidas + cantidad <= e.AforoMaximo)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.EntradasVendidas, e => e.EntradasVendidas + cantidad));
            return filas == 1;
        }
    }
}
