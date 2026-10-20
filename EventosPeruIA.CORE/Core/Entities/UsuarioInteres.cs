namespace EventosPeruIA.CORE.Core.Entities;

public partial class UsuarioInteres
{
    public int IdUsuario { get; set; }
    public int IdInteres { get; set; }
    public DateTime FechaRegistro { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Interes Interes { get; set; } = null!;
}