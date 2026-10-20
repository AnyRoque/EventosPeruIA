namespace EventosPeruIA.CORE.Core.Entities;

public partial class Entrada
{
    public int IdEntrada { get; set; }
    public int IdDetalleCompra { get; set; }
    public Guid CodigoEntrada { get; set; }
    public int NumeroEntrada { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaGeneracion { get; set; }

    public virtual DetalleCompra DetalleCompra { get; set; } = null!;
    public virtual CheckIn? CheckIn { get; set; }
}