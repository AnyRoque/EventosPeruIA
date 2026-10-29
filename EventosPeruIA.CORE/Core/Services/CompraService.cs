using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;

namespace EventosPeruIA.CORE.Core.Services
{
    // US-06 Compra de entradas con generación de código QR / voucher digital
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _compraRepository;
        private readonly IEventoRepository _eventoRepository;
        private readonly EventosPeruIAContext _dbContext;
        private readonly INotificacionService _notificacionService;

        public CompraService(ICompraRepository compraRepository, IEventoRepository eventoRepository,
                             EventosPeruIAContext dbContext, INotificacionService notificacionService)
        {
            _compraRepository = compraRepository;
            _eventoRepository = eventoRepository;
            _dbContext = dbContext;
            _notificacionService = notificacionService;
        }

        public async Task<ResultadoDTO<CompraResultadoDTO>> ComprarEntradas(int idUsuario, CompraCreateDTO dto)
        {
            var codigo = dto.CodigoOperacion!.Value;

            // 1. Idempotencia: si esta operación ya se procesó, se devuelve el mismo resultado
            var previa = await _compraRepository.GetCompraByCodigoOperacion(codigo);
            if (previa != null) return RespuestaCompraPrevia(previa, idUsuario);

            // 2. Validaciones del evento
            var evento = await _eventoRepository.GetEventoById(dto.IdEvento);
            if (evento == null || evento.Estado != EstadosEvento.Publicado)
                return ResultadoDTO<CompraResultadoDTO>.Error(404, "Este evento no está disponible para la venta.");
            if (evento.FechaHoraInicio <= DateTime.Now)
                return ResultadoDTO<CompraResultadoDTO>.Error(400, "Este evento ya se realizó.");
            if (evento.IdOrganizador == idUsuario)
                return ResultadoDTO<CompraResultadoDTO>.Error(400, "No puedes comprar entradas de tu propio evento.");

            var disponibles = evento.AforoMaximo - evento.EntradasVendidas;
            if (dto.Cantidad > disponibles)
                return ResultadoDTO<CompraResultadoDTO>.Error(409, $"Ya no quedan entradas suficientes. Disponibles: {disponibles}");

            // 3. Pago simulado (solo si el evento no es gratis)
            var esGratis = evento.Precio == 0;
            if (!esGratis)
            {
                var errorPago = ValidarTarjeta(dto);
                if (errorPago != null) return ResultadoDTO<CompraResultadoDTO>.Error(400, errorPago);
            }

            var total = evento.Precio * dto.Cantidad;
            var compra = new Compra
            {
                IdUsuario = idUsuario,
                CodigoOperacion = codigo,
                MontoTotal = total,
                TitularTarjeta = esGratis ? null : dto.TitularTarjeta!.Trim().ToUpper(),
                UltimosDigitos = esGratis ? null : dto.NumeroTarjeta!.Substring(12, 4),
                FechaCompra = DateTime.Now
            };
            compra.DetalleCompra.Add(new DetalleCompra
            {
                IdEvento = evento.IdEvento,
                Cantidad = dto.Cantidad,
                PrecioUnitario = evento.Precio,
                Subtotal = total
            });

            // Tarjeta de prueba terminada en 0000 = pago rechazado (no se descuentan cupos)
            if (!esGratis && dto.NumeroTarjeta!.EndsWith("0000"))
            {
                compra.Estado = EstadosCompra.Rechazada;
                await _compraRepository.CreateCompra(compra);
                return ResultadoDTO<CompraResultadoDTO>.Error(402,
                    "El pago fue rechazado. Verifica los datos de tu tarjeta o usa otra.", Mapear(compra));
            }

            // 4. Cupos + compra + entradas en una sola transacción
            using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var hayCupos = await _eventoRepository.DescontarCupos(evento.IdEvento, dto.Cantidad);
                if (!hayCupos)
                {
                    await tx.RollbackAsync();
                    await _dbContext.Entry(evento).ReloadAsync();   // trae los cupos actuales
                    var quedan = Math.Max(0, evento.AforoMaximo - evento.EntradasVendidas);
                    return ResultadoDTO<CompraResultadoDTO>.Error(409, $"Ya no quedan entradas suficientes. Disponibles: {quedan}");
                }

                compra.Estado = EstadosCompra.Pagada;
                var detalle = compra.DetalleCompra.First();
                for (var i = 1; i <= dto.Cantidad; i++)
                {
                    detalle.Entrada.Add(new Entrada
                    {
                        CodigoEntrada = Guid.NewGuid(),   // contenido del QR, único y no adivinable
                        NumeroEntrada = i,
                        Estado = EstadosEntrada.Valida,
                        FechaGeneracion = DateTime.Now
                    });
                }

                await _compraRepository.CreateCompra(compra);
                await tx.CommitAsync();
            }
            catch (DbUpdateException)
            {
                // Dos solicitudes con el mismo código llegaron a la vez: la BD aceptó solo una
                await tx.RollbackAsync();
                _dbContext.ChangeTracker.Clear();
                var existente = await _compraRepository.GetCompraByCodigoOperacion(codigo);
                if (existente != null) return RespuestaCompraPrevia(existente, idUsuario);
                return ResultadoDTO<CompraResultadoDTO>.Error(500, "No se pudo completar la compra. Inténtalo nuevamente.");
            }

            var guardada = await _compraRepository.GetCompraById(compra.IdCompra);
            var resultado = Mapear(guardada!);

            // US-15: confirmación de compra y, si se acabaron los cupos, aviso al organizador.
            // Si falla un aviso, la compra igual queda registrada.
            try
            {
                await _notificacionService.NotificarCompra(idUsuario, evento, dto.Cantidad, resultado.NumeroCompra);
                await _dbContext.Entry(evento).ReloadAsync();   // trae los cupos actualizados
                if (evento.EntradasVendidas >= evento.AforoMaximo)
                    await _notificacionService.NotificarAgotado(evento);
            }
            catch (Exception)
            {
            }

            return ResultadoDTO<CompraResultadoDTO>.Ok(resultado,
                $"Compra realizada con éxito. N° de compra: {resultado.NumeroCompra}", 201);
        }

        public async Task<ResultadoDTO<CompraResultadoDTO>> GetCompra(int idUsuario, int idCompra)
        {
            var compra = await _compraRepository.GetCompraById(idCompra);
            if (compra == null || compra.IdUsuario != idUsuario)
                return ResultadoDTO<CompraResultadoDTO>.Error(404, "Compra no encontrada.");
            return ResultadoDTO<CompraResultadoDTO>.Ok(Mapear(compra));
        }

        public async Task<IEnumerable<EntradaDTO>> GetMisEntradas(int idUsuario)
        {
            var entradas = await _compraRepository.GetEntradasByUsuario(idUsuario);
            return entradas.Select(en => MapearEntrada(en, en.DetalleCompra, en.DetalleCompra.Compra));
        }

        // ===================== Métodos de apoyo =====================

        private static ResultadoDTO<CompraResultadoDTO> RespuestaCompraPrevia(Compra previa, int idUsuario)
        {
            if (previa.IdUsuario != idUsuario)
                return ResultadoDTO<CompraResultadoDTO>.Error(409, "El código de operación ya fue usado.");
            if (previa.Estado == EstadosCompra.Rechazada)
                return ResultadoDTO<CompraResultadoDTO>.Error(402,
                    "El pago fue rechazado. Para reintentar, inicia una nueva compra.", Mapear(previa));

            var dto = Mapear(previa);
            return ResultadoDTO<CompraResultadoDTO>.Ok(dto,
                $"Esta compra ya fue procesada. N° de compra: {dto.NumeroCompra}");
        }

        private static string? ValidarTarjeta(CompraCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TitularTarjeta)) return "El nombre del titular es obligatorio.";
            if (string.IsNullOrWhiteSpace(dto.NumeroTarjeta)) return "El número de tarjeta es obligatorio.";
            if (string.IsNullOrWhiteSpace(dto.Vencimiento)) return "La fecha de vencimiento es obligatoria.";
            if (string.IsNullOrWhiteSpace(dto.Cvv)) return "El CVV es obligatorio.";

            // MM/AA: la tarjeta vence al terminar ese mes
            var mes = int.Parse(dto.Vencimiento.Substring(0, 2));
            var anio = 2000 + int.Parse(dto.Vencimiento.Substring(3, 2));
            var finDeMes = new DateTime(anio, mes, 1).AddMonths(1);
            if (finDeMes <= DateTime.Today) return "La tarjeta está vencida.";

            return null;
        }

        private static string NumeroCompra(int idCompra) => $"EP-{idCompra:D6}";

        private static CompraResultadoDTO Mapear(Compra compra)
        {
            var detalle = compra.DetalleCompra.First();
            return new CompraResultadoDTO
            {
                IdCompra = compra.IdCompra,
                NumeroCompra = NumeroCompra(compra.IdCompra),
                Estado = compra.Estado,
                Evento = detalle.Evento?.Nombre ?? string.Empty,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                MontoTotal = compra.MontoTotal,
                FechaCompra = compra.FechaCompra,
                Entradas = detalle.Entrada
                    .OrderBy(en => en.NumeroEntrada)
                    .Select(en => MapearEntrada(en, detalle, compra))
                    .ToList()
            };
        }

        private static EntradaDTO MapearEntrada(Entrada en, DetalleCompra detalle, Compra compra)
        {
            var evento = detalle.Evento;
            return new EntradaDTO
            {
                IdEntrada = en.IdEntrada,
                CodigoEntrada = en.CodigoEntrada,
                NumeroEntrada = en.NumeroEntrada,
                TotalEntradas = detalle.Cantidad,
                Estado = en.Estado,
                IdEvento = detalle.IdEvento,
                Evento = evento?.Nombre ?? string.Empty,
                FechaHoraInicio = evento?.FechaHoraInicio ?? default,
                Lugar = evento == null ? string.Empty : $"{evento.Direccion}, {evento.Distrito}, {evento.Departamento}",
                Asistente = compra.Usuario == null ? string.Empty : $"{compra.Usuario.Nombre} {compra.Usuario.Apellido}",
                NumeroCompra = NumeroCompra(compra.IdCompra),
                PrecioPagado = detalle.PrecioUnitario,
                EventoCancelado = evento?.Estado == EstadosEvento.Cancelado,
                MotivoCancelacion = evento?.MotivoCancelacion
            };
        }
    }
}
