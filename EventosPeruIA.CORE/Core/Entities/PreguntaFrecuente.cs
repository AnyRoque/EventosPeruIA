namespace EventosPeruIA.CORE.Core.Entities;

public partial class PreguntaFrecuente
{
    public int IdPregunta { get; set; }
    public string Pregunta { get; set; } = null!;
    public string Respuesta { get; set; } = null!;
    public bool Estado { get; set; }
}