namespace EventosPeruIA.CORE.Core.Entities;

public partial class CheckIn
{
    public int IdCheckIn { get; set; }
    public int IdEntrada { get; set; }
    public int IdUsuarioValidador { get; set; }
    public DateTime FechaHora { get; set; }

    public virtual Entrada Entrada { get; set; } = null!;
    public virtual Usuario UsuarioValidador { get; set; } = null!;
}