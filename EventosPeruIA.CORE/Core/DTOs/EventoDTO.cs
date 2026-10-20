using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs
{
    // US-02 Búsqueda y filtrado (parámetros de la URL)
    public class EventoFiltroDTO
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "La palabra clave debe tener mínimo 3 caracteres.")]
        public string? Palabra { get; set; }
        public int? IdCategoria { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string? Departamento { get; set; }
        public string? Distrito { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser 1 o mayor.")]
        public int Pagina { get; set; } = 1;
    }

    // Tarjeta de evento en los resultados de búsqueda
    public class EventoListDTO
    {
        public int IdEvento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string PrecioTexto { get; set; } = string.Empty;   // "S/ 60.00" o "Gratis"
        public int CuposDisponibles { get; set; }
        public bool Agotado { get; set; }
        public string? ImagenUrl { get; set; }
    }

    // US-03 Detalle del evento
    public class EventoDetalleDTO
    {
        public int IdEvento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Organizador { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal Precio { get; set; }
        public string PrecioTexto { get; set; } = string.Empty;
        public int AforoMaximo { get; set; }
        public int CuposDisponibles { get; set; }
        public string Estado { get; set; } = string.Empty;

        // Qué mostrar en lugar del botón "Comprar":
        // Disponible, Agotado, Realizado, Cancelado, EsTuEvento, Borrador
        public string EstadoCompra { get; set; } = string.Empty;
        public string MensajeCompra { get; set; } = string.Empty;
        public bool PuedeComprar { get; set; }
        public int MisEntradas { get; set; }

        public string? MotivoCancelacion { get; set; }

        // Opiniones (solo si el evento ya se realizó)
        public double? PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }
        public List<ResenaListDTO> Resenas { get; set; } = new();
    }

    public class ResenaListDTO
    {
        public string Usuario { get; set; } = string.Empty;
        public int Calificacion { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
    }

    // US-09 Publicación y edición de eventos
    public class EventoCreateDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "El nombre debe tener entre 5 y 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(1000, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 1000 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "La fecha y hora de inicio es obligatoria.")]
        public DateTime? FechaHoraInicio { get; set; }

        [Required(ErrorMessage = "La fecha y hora de fin es obligatoria.")]
        public DateTime? FechaHoraFin { get; set; }

        [Required(ErrorMessage = "El departamento es obligatorio.")]
        [StringLength(50)]
        public string Departamento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El distrito es obligatorio.")]
        [StringLength(50)]
        public string Distrito { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "La dirección debe tener entre 5 y 250 caracteres.")]
        public string Direccion { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "El aforo debe ser un número entero entre 1 y 10 000.")]
        public int AforoMaximo { get; set; }

        [Range(typeof(decimal), "0", "9999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "El precio debe estar entre S/ 0.00 y S/ 9 999.99.")]
        public decimal Precio { get; set; }

        [StringLength(500)]
        public string? ImagenUrl { get; set; }

        // true = Publicar, false = Guardar como borrador
        public bool Publicar { get; set; }
    }

    public class EventoUpdateDTO : EventoCreateDTO
    {
        // Debe enviarse en true para confirmar un cambio de fecha, hora o lugar
        // cuando el evento ya tiene entradas vendidas.
        public bool ConfirmarCambios { get; set; }
    }

    // Fila de "Mis eventos"
    public class MiEventoDTO
    {
        public int IdEvento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int EntradasVendidas { get; set; }
        public int AforoMaximo { get; set; }
        public string Vendidas { get; set; } = string.Empty;    // "8 / 200"
        public bool PuedeEditar { get; set; }
        public bool PuedeCancelar { get; set; }
    }

    // US-07: cupos del evento (se consulta cada pocos segundos desde el frontend)
    public class DisponibilidadDTO
    {
        public int IdEvento { get; set; }
        public int AforoMaximo { get; set; }
        public int EntradasVendidas { get; set; }
        public int CuposDisponibles { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
        public bool Agotado { get; set; }
    }
}
