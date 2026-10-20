using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs
{
    // US-06 Compra de entradas
    public class CompraCreateDTO
    {
        // Lo genera el frontend al abrir el checkout (un GUID por intento de compra).
        // Si la misma solicitud llega dos veces (doble clic, recarga, mala conexión),
        // el servidor reconoce el código y no registra otra compra.
        [Required(ErrorMessage = "El código de operación es obligatorio.")]
        public Guid? CodigoOperacion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un evento.")]
        public int IdEvento { get; set; }

        [Range(1, 10, ErrorMessage = "Puedes comprar de 1 a 10 entradas por compra.")]
        public int Cantidad { get; set; }

        // Pago simulado (no se piden si el evento es gratis)
        [StringLength(100)]
        public string? TitularTarjeta { get; set; }

        [RegularExpression(@"^\d{16}$", ErrorMessage = "El número de tarjeta debe tener 16 dígitos.")]
        public string? NumeroTarjeta { get; set; }

        [RegularExpression(@"^(0[1-9]|1[0-2])/\d{2}$", ErrorMessage = "La fecha de vencimiento debe tener el formato MM/AA.")]
        public string? Vencimiento { get; set; }

        [RegularExpression(@"^\d{3}$", ErrorMessage = "El CVV debe tener 3 dígitos.")]
        public string? Cvv { get; set; }
    }

    public class CompraResultadoDTO
    {
        public int IdCompra { get; set; }
        public string NumeroCompra { get; set; } = string.Empty;   // EP-000001
        public string Estado { get; set; } = string.Empty;
        public string Evento { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaCompra { get; set; }
        public List<EntradaDTO> Entradas { get; set; } = new();
    }

    // Voucher de cada entrada (el frontend dibuja el QR con CodigoEntrada)
    public class EntradaDTO
    {
        public int IdEntrada { get; set; }
        public Guid CodigoEntrada { get; set; }
        public int NumeroEntrada { get; set; }
        public int TotalEntradas { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int IdEvento { get; set; }
        public string Evento { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public string Lugar { get; set; } = string.Empty;
        public string Asistente { get; set; } = string.Empty;
        public string NumeroCompra { get; set; } = string.Empty;
        public decimal PrecioPagado { get; set; }
        public bool EventoCancelado { get; set; }
        public string? MotivoCancelacion { get; set; }
    }
}
