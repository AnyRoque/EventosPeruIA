namespace EventosPeruIA.CORE.Core.DTOs
{
    public class CrearResenaDTO
    {
        public int Calificacion { get; set; } // Valor entero entre 1 y 5
        public string? Comentario { get; set; }
    }

    public class ResenaDTO
    {
        public int IdResena { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class ResumenResenasEventoDTO
    {
        public double PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }
        public List<ResenaDTO> Resenas { get; set; } = new();
    }
}
