namespace EventosPeruIA.CORE.Core.Entities;

public partial class Notificacion
{
    public int IdNotificacion { get; set; }
    public int IdUsuario { get; set; }
    public int? IdEvento { get; set; }
    public string Tipo { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLectura { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Evento? Evento { get; set; }
}