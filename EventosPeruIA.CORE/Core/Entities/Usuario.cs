namespace EventosPeruIA.CORE.Core.Entities;

public partial class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Celular { get; set; } = null!;
    public DateOnly FechaNacimiento { get; set; }
    public string Departamento { get; set; } = null!;
    public string Distrito { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? NombreOrganizador { get; set; }
    public string Estado { get; set; } = null!;
    public string? MotivoSuspension { get; set; }
    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<UsuarioRol> UsuarioRol { get; set; } = new List<UsuarioRol>();
    public virtual ICollection<UsuarioInteres> UsuarioInteres { get; set; } = new List<UsuarioInteres>();
    public virtual ICollection<Evento> Evento { get; set; } = new List<Evento>();
    public virtual ICollection<Compra> Compra { get; set; } = new List<Compra>();
}