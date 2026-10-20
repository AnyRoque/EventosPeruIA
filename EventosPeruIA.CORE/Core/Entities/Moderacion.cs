namespace EventosPeruIA.CORE.Core.Entities;

public partial class Moderacion
{
    public int IdModeracion { get; set; }
    public int IdAdministrador { get; set; }
    public int? IdUsuarioAfectado { get; set; }
    public int? IdEventoAfectado { get; set; }
    public int? IdResenaAfectada { get; set; }
    public string Accion { get; set; } = null!;
    public string Motivo { get; set; } = null!;
    public DateTime FechaAccion { get; set; }

    public virtual Usuario Administrador { get; set; } = null!;
    public virtual Usuario? UsuarioAfectado { get; set; }
    public virtual Evento? EventoAfectado { get; set; }
    public virtual Resena? ResenaAfectada { get; set; }
}