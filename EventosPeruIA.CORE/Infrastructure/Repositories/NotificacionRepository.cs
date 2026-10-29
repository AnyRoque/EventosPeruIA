using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Infrastructure.Repositories
{
    public class NotificacionRepository : INotificacionRepository
    {
        private readonly EventosPeruIAContext _dbContext;

        public NotificacionRepository(EventosPeruIAContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Cada usuario solo ve sus propias notificaciones
        public async Task<(IEnumerable<Notificacion> Notificaciones, int Total)> GetNotificaciones(int idUsuario, bool soloNoLeidas, int pagina, int tamanoPagina)
        {
            var query = _dbContext.Notificacion.Where(n => n.IdUsuario == idUsuario);
            if (soloNoLeidas) query = query.Where(n => !n.Leida);

            var total = await query.CountAsync();
            var lista = await query
                .OrderByDescending(n => n.FechaCreacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
            return (lista, total);
        }

        public async Task<int> ContarNoLeidas(int idUsuario)
        {
            return await _dbContext.Notificacion.CountAsync(n => n.IdUsuario == idUsuario && !n.Leida);
        }

        public async Task<bool> MarcarLeida(int idUsuario, int idNotificacion)
        {
            var filas = await _dbContext.Notificacion
                .Where(n => n.IdNotificacion == idNotificacion && n.IdUsuario == idUsuario)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.Leida, true)
                    .SetProperty(n => n.FechaLectura, n => n.FechaLectura ?? (DateTime?)DateTime.Now));
            return filas == 1;
        }

        public async Task<int> MarcarTodasLeidas(int idUsuario)
        {
            return await _dbContext.Notificacion
                .Where(n => n.IdUsuario == idUsuario && !n.Leida)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.Leida, true)
                    .SetProperty(n => n.FechaLectura, (DateTime?)DateTime.Now));
        }

        public async Task<bool> CreateNotificaciones(IEnumerable<Notificacion> notificaciones)
        {
            await _dbContext.Notificacion.AddRangeAsync(notificaciones);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0;
        }

        // Compradores con entradas vigentes (no anuladas) de un evento
        public async Task<IEnumerable<CompradorEventoDTO>> GetCompradores(int idEvento)
        {
            return await _dbContext.DetalleCompra
                .Where(d => d.IdEvento == idEvento
                         && (d.Compra.Estado == EstadosCompra.Pagada || d.Compra.Estado == EstadosCompra.Reembolsada))
                .GroupBy(d => d.Compra.IdUsuario)
                .Select(g => new CompradorEventoDTO
                {
                    IdUsuario = g.Key,
                    Entradas = g.Sum(d => d.Cantidad),
                    Monto = g.Sum(d => d.Subtotal)
                })
                .ToListAsync();
        }

        // Eventos similares para sugerir cuando se cancela uno: misma categoría, publicados, futuros y con cupos
        public async Task<IEnumerable<Evento>> GetEventosSimilares(int idCategoria, int idEventoExcluido, int cantidad)
        {
            var ahora = DateTime.Now;
            return await _dbContext.Evento
                .Where(e => e.IdCategoria == idCategoria
                         && e.IdEvento != idEventoExcluido
                         && e.Estado == EstadosEvento.Publicado
                         && e.FechaHoraInicio > ahora
                         && e.EntradasVendidas < e.AforoMaximo)
                .OrderBy(e => e.FechaHoraInicio)
                .Take(cantidad)
                .ToListAsync();
        }

        // Usuarios con entradas válidas de eventos que empiezan en el rango y que aún no recibieron el recordatorio
        public async Task<IEnumerable<(int IdUsuario, Evento Evento)>> GetPendientesRecordatorio(DateTime desde, DateTime hasta)
        {
            var pendientes = await _dbContext.Entrada
                .Where(en => en.Estado == EstadosEntrada.Valida
                          && en.DetalleCompra.Evento.Estado == EstadosEvento.Publicado
                          && en.DetalleCompra.Evento.FechaHoraInicio > desde
                          && en.DetalleCompra.Evento.FechaHoraInicio <= hasta
                          && !_dbContext.Notificacion.Any(n =>
                                n.IdUsuario == en.DetalleCompra.Compra.IdUsuario
                             && n.IdEvento == en.DetalleCompra.IdEvento
                             && n.Tipo == "Recordatorio"))
                .Select(en => new { en.DetalleCompra.Compra.IdUsuario, en.DetalleCompra.IdEvento })
                .Distinct()
                .ToListAsync();

            var idsEventos = pendientes.Select(p => p.IdEvento).Distinct().ToList();
            var eventos = await _dbContext.Evento
                .Where(e => idsEventos.Contains(e.IdEvento))
                .ToDictionaryAsync(e => e.IdEvento);

            return pendientes.Select(p => (p.IdUsuario, eventos[p.IdEvento])).ToList();
        }
    }
}
