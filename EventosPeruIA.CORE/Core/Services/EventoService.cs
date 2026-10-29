using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;

namespace EventosPeruIA.CORE.Core.Services
{
    public class EventoService : IEventoService
    {
        private const int TamanoPagina = 12;

        private readonly IEventoRepository _eventoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly INotificacionService _notificacionService;

        public EventoService(IEventoRepository eventoRepository, ICategoriaRepository categoriaRepository, INotificacionService notificacionService)
        {
            _eventoRepository = eventoRepository;
            _categoriaRepository = categoriaRepository;
            _notificacionService = notificacionService;
        }

        // ===================== US-05 Búsqueda y filtrado =====================
        public async Task<ResultadoDTO<PaginaDTO<EventoListDTO>>> BuscarEventos(EventoFiltroDTO filtro)
        {
            if (filtro.Desde.HasValue && filtro.Hasta.HasValue && filtro.Hasta.Value.Date < filtro.Desde.Value.Date)
                return ResultadoDTO<PaginaDTO<EventoListDTO>>.Error(400, "La fecha final debe ser mayor o igual a la fecha inicial.");

            var (eventos, total) = await _eventoRepository.BuscarEventos(filtro, TamanoPagina);

            var pagina = new PaginaDTO<EventoListDTO>
            {
                Items = eventos.Select(e => new EventoListDTO
                {
                    IdEvento = e.IdEvento,
                    Nombre = e.Nombre,
                    Categoria = e.Categoria.Nombre,
                    FechaHoraInicio = e.FechaHoraInicio,
                    Departamento = e.Departamento,
                    Distrito = e.Distrito,
                    Precio = e.Precio,
                    PrecioTexto = TextoPrecio(e.Precio),
                    CuposDisponibles = e.AforoMaximo - e.EntradasVendidas,
                    Agotado = e.EntradasVendidas >= e.AforoMaximo,
                    ImagenUrl = e.ImagenUrl
                }).ToList(),
                Total = total,
                Pagina = filtro.Pagina,
                TamanoPagina = TamanoPagina,
                TotalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina)
            };

            var mensaje = total == 0 ? "No se encontraron eventos con los filtros seleccionados." : $"{total} eventos encontrados.";
            return ResultadoDTO<PaginaDTO<EventoListDTO>>.Ok(pagina, mensaje);
        }

        // ===================== US-09 Detalle del evento =====================
        public async Task<ResultadoDTO<EventoDetalleDTO>> GetDetalle(int idEvento, int? idUsuario)
        {
            var evento = await _eventoRepository.GetEventoConResenas(idEvento);
            var esMiEvento = evento != null && idUsuario.HasValue && evento.IdOrganizador == idUsuario.Value;

            // No existe, está suspendido, o es un borrador de otra persona
            if (evento == null
                || evento.Estado == EstadosEvento.Suspendido
                || (evento.Estado == EstadosEvento.Borrador && !esMiEvento))
                return ResultadoDTO<EventoDetalleDTO>.Error(404, "Este evento no está disponible.");

            var detalle = MapearDetalle(evento);

            if (idUsuario.HasValue && !esMiEvento)
            {
                detalle.MisEntradas = await _eventoRepository.ContarEntradasUsuario(idEvento, idUsuario.Value);
                await _eventoRepository.RegistrarVisita(idEvento, idUsuario.Value); // señal para US-08
            }

            // Qué se muestra en lugar del botón "Comprar"
            var ahora = DateTime.Now;
            if (evento.Estado == EstadosEvento.Cancelado)
                Asignar(detalle, "Cancelado", $"Este evento fue cancelado por el organizador. Motivo: {evento.MotivoCancelacion}");
            else if (evento.Estado == EstadosEvento.Borrador)
                Asignar(detalle, "Borrador", "Este evento aún no está publicado.");
            else if (evento.Estado == EstadosEvento.Finalizado || evento.FechaHoraInicio <= ahora)
                Asignar(detalle, "Realizado", "Este evento ya se realizó.");
            else if (esMiEvento)
                Asignar(detalle, "EsTuEvento", "Es tu evento.");
            else if (detalle.CuposDisponibles <= 0)
                Asignar(detalle, "Agotado", "Agotado");
            else
            {
                Asignar(detalle, "Disponible", detalle.MisEntradas > 0 ? $"Ya tienes {detalle.MisEntradas} entradas." : "Comprar");
                detalle.PuedeComprar = true;
            }

            // Opiniones: solo si el evento ya se realizó
            if (detalle.EstadoCompra == "Realizado" && evento.Resena.Any())
            {
                detalle.TotalResenas = evento.Resena.Count;
                detalle.PromedioCalificacion = Math.Round(evento.Resena.Average(r => (double)r.Calificacion), 1);
                detalle.Resenas = evento.Resena
                    .OrderByDescending(r => r.FechaPublicacion)
                    .Select(r => new ResenaListDTO
                    {
                        Usuario = r.Usuario.Nombre + " " + r.Usuario.Apellido.Substring(0, 1) + ".",
                        Calificacion = r.Calificacion,
                        Comentario = r.Comentario,
                        FechaPublicacion = r.FechaPublicacion
                    }).ToList();
            }

            return ResultadoDTO<EventoDetalleDTO>.Ok(detalle);
        }

        // ===================== US-10 Disponibilidad (cupos) =====================
        public async Task<ResultadoDTO<DisponibilidadDTO>> GetDisponibilidad(int idEvento)
        {
            var evento = await _eventoRepository.GetEventoById(idEvento);
            if (evento == null || evento.Estado == EstadosEvento.Borrador || evento.Estado == EstadosEvento.Suspendido)
                return ResultadoDTO<DisponibilidadDTO>.Error(404, "Este evento no está disponible.");

            var cupos = evento.Estado == EstadosEvento.Cancelado ? 0 : evento.AforoMaximo - evento.EntradasVendidas;
            return ResultadoDTO<DisponibilidadDTO>.Ok(new DisponibilidadDTO
            {
                IdEvento = evento.IdEvento,
                AforoMaximo = evento.AforoMaximo,
                EntradasVendidas = evento.EntradasVendidas,
                CuposDisponibles = cupos,
                PorcentajeOcupacion = Math.Round(evento.EntradasVendidas * 100m / evento.AforoMaximo, 1),
                Agotado = cupos <= 0
            });
        }

        // ===================== US-03 Publicación y edición =====================
        public async Task<IEnumerable<MiEventoDTO>> GetMisEventos(int idOrganizador)
        {
            var ahora = DateTime.Now;
            var eventos = await _eventoRepository.GetEventosByOrganizador(idOrganizador);
            return eventos.Select(e => new MiEventoDTO
            {
                IdEvento = e.IdEvento,
                Nombre = e.Nombre,
                FechaHoraInicio = e.FechaHoraInicio,
                Estado = e.Estado,
                EntradasVendidas = e.EntradasVendidas,
                AforoMaximo = e.AforoMaximo,
                Vendidas = $"{e.EntradasVendidas} / {e.AforoMaximo}",
                PuedeEditar = PuedeEditarse(e, ahora),
                PuedeCancelar = e.Estado == EstadosEvento.Publicado && e.FechaHoraInicio > ahora
            });
        }

        public async Task<ResultadoDTO<EventoDetalleDTO>> CreateEvento(int idOrganizador, EventoCreateDTO dto)
        {
            var error = await ValidarDatos(dto);
            if (error != null) return ResultadoDTO<EventoDetalleDTO>.Error(400, error);

            var evento = new Evento
            {
                IdOrganizador = idOrganizador,
                Estado = dto.Publicar ? EstadosEvento.Publicado : EstadosEvento.Borrador,
                EntradasVendidas = 0,
                FechaCreacion = DateTime.Now
            };
            CopiarDatos(dto, evento);

            await _eventoRepository.CreateEvento(evento);

            var creado = await _eventoRepository.GetEventoById(evento.IdEvento);
            var mensaje = dto.Publicar ? "Evento publicado correctamente." : "Borrador guardado.";
            return ResultadoDTO<EventoDetalleDTO>.Ok(MapearDetalle(creado!), mensaje, 201);
        }

        public async Task<ResultadoDTO<EventoDetalleDTO>> UpdateEvento(int idOrganizador, int idEvento, EventoUpdateDTO dto)
        {
            var evento = await _eventoRepository.GetEventoById(idEvento);
            if (evento == null) return ResultadoDTO<EventoDetalleDTO>.Error(404, "Evento no encontrado.");
            if (evento.IdOrganizador != idOrganizador)
                return ResultadoDTO<EventoDetalleDTO>.Error(403, "Solo el organizador del evento puede editarlo.");

            var ahora = DateTime.Now;
            if (evento.Estado == EstadosEvento.Cancelado || evento.Estado == EstadosEvento.Finalizado || evento.FechaHoraInicio <= ahora)
                return ResultadoDTO<EventoDetalleDTO>.Error(400, "No se pueden editar eventos cancelados o finalizados.");
            if (evento.Estado == EstadosEvento.Suspendido)
                return ResultadoDTO<EventoDetalleDTO>.Error(400, "El evento está suspendido por el administrador y no se puede editar.");

            var error = await ValidarDatos(dto, evento.IdCategoria);
            if (error != null) return ResultadoDTO<EventoDetalleDTO>.Error(400, error);

            // Restricción de aforo (US-10): nunca por debajo de lo vendido
            if (dto.AforoMaximo < evento.EntradasVendidas)
                return ResultadoDTO<EventoDetalleDTO>.Error(400,
                    $"El aforo no puede ser menor a las {evento.EntradasVendidas} entradas ya vendidas.");

            // Cambio de fecha, hora o lugar con entradas vendidas: pedir confirmación
            var cambiaProgramacion = evento.FechaHoraInicio != dto.FechaHoraInicio!.Value
                                  || evento.FechaHoraFin != dto.FechaHoraFin!.Value
                                  || evento.Departamento != dto.Departamento.Trim()
                                  || evento.Distrito != dto.Distrito.Trim()
                                  || evento.Direccion != dto.Direccion.Trim();

            if (cambiaProgramacion && evento.EntradasVendidas > 0 && !dto.ConfirmarCambios)
            {
                var compradores = await _eventoRepository.ContarCompradores(idEvento);
                return ResultadoDTO<EventoDetalleDTO>.Error(409,
                    $"Se notificará a {compradores} compradores. Vuelve a enviar con confirmarCambios=true para guardar.");
            }

            // Datos anteriores para el aviso de cambio (US-15)
            var inicioAnterior = evento.FechaHoraInicio;
            var lugarAnterior = NotificacionService.Lugar(evento);

            // El precio nuevo solo aplica a compras nuevas: cada compra guarda el precio que se pagó.
            CopiarDatos(dto, evento);
            evento.FechaModificacion = DateTime.Now;

            try
            {
                await _eventoRepository.UpdateEvento(evento);
            }
            catch (DbUpdateException)
            {
                // Si justo se vendieron entradas y el aforo nuevo quedó por debajo, la BD lo rechaza
                return ResultadoDTO<EventoDetalleDTO>.Error(409,
                    "El aforo no puede ser menor a las entradas ya vendidas. Actualiza la página e inténtalo nuevamente.");
            }

            // US-15: avisar a los compradores el cambio de fecha, hora o lugar
            if (cambiaProgramacion && evento.EntradasVendidas > 0 && evento.Estado == EstadosEvento.Publicado)
                await _notificacionService.NotificarCambioProgramacion(evento, inicioAnterior, lugarAnterior);

            var actualizado = await _eventoRepository.GetEventoById(idEvento);
            return ResultadoDTO<EventoDetalleDTO>.Ok(MapearDetalle(actualizado!), "Evento actualizado correctamente.");
        }

        public async Task<ResultadoDTO<EventoDetalleDTO>> PublicarEvento(int idOrganizador, int idEvento)
        {
            var evento = await _eventoRepository.GetEventoById(idEvento);
            if (evento == null) return ResultadoDTO<EventoDetalleDTO>.Error(404, "Evento no encontrado.");
            if (evento.IdOrganizador != idOrganizador)
                return ResultadoDTO<EventoDetalleDTO>.Error(403, "Solo el organizador del evento puede publicarlo.");
            if (evento.Estado != EstadosEvento.Borrador)
                return ResultadoDTO<EventoDetalleDTO>.Error(400, "Solo se pueden publicar eventos en borrador.");
            if (evento.FechaHoraInicio <= DateTime.Now)
                return ResultadoDTO<EventoDetalleDTO>.Error(400, "La fecha de inicio debe ser posterior a la fecha actual.");
            if (!evento.Categoria.Estado)
                return ResultadoDTO<EventoDetalleDTO>.Error(400, "La categoría del evento está inactiva. Elige otra categoría.");

            evento.Estado = EstadosEvento.Publicado;
            evento.FechaModificacion = DateTime.Now;
            await _eventoRepository.UpdateEvento(evento);
            return ResultadoDTO<EventoDetalleDTO>.Ok(MapearDetalle(evento), "Evento publicado correctamente.");
        }

        // Un borrador se puede eliminar; un evento publicado solo se puede cancelar (US-04)
        public async Task<ResultadoDTO<object>> DeleteEvento(int idOrganizador, int idEvento)
        {
            var evento = await _eventoRepository.GetEventoById(idEvento);
            if (evento == null) return ResultadoDTO<object>.Error(404, "Evento no encontrado.");
            if (evento.IdOrganizador != idOrganizador)
                return ResultadoDTO<object>.Error(403, "Solo el organizador del evento puede eliminarlo.");
            if (evento.Estado != EstadosEvento.Borrador)
                return ResultadoDTO<object>.Error(400, "Un evento publicado no se puede eliminar, solo cancelar.");

            await _eventoRepository.DeleteEvento(evento);
            return ResultadoDTO<object>.Ok(null, "Borrador eliminado.");
        }

        // ===================== Métodos de apoyo =====================

        // Validaciones que no cubren los atributos del DTO
        private async Task<string?> ValidarDatos(EventoCreateDTO dto, int? idCategoriaActual = null)
        {
            if (dto.FechaHoraInicio!.Value <= DateTime.Now)
                return "La fecha de inicio debe ser posterior a la fecha actual.";
            if (dto.FechaHoraFin!.Value <= dto.FechaHoraInicio.Value)
                return "La fecha de fin debe ser posterior a la fecha de inicio.";
            if (decimal.Round(dto.Precio, 2) != dto.Precio)
                return "El precio solo puede tener 2 decimales.";

            var categoria = await _categoriaRepository.GetCategoriaById(dto.IdCategoria);
            if (categoria == null) return "La categoría no existe.";
            // Una categoría inactiva no se puede elegir, salvo que el evento ya la tuviera
            if (!categoria.Estado && dto.IdCategoria != idCategoriaActual) return "La categoría seleccionada no está activa.";

            return null;
        }

        private static void CopiarDatos(EventoCreateDTO dto, Evento evento)
        {
            evento.Nombre = dto.Nombre.Trim();
            evento.Descripcion = dto.Descripcion.Trim();
            evento.IdCategoria = dto.IdCategoria;
            evento.FechaHoraInicio = dto.FechaHoraInicio!.Value;
            evento.FechaHoraFin = dto.FechaHoraFin!.Value;
            evento.Departamento = dto.Departamento.Trim();
            evento.Distrito = dto.Distrito.Trim();
            evento.Direccion = dto.Direccion.Trim();
            evento.AforoMaximo = dto.AforoMaximo;
            evento.Precio = dto.Precio;
            evento.ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim();
        }

        private static bool PuedeEditarse(Evento e, DateTime ahora) =>
            (e.Estado == EstadosEvento.Borrador || e.Estado == EstadosEvento.Publicado) && e.FechaHoraInicio > ahora;

        private static string TextoPrecio(decimal precio) => precio == 0 ? "Gratis" : $"S/ {precio:0.00}";

        private static void Asignar(EventoDetalleDTO d, string estado, string mensaje)
        {
            d.EstadoCompra = estado;
            d.MensajeCompra = mensaje;
        }

        private static EventoDetalleDTO MapearDetalle(Evento e) => new()
        {
            IdEvento = e.IdEvento,
            Nombre = e.Nombre,
            Descripcion = e.Descripcion,
            IdCategoria = e.IdCategoria,
            Categoria = e.Categoria.Nombre,
            Organizador = e.Organizador.NombreOrganizador ?? (e.Organizador.Nombre + " " + e.Organizador.Apellido),
            FechaHoraInicio = e.FechaHoraInicio,
            FechaHoraFin = e.FechaHoraFin,
            Departamento = e.Departamento,
            Distrito = e.Distrito,
            Direccion = e.Direccion,
            ImagenUrl = e.ImagenUrl,
            Precio = e.Precio,
            PrecioTexto = TextoPrecio(e.Precio),
            AforoMaximo = e.AforoMaximo,
            CuposDisponibles = e.Estado == EstadosEvento.Cancelado ? 0 : e.AforoMaximo - e.EntradasVendidas,
            Estado = e.Estado,
            MotivoCancelacion = e.MotivoCancelacion
        };
    }
}
