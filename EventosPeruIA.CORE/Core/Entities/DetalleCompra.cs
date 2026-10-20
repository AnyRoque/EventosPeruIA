namespace EventosPeruIA.CORE.Core.Entities;

public partial class DetalleCompra
{
    public int IdDetalleCompra { get; set; }
    public int IdCompra { get; set; }
    public int IdEvento { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public virtual Compra Compra { get; set; } = null!;
    public virtual Evento Evento { get; set; } = null!;
    public virtual ICollection<Entrada> Entrada { get; set; } = new List<Entrada>();
}