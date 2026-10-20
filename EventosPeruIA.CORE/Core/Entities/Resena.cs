namespace EventosPeruIA.CORE.Core.Entities;

public partial class Resena
{
    public int IdResena { get; set; }
    public int IdUsuario { get; set; }
    public int IdEvento { get; set; }
    public byte Calificacion { get; set; }
    public string Comentario { get; set; } = null!;
    public bool Visible { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Evento Evento { get; set; } = null!;
}