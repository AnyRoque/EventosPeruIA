using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Core.Services
{
    // US-07 Validación de entradas (check-in) el día del evento
    public class CheckInService : ICheckInService
    {
        private const int HorasAntesDelInicio = 3;

        private readonly ICheckInRepository _checkInRepository;
        private readonly IEventoRepository _eventoRepository;
        private readonly EventosPeruIAContext _dbContext;

        public CheckInService(ICheckInRepository checkInRepository, IEventoRepository eventoRepository, EventosPeruIAContext dbContext)
        {
            _checkInRepository = checkInRepository;
            _eventoRepository = eventoRepository;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<EventoCheckInDTO>> GetEventosDelDia(int idOrganizador)
        {
            var hoy = DateTime.Today;
            var eventos = await _checkInRepository.GetEventosParaCheckIn(idOrganizador, hoy, hoy.AddDays(1));
            return eventos.Select(e => new EventoCheckInDTO
            {
                IdEvento = e.IdEvento,
                Nombre = e.Nombre,
                FechaHoraInicio = e.FechaHoraInicio,
                FechaHoraFin = e.FechaHoraFin,
                Lugar = $"{e.Direccion}, {e.Distrito}"
            });
        }

        public async Task<ResultadoDTO<CheckInResultadoDTO>> ValidarEntrada(int idOrganizador, CheckInRequestDTO dto)
        {
            var evento = await _eventoRepository.GetEventoById(dto.IdEvento);
            if (evento == null) return ResultadoDTO<CheckInResultadoDTO>.Error(404, "Evento no encontrado.");
            if (evento.IdOrganizador != idOrganizador)
                return ResultadoDTO<CheckInResultadoDTO>.Error(403, "Solo puedes validar entradas de tus propios eventos.");

            var ahora = DateTime.Now;
            if (evento.Estado != EstadosEvento.Publicado
                || ahora < evento.FechaHoraInicio.AddHours(-HorasAntesDelInicio)
                || ahora > evento.FechaHoraFin)
                return Rechazo(400, "El check-in de este evento no está habilitado en este momento.");

            // El código del QR debe existir: si no es un código válido, se rechaza
            if (!Guid.TryParse(dto.Codigo.Trim(), out var codigo))
                return await RechazoConContador(evento, "Código no válido.");

            var entrada = await _checkInRepository.GetEntradaByCodigo(codigo);
            if (entrada == null)
                return await RechazoConContador(evento, "Código no válido.");

            if (entrada.DetalleCompra.IdEvento != evento.IdEvento)
                return await RechazoConContador(evento, "Esta entrada no corresponde a este evento.");

            if (entrada.Estado == EstadosEntrada.Anulada)
                return await RechazoConContador(evento, "Entrada anulada.");

            if (entrada.Estado == EstadosEntrada.Utilizada)
                return await RechazoYaUtilizada(evento, entrada.CheckIn?.FechaHora);

            // Entrada válida: se marca como utilizada y se registra el ingreso
            using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var marcada = await _checkInRepository.MarcarUtilizada(entrada.IdEntrada);
                if (!marcada)
                {
                    // Otro dispositivo la validó un instante antes
                    await tx.RollbackAsync();
                    return await RechazoYaUtilizada(evento, ahora);
                }

                await _checkInRepository.CreateCheckIn(new CheckIn
                {
                    IdEntrada = entrada.IdEntrada,
                    IdUsuarioValidador = idOrganizador,
                    FechaHora = ahora
                });
                await tx.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                return await RechazoYaUtilizada(evento, ahora);
            }

            var usuario = entrada.DetalleCompra.Compra.Usuario;
            var resultado = new CheckInResultadoDTO
            {
                Permitido = true,
                Mensaje = "Acceso permitido",
                Asistente = $"{usuario.Nombre} {usuario.Apellido}",
                NumeroEntrada = entrada.NumeroEntrada,
                TotalEntradas = entrada.DetalleCompra.Cantidad,
                FechaUso = ahora,
                Ingresados = await _checkInRepository.ContarIngresados(evento.IdEvento),
                Vendidas = evento.EntradasVendidas
            };
            return ResultadoDTO<CheckInResultadoDTO>.Ok(resultado, resultado.Mensaje);
        }

        public async Task<ResultadoDTO<CheckInResumenDTO>> GetResumen(int idOrganizador, int idEvento)
        {
            var evento = await _eventoRepository.GetEventoById(idEvento);
            if (evento == null) return ResultadoDTO<CheckInResumenDTO>.Error(404, "Evento no encontrado.");
            if (evento.IdOrganizador != idOrganizador)
                return ResultadoDTO<CheckInResumenDTO>.Error(403, "Solo puedes ver el check-in de tus propios eventos.");

            var ingresados = await _checkInRepository.ContarIngresados(idEvento);
            var ultimos = await _checkInRepository.GetUltimosIngresos(idEvento, 10);

            return ResultadoDTO<CheckInResumenDTO>.Ok(new CheckInResumenDTO
            {
                IdEvento = evento.IdEvento,
                Evento = evento.Nombre,
                Ingresados = ingresados,
                Vendidas = evento.EntradasVendidas,
                Contador = $"{ingresados} / {evento.EntradasVendidas}",
                UltimosIngresos = ultimos.Select(c => new IngresoDTO
                {
                    FechaHora = c.FechaHora,
                    Asistente = $"{c.Entrada.DetalleCompra.Compra.Usuario.Nombre} {c.Entrada.DetalleCompra.Compra.Usuario.Apellido}",
                    NumeroEntrada = c.Entrada.NumeroEntrada
                }).ToList()
            });
        }

        // ===================== Métodos de apoyo =====================

        private static ResultadoDTO<CheckInResultadoDTO> Rechazo(int codigo, string mensaje)
            => ResultadoDTO<CheckInResultadoDTO>.Error(codigo, mensaje,
                new CheckInResultadoDTO { Permitido = false, Mensaje = mensaje });

        // Rechazos de la puerta: se responde 200 con Permitido = false (pantalla roja)
        private async Task<ResultadoDTO<CheckInResultadoDTO>> RechazoConContador(Evento evento, string mensaje)
        {
            var datos = new CheckInResultadoDTO
            {
                Permitido = false,
                Mensaje = mensaje,
                Ingresados = await _checkInRepository.ContarIngresados(evento.IdEvento),
                Vendidas = evento.EntradasVendidas
            };
            return ResultadoDTO<CheckInResultadoDTO>.Ok(datos, mensaje);
        }

        private Task<ResultadoDTO<CheckInResultadoDTO>> RechazoYaUtilizada(Evento evento, DateTime? fechaUso)
        {
            var mensaje = fechaUso.HasValue
                ? $"Entrada ya utilizada el {fechaUso.Value:dd/MM/yyyy} a las {fechaUso.Value:HH:mm}."
                : "Entrada ya utilizada.";
            return RechazoConContador(evento, mensaje);
        }
    }
}
