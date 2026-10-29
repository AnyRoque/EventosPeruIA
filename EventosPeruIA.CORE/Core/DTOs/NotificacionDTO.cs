namespace EventosPeruIA.CORE.Core.DTOs
{
    // US-15 Notificaciones
    public class NotificacionDTO
    {
        public int IdNotificacion { get; set; }
        public string Tipo { get; set; } = string.Empty;    // Compra, Recordatorio, Cambio, Cancelacion, Agotado, Suspension
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public int? IdEvento { get; set; }                  // enlace al detalle del evento
        public bool Leida { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class ContadorNotificacionesDTO
    {
        public int NoLeidas { get; set; }
    }

    // Datos de un comprador con entradas vigentes en un evento
    public class CompradorEventoDTO
    {
        public int IdUsuario { get; set; }
        public int Entradas { get; set; }
        public decimal Monto { get; set; }
    }
}
