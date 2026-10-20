namespace EventosPeruIA.CORE.Core.Entities;

public partial class Embedding
{
    public int IdEmbedding { get; set; }
    public string TipoOrigen { get; set; } = null!;
    public int IdOrigen { get; set; }
    public string Texto { get; set; } = null!;
    public string Vector { get; set; } = null!;
    public DateTime FechaIndexado { get; set; }
}