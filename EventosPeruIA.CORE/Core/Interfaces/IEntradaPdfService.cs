using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IEntradaPdfService
    {
        byte[] GenerarPdf(IReadOnlyCollection<EntradaDTO> entradas);
    }
}
