using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs
{
    // US-14 Asistente conversacional PeruIA
    public class PeruIARequestDTO
    {
        [Required(ErrorMessage = "Escribe tu pregunta.")]
        [StringLength(500, ErrorMessage = "La pregunta puede tener como máximo 500 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;

        // Mensajes anteriores de la conversación (el frontend envía los últimos 10).
        // "Nueva conversación" = enviar la lista vacía.
        [MaxLength(10, ErrorMessage = "El historial puede tener como máximo 10 mensajes.")]
        public List<PeruIAMensajeDTO> Historial { get; set; } = new();
    }

    public class PeruIAMensajeDTO
    {
        public string Rol { get; set; } = "usuario";   // "usuario" o "asistente"
        public string Texto { get; set; } = string.Empty;
    }

    public class PeruIAResponseDTO
    {
        public bool Disponible { get; set; } = true;
        public string Respuesta { get; set; } = string.Empty;
        public List<PeruIAEventoDTO> Eventos { get; set; } = new();
        public string? EnlaceBuscador { get; set; }    // solo si PeruIA no está disponible
    }

    // Tarjeta de evento recomendado
    public class PeruIAEventoDTO
    {
        public int IdEvento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public string Lugar { get; set; } = string.Empty;
        public string PrecioTexto { get; set; } = string.Empty;
        public int CuposDisponibles { get; set; }
        public string Motivo { get; set; } = string.Empty;   // por qué se recomienda
    }
}
