namespace EventosPeruIA.CORE.Core.Entities;

public partial class UsuarioRol
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public DateTime FechaAsignacion { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Rol Rol { get; set; } = null!;
}