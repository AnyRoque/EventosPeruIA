namespace EventosPeruIA.CORE.Core.Entities;

public partial class EventoVisita
{
    public int IdEventoVisita { get; set; }
    public int IdUsuario { get; set; }
    public int IdEvento { get; set; }
    public DateTime FechaVisita { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Evento Evento { get; set; } = null!;
}