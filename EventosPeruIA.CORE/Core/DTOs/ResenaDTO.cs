using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs
{
    // US-12 Reseñas
    public class CrearResenaDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona el evento.")]
        public int IdEvento { get; set; }

        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5 estrellas.")]
        public int Calificacion { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 500 caracteres.")]
        public string Comentario { get; set; } = string.Empty;
    }

    public class EditarResenaDTO
    {
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5 estrellas.")]
        public int Calificacion { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 500 caracteres.")]
        public string Comentario { get; set; } = string.Empty;
    }

    public class ResenaDTO
    {
        public int IdResena { get; set; }
        public int IdEvento { get; set; }
        public int IdUsuario { get; set; }          // el frontend muestra "Editar / Eliminar" solo en la reseña propia
        public string NombreUsuario { get; set; } = null!;
        public int Calificacion { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class ResumenResenasEventoDTO
    {
        public double PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }
        public List<ResenaDTO> Resenas { get; set; } = new();
    }
}
