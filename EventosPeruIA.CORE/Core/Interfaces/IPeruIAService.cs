using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IPeruIAService
    {
        Task<ResultadoDTO<PeruIAResponseDTO>> Conversar(PeruIARequestDTO dto);
    }
}
