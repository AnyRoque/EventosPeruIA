using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs
{
    public class InteresDTO
    {
        public int IdInteres { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class UsuarioInteresesUpdateDTO
    {
        [Required(ErrorMessage = "Selecciona tus intereses.")]
        [MinLength(1, ErrorMessage = "Selecciona al menos un interés.")]
        [MaxLength(5, ErrorMessage = "Puedes seleccionar hasta 5 intereses.")]
        public int[] IdIntereses { get; set; } = Array.Empty<int>();
    }

    public class EventoRecomendadoDTO : EventoListDTO
    {
        public string Motivo { get; set; } = string.Empty;
    }

    public class RecomendacionResultadoDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public bool EsPopular { get; set; }
        public bool TieneIntereses { get; set; }
        public List<EventoRecomendadoDTO> Eventos { get; set; } = new();
    }
}
