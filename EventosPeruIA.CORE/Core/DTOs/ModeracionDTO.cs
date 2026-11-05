using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs
{
    // US-16 Moderación de eventos, usuarios y reseñas

    // Suspender u ocultar: el motivo es obligatorio
    public class ModeracionMotivoDTO
    {
        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El motivo debe tener entre 10 y 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }

    // Reactivar: el motivo es opcional
    public class ModeracionReactivarDTO
    {
        [StringLength(500, ErrorMessage = "El motivo puede tener como máximo 500 caracteres.")]
        public string? Motivo { get; set; }
    }

    // Buscador y filtro por estado de cada pestaña
    public class ModeracionFiltroDTO
    {
        public string? Buscar { get; set; }
        public string? Estado { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser 1 o mayor.")]
        public int Pagina { get; set; } = 1;
    }

    public class HistorialFiltroDTO
    {
        public string? Accion { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser 1 o mayor.")]
        public int Pagina { get; set; } = 1;
    }

    public class EventoModeracionDTO
    {
        public int IdEvento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Organizador { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool PuedeSuspender { get; set; }
        public bool PuedeReactivar { get; set; }
    }

    public class UsuarioModeracionDTO
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }

    public class ResenaModeracionDTO
    {
        public int IdResena { get; set; }
        public string Evento { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int Calificacion { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;   // Visible / Oculta
        public DateTime FechaPublicacion { get; set; }
    }

    public class HistorialModeracionDTO
    {
        public int IdModeracion { get; set; }
        public DateTime FechaAccion { get; set; }
        public string Administrador { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string TipoElemento { get; set; } = string.Empty;   // Evento / Usuario / Reseña
        public string ElementoAfectado { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }
}
