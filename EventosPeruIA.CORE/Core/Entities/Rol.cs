namespace EventosPeruIA.CORE.Core.Entities;

public partial class Rol
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = null!;

    public virtual ICollection<UsuarioRol> UsuarioRol { get; set; } = new List<UsuarioRol>();
}