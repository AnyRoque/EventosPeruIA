namespace EventosPeruIA.CORE.Core.Entities;

public partial class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Evento> Evento { get; set; } = new List<Evento>();
}