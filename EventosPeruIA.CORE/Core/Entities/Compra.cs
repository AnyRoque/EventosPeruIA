namespace EventosPeruIA.CORE.Core.Entities;

public partial class Compra
{
    public int IdCompra { get; set; }
    public int IdUsuario { get; set; }
    public Guid CodigoOperacion { get; set; }
    public decimal MontoTotal { get; set; }
    public string? TitularTarjeta { get; set; }
    public string? UltimosDigitos { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaCompra { get; set; }
    public DateTime? FechaReembolso { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<DetalleCompra> DetalleCompra { get; set; } = new List<DetalleCompra>();
}