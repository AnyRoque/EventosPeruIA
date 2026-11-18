namespace EventosPeruIA.CORE.Core.DTOs
{
    public class ResumenCancelacionDTO
    {
        public int IdEvento { get; set; }
        public string TituloEvento { get; set; } = null!;
        public int EntradasVendidas { get; set; }
        public int TotalCompradores { get; set; }
        public decimal MontoADevolver { get; set; }
    }
}
