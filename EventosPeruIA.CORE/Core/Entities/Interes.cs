namespace EventosPeruIA.CORE.Core.Entities;

public partial class Interes
{
    public int IdInteres { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Estado { get; set; }

    public virtual ICollection<UsuarioInteres> UsuarioInteres { get; set; } = new List<UsuarioInteres>();
}