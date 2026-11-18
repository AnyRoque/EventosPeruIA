namespace EventosPeruIA.CORE.Core.DTOs
{
    public class ArchivoDTO
    {
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
        public string Nombre { get; set; } = string.Empty;
        public string TipoContenido { get; set; } = "application/pdf";
    }
}
