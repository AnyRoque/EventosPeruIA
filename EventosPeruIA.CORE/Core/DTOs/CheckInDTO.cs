using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs
{
    // US-10 Validación de entradas (check-in)
    public class CheckInRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona el evento.")]
        public int IdEvento { get; set; }

        // Código leído del QR o escrito manualmente
        [Required(ErrorMessage = "Ingresa o escanea el código de la entrada.")]
        public string Codigo { get; set; } = string.Empty;
    }

    public class CheckInResultadoDTO
    {
        public bool Permitido { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? Asistente { get; set; }
        public int? NumeroEntrada { get; set; }
        public int? TotalEntradas { get; set; }
        public DateTime? FechaUso { get; set; }
        public int Ingresados { get; set; }
        public int Vendidas { get; set; }
    }

    public class EventoCheckInDTO
    {
        public int IdEvento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Lugar { get; set; } = string.Empty;
    }

    public class CheckInResumenDTO
    {
        public int IdEvento { get; set; }
        public string Evento { get; set; } = string.Empty;
        public int Ingresados { get; set; }
        public int Vendidas { get; set; }
        public string Contador { get; set; } = string.Empty;   // "120 / 150"
        public List<IngresoDTO> UltimosIngresos { get; set; } = new();
    }

    public class IngresoDTO
    {
        public DateTime FechaHora { get; set; }
        public string Asistente { get; set; } = string.Empty;
        public int NumeroEntrada { get; set; }
    }
}
