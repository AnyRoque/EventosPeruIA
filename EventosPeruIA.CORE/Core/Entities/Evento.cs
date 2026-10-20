namespace EventosPeruIA.CORE.Core.Entities;

public partial class Evento
{
    public int IdEvento { get; set; }
    public int IdOrganizador { get; set; }
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string Departamento { get; set; } = null!;
    public string Distrito { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string? ImagenUrl { get; set; }
    public int AforoMaximo { get; set; }
    public int EntradasVendidas { get; set; }
    public decimal Precio { get; set; }
    public string Estado { get; set; } = null!;
    public string? MotivoCancelacion { get; set; }
    public DateTime? FechaCancelacion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public virtual Usuario Organizador { get; set; } = null!;
    public virtual Categoria Categoria { get; set; } = null!;
    public virtual ICollection<DetalleCompra> DetalleCompra { get; set; } = new List<DetalleCompra>();
    public virtual ICollection<Resena> Resena { get; set; } = new List<Resena>();
}